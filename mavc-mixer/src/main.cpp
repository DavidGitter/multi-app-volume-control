#include <Arduino.h>
#include "COMClient.h"
#include "ADC.h"
#include "TemporalTresholdLock.h"

COMClient cc("COM3", 9600);

#define POTI1 36
#define POTI2 39
#define POTI3 34
#define POTI4 35

#define POTI_COUNT 4



// #define ADC_MAX_VAL 2048

TemporalTresholdLock tta[4]; // for preventing poti flickering
int lastVol[4];

void setup() {
  analogReadResolution(12);
  int i=0;
  for(i; i<4; i++){
    tta[i].setMsUnlocked(2000);
    tta[i].setUnlockDiff(2);
  }
}

ADC potis[POTI_COUNT] = {ADC(POTI1, 100),ADC(POTI2, 100),ADC(POTI3, 100),ADC(POTI4, 100)};

void sendVolume(int potiNum, char action, int volume) {
  if(tta[potiNum-1].isUnlocked(volume) && lastVol[potiNum-1] != volume) {
    lastVol[potiNum-1] = volume;
    cc.sendCommand(action, String(volume));
  }
}

void sendVolumes() {
  int i, potInitValue;
  for(i=0; i<POTI_COUNT; i++){
    potInitValue = potis[i].getValue();
    cc.sendCommand(((char)(65+i)), String(potInitValue));
  }
}

void onReceive(COMClient::Command c) {
  char action = c.action;
  switch(action){
    case 'A':
      sendVolumes();
      break;
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
    for(e=0; e<POTI_COUNT; e++) {
      potValue = potis[e].getValue();
      sendVolume(e+1, ((char)(65+e)), potValue);
    }

    if(cc.receivedCommand()) {
      COMClient::Command c = cc.readCommand();
      Serial.println("Received action " + c.action);
      onReceive(c);
    }


    delay(40);
  }
}
