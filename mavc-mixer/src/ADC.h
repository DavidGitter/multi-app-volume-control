#pragma once
#include <Arduino.h>

#define ADC_MAX_VAL 4095 // 12-bit ESP32

class ADC {
public:
    ADC(int portPin, int map_max = 100, int map_min = 0);

    int getValue(); // filtered + hysteresis
    int getRawValue();
    int getRawAvgValue(int sampleRate);
    int getAvgValue(int sampleRate);
    int getRawMaxValue(int sampleRate);
    int getMaxValue(int sampleRate);

    int getThreshAvgValue(int sampleRate, float threshold);
    int getUpperThreshAvgValue(int sampleRate, float threshold);

    void setOutputMapping(int map_min, int map_max);
    int getMappingMax();
    int getMappingMin();

private:
    int mapInternal(int val); // renamed to avoid confusion
    int maxInternal(int v1, int v2);

    int portPin;
    int map_min;
    int map_max;

    int lastVal;   // last *mapped* value used
    float filtVal; // filtered raw value [0..4095]
};
