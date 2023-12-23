#include <Arduino.h>
#include <COMClient.h>
#include <ADC.h>

COMClient cc("COM3", 9600);

#define POTI1 36
#define POTI2 39
#define POTI3 34
#define POTI4 35

#define POTI_COUNT 4

// #define ADC_MAX_VAL 2048

void setup() {
  analogReadResolution(12);
}

int lastVal[4];
ADC potis[POTI_COUNT] = {ADC(POTI1, 100),ADC(POTI2, 100),ADC(POTI3, 100),ADC(POTI4, 100)};

void sendVolume(int potiNum, char action, int volume) {
  if(lastVal[potiNum-1] != volume) {
    lastVal[potiNum-1] = volume;
    cc.sendCommand(action, String(volume));
  }
}

void loop() {
  // send poti init states once
  int i, potInitValue;
  for(i=0; i<POTI_COUNT; i++){
    potInitValue = potis[i].getUpperThreshAvgValue(10, 0.5f);
    cc.sendCommand(((char)(65+i)), String(potInitValue));
  }

  while(1==1){

    int e, potValue;
    for(e=0; e<POTI_COUNT; e++){
      potValue = potis[e].getUpperThreshAvgValue(10, 0.5f);
      sendVolume(e+1, ((char)(65+e)), potValue);
    }

    delay(20);
  }
}