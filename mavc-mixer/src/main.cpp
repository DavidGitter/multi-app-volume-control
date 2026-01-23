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

TemporalTresholdLock tta[POTI_COUNT];
int lastVol[POTI_COUNT] = { -1, -1, -1, -1 };

ADC potis[POTI_COUNT] = {
    ADC(POTI1, 100),
    ADC(POTI2, 100),
    ADC(POTI3, 100),
    ADC(POTI4, 100) };

void setup() {
  analogReadResolution(12);

  for (int i = 0; i < POTI_COUNT; i++) {
    tta[i].setMsUnlocked(2000);
    tta[i].setUnlockDiff(2);
  }
}

void sendVolume(int potiNum, char action, int volume) {
  if (tta[potiNum - 1].isUnlocked(volume) && lastVol[potiNum - 1] != volume) {
    lastVol[potiNum - 1] = volume;
    cc.sendCommand(action, String(volume));
  }
}

void sendVolumes() {
  for (int i = 0; i < POTI_COUNT; i++) {
    int potInitValue = potis[i].getValue();   // filtered
    cc.sendCommand((char)(65 + i), String(potInitValue));
  }
}

/* void onReceive(COMClient::Command c){
  char action = c.action;
  switch (action) {
  case 'A':
    sendVolumes();
    break;
  }
} */

void loop() {
  // initial states once
  sendVolumes();

  while (true) {
    for (int e = 0; e < POTI_COUNT; e++) {
      int potValue = potis[e].getValue();   // filtered + hysteresis
      sendVolume(e + 1, (char)(65 + e), potValue);
    }

    /* if (cc.receivedCommand()) {
      COMClient::Command c = cc.readCommand();
      onReceive(c);
    } */

    delay(20);
  }
}
