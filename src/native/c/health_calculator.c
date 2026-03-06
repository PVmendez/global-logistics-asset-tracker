#include "health_calculator.h"
#include <math.h>
#include <stddef.h>

/**
 * Calculate temperature health score
 * Optimal range: 15-25°C
 * Warning: 10-15°C or 25-30°C
 * Critical: <10°C or >30°C
 */
float calculate_temperature_health(float celsius) {
    if (celsius < 0 || celsius > 50) {
        return 0.0f;  // Critical
    }
    
    if (celsius >= 15 && celsius <= 25) {
        return 100.0f;  // Optimal
    }
    
    if (celsius >= 10 && celsius < 15) {
        return 50.0f + (celsius - 10) * 10.0f;  // 50-100
    }
    
    if (celsius > 25 && celsius <= 30) {
        return 100.0f - (celsius - 25) * 10.0f;  // 100-50
    }
    
    if (celsius < 10 || celsius > 30) {
        return 20.0f + (30.0f - fabs(celsius - 20.0f)) * 2.67f;
    }
    
    return 50.0f;  // Default fallback
}

/**
 * Calculate humidity health score
 * Optimal range: 40-60%
 * Warning: 30-40% or 60-70%
 * Critical: <30% or >70%
 */
float calculate_humidity_health(float humidity_pct) {
    if (humidity_pct < 0 || humidity_pct > 100) {
        return 0.0f;  // Critical
    }
    
    if (humidity_pct >= 40 && humidity_pct <= 60) {
        return 100.0f;  // Optimal
    }
    
    if (humidity_pct >= 30 && humidity_pct < 40) {
        return 50.0f + (humidity_pct - 30) * 5.0f;  // 50-100
    }
    
    if (humidity_pct > 60 && humidity_pct <= 70) {
        return 100.0f - (humidity_pct - 60) * 5.0f;  // 100-50
    }
    
    if (humidity_pct < 30 || humidity_pct > 70) {
        return 20.0f;  // Critical threshold
    }
    
    return 50.0f;
}

/**
 * Calculate pressure health score
 * Optimal range: 14.5-15.5 PSI
 * Warning: 14.0-14.5 PSI or 15.5-16.0 PSI
 * Critical: <14.0 PSI or >16.0 PSI
 */
float calculate_pressure_health(float pressure_psi) {
    if (pressure_psi < 13.0f || pressure_psi > 17.0f) {
        return 0.0f;  // Critical
    }
    
    if (pressure_psi >= 14.5f && pressure_psi <= 15.5f) {
        return 100.0f;  // Optimal
    }
    
    if (pressure_psi >= 14.0f && pressure_psi < 14.5f) {
        return 50.0f + (pressure_psi - 14.0f) * 100.0f;  // 50-100
    }
    
    if (pressure_psi > 15.5f && pressure_psi <= 16.0f) {
        return 100.0f - (pressure_psi - 15.5f) * 100.0f;  // 100-50
    }
    
    if (pressure_psi >= 13.0f && pressure_psi < 14.0f) {
        return 20.0f + (pressure_psi - 13.0f) * 30.0f;  // 20-50
    }
    
    if (pressure_psi > 16.0f && pressure_psi <= 17.0f) {
        return 20.0f + (17.0f - pressure_psi) * 20.0f;  // 20-40
    }
    
    return 50.0f;
}

/**
 * Calculate vibration health score
 * Optimal range: 0-2 mm/s
 * Warning: 2-5 mm/s
 * Critical: >5 mm/s
 */
float calculate_vibration_health(float vibration_mm_s) {
    if (vibration_mm_s < 0) {
        return 0.0f;  // Invalid
    }
    
    if (vibration_mm_s <= 2.0f) {
        return 100.0f;  // Optimal
    }
    
    if (vibration_mm_s > 2.0f && vibration_mm_s <= 5.0f) {
        return 100.0f - (vibration_mm_s - 2.0f) * 16.67f;  // 100-50
    }
    
    if (vibration_mm_s > 5.0f && vibration_mm_s <= 8.0f) {
        return 50.0f - (vibration_mm_s - 5.0f) * 16.67f;  // 50-0
    }
    
    return 0.0f;  // Critical
}

/**
 * Calculate overall asset health score
 * Weighted average: 30% Temperature + 25% Humidity + 25% Pressure + 20% Vibration
 */
float calculate_asset_health(const SensorData* sensor_data, HealthFactors* factors) {
    if (sensor_data == NULL || factors == NULL) {
        return -1.0f;  // Error
    }
    
    // Validate input data
    if (!validate_sensor_data(sensor_data)) {
        return -1.0f;
    }
    
    // Calculate individual health scores
    factors->temperature_score = calculate_temperature_health(sensor_data->temperature);
    factors->humidity_score = calculate_humidity_health(sensor_data->humidity);
    factors->pressure_score = calculate_pressure_health(sensor_data->pressure);
    factors->vibration_score = calculate_vibration_health(sensor_data->vibration);
    
    // Calculate weighted average
    float overall_health = 
        (factors->temperature_score * 0.30f) +
        (factors->humidity_score * 0.25f) +
        (factors->pressure_score * 0.25f) +
        (factors->vibration_score * 0.20f);
    
    // Clamp to 0-100 range
    if (overall_health < 0.0f) overall_health = 0.0f;
    if (overall_health > 100.0f) overall_health = 100.0f;
    
    return overall_health;
}

/**
 * Get health status classification
 */
int get_health_status(float score) {
    if (score >= 70.0f) {
        return 2;  // HEALTHY
    } else if (score >= 40.0f) {
        return 1;  // DEGRADED
    } else {
        return 0;  // CRITICAL
    }
}

/**
 * Validate sensor data ranges
 */
int validate_sensor_data(const SensorData* sensor_data) {
    if (sensor_data == NULL) {
        return 0;
    }
    
    // Check temperature range (-50 to +60 Celsius)
    if (sensor_data->temperature < -50.0f || sensor_data->temperature > 60.0f) {
        return 0;
    }
    
    // Check humidity range (0-100%)
    if (sensor_data->humidity < 0.0f || sensor_data->humidity > 100.0f) {
        return 0;
    }
    
    // Check pressure range (0-25 PSI)
    if (sensor_data->pressure < 0.0f || sensor_data->pressure > 25.0f) {
        return 0;
    }
    
    // Check vibration range (0-20 mm/s)
    if (sensor_data->vibration < 0.0f || sensor_data->vibration > 20.0f) {
        return 0;
    }
    
    // Battery level is optional, but if provided must be 0-100%
    if (sensor_data->battery_level != -1.0f) {
        if (sensor_data->battery_level < 0.0f || sensor_data->battery_level > 100.0f) {
            return 0;
        }
    }
    
    return 1;  // Valid
}
