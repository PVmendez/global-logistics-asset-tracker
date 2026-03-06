using System.Runtime.InteropServices;

namespace Lexart.GLAT.Api.Infrastructure.Native;

public static class HealthCalculatorInterop
{
    private const string LibraryNameLinux = "libhealthcalculator.so";
    private const string LibraryNameWindows = "healthcalculator.dll";
    private const string LibraryNameMac = "libhealthcalculator.dylib";

    private static readonly string LibraryName = GetLibraryName();

    private static string GetLibraryName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return LibraryNameWindows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return LibraryNameLinux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return LibraryNameMac;
        throw new PlatformNotSupportedException("Unsupported platform");
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SensorData
    {
        public float temperature;
        public float humidity;
        public float pressure;
        public float vibration;
        public float battery_level;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HealthFactors
    {
        public float temperature_score;
        public float humidity_score;
        public float pressure_score;
        public float vibration_score;
    }

    [DllImport("libhealthcalculator.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern float calculate_asset_health(in SensorData sensor_data, out HealthFactors factors);

    [DllImport("libhealthcalculator.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern int get_health_status(float score);

    [DllImport("libhealthcalculator.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern int validate_sensor_data(in SensorData sensor_data);

    public static (float score, Dictionary<string, float> factors)? CalculateAssetHealth(
        float temperature, float humidity, float pressure, float vibration, float batteryLevel = -1)
    {
        try
        {
            var sensorData = new SensorData
            {
                temperature = temperature,
                humidity = humidity,
                pressure = pressure,
                vibration = vibration,
                battery_level = batteryLevel
            };

            if (validate_sensor_data(sensorData) == 0)
            {
                return null;
            }

            float score = calculate_asset_health(sensorData, out var factors);

            if (score < 0)
                return null;

            var factorDict = new Dictionary<string, float>
            {
                { "temperature", factors.temperature_score },
                { "humidity", factors.humidity_score },
                { "pressure", factors.pressure_score },
                { "vibration", factors.vibration_score }
            };

            return (score, factorDict);
        }
        catch (DllNotFoundException)
        {
            return CalculateFallback(temperature, humidity, pressure, vibration);
        }
    }

    public static string GetHealthStatus(float score)
    {
        try
        {
            int status = get_health_status(score);
            return status switch
            {
                0 => "CRITICAL",
                1 => "DEGRADED",
                2 => "HEALTHY",
                _ => "UNKNOWN"
            };
        }
        catch (DllNotFoundException)
        {
            return ManualGetHealthStatus(score);
        }
    }

    private static (float score, Dictionary<string, float>) CalculateFallback(
        float temperature, float humidity, float pressure, float vibration)
    {
        var tempScore = temperature >= 15 && temperature <= 25 ? 100f : 
                       temperature < 15 ? Math.Max(0, 50 + (temperature - 10) * 10) :
                       Math.Max(0, 100 - (temperature - 25) * 10);

        var humidityScore = humidity >= 40 && humidity <= 60 ? 100f :
                           humidity < 40 ? Math.Max(0, 50 + (humidity - 30) * 5) :
                           Math.Max(0, 100 - (humidity - 60) * 5);

        var pressureScore = pressure >= 14.5 && pressure <= 15.5 ? 100f :
                           pressure < 14.5 ? Math.Max(0, 50 + (pressure - 14.0f) * 100) :
                           Math.Max(0, 100 - (pressure - 15.5f) * 100);

        var vibrationScore = vibration <= 2 ? 100f :
                            vibration <= 5 ? Math.Max(0, 100 - (vibration - 2) * 16.67f) :
                            0f;

        float overall = (tempScore * 0.3f) + (humidityScore * 0.25f) + 
                       (pressureScore * 0.25f) + (vibrationScore * 0.2f);

        overall = Math.Clamp(overall, 0, 100);

        var factors = new Dictionary<string, float>
        {
            { "temperature", tempScore },
            { "humidity", humidityScore },
            { "pressure", pressureScore },
            { "vibration", vibrationScore }
        };

        return (overall, factors);
    }

    private static string ManualGetHealthStatus(float score)
    {
        return score >= 70 ? "HEALTHY" :
               score >= 40 ? "DEGRADED" :
               "CRITICAL";
    }
}
