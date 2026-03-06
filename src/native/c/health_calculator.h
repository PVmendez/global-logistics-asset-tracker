#ifndef HEALTH_CALCULATOR_H
#define HEALTH_CALCULATOR_H

#ifdef __cplusplus
extern "C" {
#endif

/**
 * Health Calculator Library
 * 
 * This library provides low-level functions to calculate health scores
 * based on sensor data from logistics assets.
 */

/**
 * Sensor data structure
 */
typedef struct {
    float temperature;      // Celsius
    float humidity;         // Percentage (0-100)
    float pressure;         // PSI
    float vibration;        // mm/s
    float battery_level;    // Percentage (0-100), can be -1 if not available
} SensorData;

/**
 * Health score factors structure
 */
typedef struct {
    float temperature_score;
    float humidity_score;
    float pressure_score;
    float vibration_score;
} HealthFactors;

/**
 * Calculate individual health scores based on sensor readings.
 * 
 * Returns score between 0-100 for each sensor metric.
 * 100 = Optimal, 0 = Critical
 */
float calculate_temperature_health(float celsius);
float calculate_humidity_health(float humidity_pct);
float calculate_pressure_health(float pressure_psi);
float calculate_vibration_health(float vibration_mm_s);

/**
 * Calculate overall health score from sensor data.
 * 
 * @param sensor_data: Pointer to SensorData structure
 * @param factors: Pointer to HealthFactors structure to store component scores
 * @return Overall health score (0-100), or -1 if error
 */
float calculate_asset_health(const SensorData* sensor_data, HealthFactors* factors);

/**
 * Get health status classification from numeric score
 * 
 * @param score: Health score (0-100)
 * @return Status: 0 = CRITICAL, 1 = DEGRADED, 2 = HEALTHY
 */
int get_health_status(float score);

/**
 * Validate sensor data for reasonable ranges
 * 
 * @param sensor_data: Pointer to SensorData structure
 * @return 1 if valid, 0 if invalid
 */
int validate_sensor_data(const SensorData* sensor_data);

#ifdef __cplusplus
}
#endif

#endif // HEALTH_CALCULATOR_H
