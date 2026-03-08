#pragma once
#include <Arduino.h>

#define ADC_MAX_VAL 4095 // 12-bit ESP32

/**
 * @file ADC.h
 * @brief ADC class for handling analog input and filtering on ESP32
 */
#pragma once
#include <Arduino.h>

#define ADC_MAX_VAL 4095 // 12-bit ESP32

/**
 * @class ADC
 * @brief Handles analog input, filtering, mapping, and hysteresis for potentiometers
 */
class ADC {
public:
    /**
     * @brief Default constructor
     */
    ADC();

    /**
     * @brief Construct ADC object
     * @param portPin GPIO pin number
     * @param map_max Maximum mapped value (default 100)
     * @param map_min Minimum mapped value (default 0)
     */
    ADC(int portPin, int map_max = 100, int map_min = 0);

    /**
     * @brief Get filtered and hysteresis value
     * @return Filtered value
     */
    int getValue();

    /**
     * @brief Get raw analog value
     * @return Raw value
     */
    int getRawValue();

    /**
     * @brief Get average raw value
     * @param sampleRate Number of samples
     * @return Averaged raw value
     */
    int getRawAvgValue(int sampleRate);

    /**
     * @brief Get average mapped value
     * @param sampleRate Number of samples
     * @return Averaged mapped value
     */
    int getAvgValue(int sampleRate);

    /**
     * @brief Get maximum raw value
     * @param sampleRate Number of samples
     * @return Maximum raw value
     */
    int getRawMaxValue(int sampleRate);

    /**
     * @brief Get maximum mapped value
     * @param sampleRate Number of samples
     * @return Maximum mapped value
     */
    int getMaxValue(int sampleRate);

    /**
     * @brief Get thresholded average mapped value
     * @param sampleRate Number of samples
     * @param threshold Threshold factor
     * @return Thresholded average value
     */
    int getThreshAvgValue(int sampleRate, float threshold);

    /**
     * @brief Get upper thresholded average mapped value
     * @param sampleRate Number of samples
     * @param threshold Threshold factor
     * @return Upper thresholded average value
     */
    int getUpperThreshAvgValue(int sampleRate, float threshold);

    /**
     * @brief Set output mapping range
     * @param map_min Minimum mapped value
     * @param map_max Maximum mapped value
     */
    void setOutputMapping(int map_min, int map_max);

    /**
     * @brief Get mapping maximum
     * @return Maximum mapped value
     */
    int getMappingMax();

    /**
     * @brief Get mapping minimum
     * @return Minimum mapped value
     */
    int getMappingMin();

private:
    /**
     * @brief Internal mapping function
     * @param val Value to map
     * @return Mapped value
     */
    int mapInternal(int val);

    /**
     * @brief Internal max function
     * @param v1 First value
     * @param v2 Second value
     * @return Maximum value
     */
    int maxInternal(int v1, int v2);

    int portPin;   ///< GPIO pin number
    int map_min;   ///< Minimum mapped value
    int map_max;   ///< Maximum mapped value
    int lastVal;   ///< Last mapped value used
    float filtVal; ///< Filtered raw value [0..4095]
};
