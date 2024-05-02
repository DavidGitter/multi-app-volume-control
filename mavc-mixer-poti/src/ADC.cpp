#include "ADC.h"

ADC::ADC(int portPin, int map_max, int map_min){
        this->map_min = map_min;
        this->map_max = map_max;
        this->portPin = portPin;
        this->lastVal = -1;
}

int ADC::map(int val) {
    return (val * (map_max - map_min) / ADC_MAX_VAL) + map_min;
}

int ADC::max(int v1, int v2) {
    return v1 > v2 ? v1 : v2;
}

int ADC::getValue() {
    return map(analogRead(portPin));
}

int ADC::getRawValue() {
    return analogRead(portPin);
}

int ADC::getRawAvgValue(int sampleRate) {
    int sum = 0;
    int i;
    //avg
    for(i=0;i<sampleRate; i++)
        sum += getRawValue();
    return sum / sampleRate;
}

int ADC::getAvgValue(int sampleRate) {
    return map(getRawAvgValue(sampleRate));
}

int ADC::getRawMaxValue(int sampleRate) {
    int maxv = 0;
    int i;
    //avg
    for(i=0;i<sampleRate; i++)
        maxv = max(getRawValue(), maxv);
    return maxv / sampleRate;
}

int ADC::getMaxValue(int sampleRate) {
    return map(getRawMaxValue(sampleRate));
}

int ADC::getThreshAvgValue(int sampleRate, float threshold) {
    //avg
    int avg = getRawAvgValue(sampleRate);
    int lval = lastVal;
    if(lval > -1) {
        if(!(abs(avg - lval) > map_max * threshold)){
            //treshold not reached
            return lval;
        }
        else{ //treshold reached, change value
            lastVal = avg;
            return avg;
        }
    }
    //first use of treshold -> return avg
    return map(avg);
}

int ADC::getUpperThreshAvgValue(int sampleRate, float threshold){
    int i;
    int maxv = 0;
    for(i=0; i<sampleRate; i++) {
        maxv = max(getThreshAvgValue(sampleRate, threshold), maxv);
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