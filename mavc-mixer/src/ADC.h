#include <Arduino.h>
#include <vector>

#ifndef ADC_MAX_VAL
    #define ADC_MAX_VAL 4095
#endif

class ADC {
public:
    ADC(int portPin, int map_max=ADC_MAX_VAL, int map_min=0);

    int getValue();
    int getRawValue();
    int getMaxValue(int sampleRate);
    int getAvgValue(int sampleRate);
    int getRawMaxValue(int sampleRate);
    int getRawAvgValue(int sampleRate);
    int getThreshAvgValue(int sampleRate, float threshold);
    int getUpperThreshAvgValue(int sampleRate, float threshold);

    void setOutputMapping(int map_min, int map_max);
    int getMappingMax();
    int getMappingMin();

private:
    int portPin, map_min, map_max, lastVal;

    int map(int val);
    int max(int v1, int v2);
};

