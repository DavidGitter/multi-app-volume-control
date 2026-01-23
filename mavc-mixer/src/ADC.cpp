#include "ADC.h"

ADC::ADC(int portPin, int map_max, int map_min) {
    this->map_min = map_min;
    this->map_max = map_max;
    this->portPin = portPin;
    this->lastVal = -1;
    this->filtVal = -1;
}

int ADC::mapInternal(int val) {
    int mapped = (val * (map_max - map_min + 1) / ADC_MAX_VAL) + map_min;
    if (mapped < map_min)  mapped = map_min;
    if (mapped > map_max)  mapped = map_max;
    return mapped;
}

int ADC::maxInternal(int v1, int v2) {
    return v1 > v2 ? v1 : v2;
}

int ADC::getRawValue() {
    return analogRead(portPin);
}

int ADC::getValue() {
    // exponential moving average on RAW value
    const float alpha = 0.25f;
    int raw = getRawValue();

    if (filtVal < 0) {
        filtVal = raw;
    }
    else {
        filtVal = filtVal + alpha * (raw - filtVal);
    }

    int mapped = mapInternal((int)filtVal);

    // hysteresis in mapped domain (0..100)
    const int hysteresis = 1;
    if (lastVal < 0 || abs(mapped - lastVal) >= hysteresis) {
        lastVal = mapped;
    }
    return lastVal;
}

int ADC::getRawAvgValue(int sampleRate) {
    long sum = 0;
    for (int i = 0; i < sampleRate; i++) {
        sum += getRawValue();
    }
    return (int)(sum / sampleRate);
}

int ADC::getAvgValue(int sampleRate) {
    return mapInternal(getRawAvgValue(sampleRate));
}

int ADC::getRawMaxValue(int sampleRate) {
    int maxv = 0;
    for (int i = 0; i < sampleRate; i++) {
        maxv = maxInternal(getRawValue(), maxv);
    }
    // <- no division here, it's already a max, not a sum
    return maxv;
}

int ADC::getMaxValue(int sampleRate) {
    return mapInternal(getRawMaxValue(sampleRate));
}

int ADC::getThreshAvgValue(int sampleRate, float threshold) {
    int avgRaw = getRawAvgValue(sampleRate);
    int avgMapped = mapInternal(avgRaw);

    int lval = lastVal;
    if (lval > -1) {
        int diff = abs(avgMapped - lval);
        if (diff <= (int)((map_max - map_min) * threshold)) {
            return lval;
        }
        else {
            lastVal = avgMapped;
            return avgMapped;
        }
    }
    lastVal = avgMapped;
    return avgMapped;
}

int ADC::getUpperThreshAvgValue(int sampleRate, float threshold) {
    int maxv = 0;
    for (int i = 0; i < sampleRate; i++) {
        int v = getThreshAvgValue(1, threshold); // 1 sample inside; outer loop decides max
        maxv = maxInternal(v, maxv);
    }
    return maxv;
}

void ADC::setOutputMapping(int map_min, int map_max) {
    this->map_min = map_min;
    this->map_max = map_max;
}

int ADC::getMappingMax() {
    return map_max;
}

int ADC::getMappingMin() {
    return map_min;
}
