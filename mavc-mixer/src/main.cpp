#include <Arduino.h>
#include "COMClient.h"
#include "ADC.h"
#include "TemporalTresholdLock.h"

COMClient cc("COM3", 9600);

/**
 * @brief List of possible pins for potentiometers
 * @note Recommended ADC pins for ESP32 potentiometers:
 *   ADC1 (safe with Wi-Fi): 32, 33, 34, 35, 36 (VP), 39 (VN)
 *   ADC2 (not safe with Wi-Fi): 0, 2, 4, 12, 13, 14, 15, 25, 26, 27
 */
int potiPins[] = {36, 39, 34, 35, 32};
const int POTI_COUNT = sizeof(potiPins) / sizeof(potiPins[0]); // Hardcoded for now

TemporalTresholdLock tta[POTI_COUNT];
int lastVol[POTI_COUNT];
ADC* potis[POTI_COUNT];

/**
 * @brief Arduino setup function
 * Initializes analog read resolution, potentiometer locks, last values, and ADC objects.
 */
void setup() {
  analogReadResolution(12);

  for (int i = 0; i < POTI_COUNT; i++) {
    tta[i].setMsUnlocked(2000);
    tta[i].setUnlockDiff(2);
    lastVol[i] = -1;
    potis[i] = new ADC(potiPins[i], 100);
  }
}

/**
 * @brief Sends volume value for a specific potentiometer if unlocked and changed
 * @param potiNum Index of potentiometer (0-based)
 * @param action Character representing the potentiometer (e.g., 'A', 'B', ...)
 * @param volume Current volume value to send
 */
void sendVolume(int potiNum, char action, int volume) {
  if (tta[potiNum].isUnlocked(volume) && lastVol[potiNum] != volume) {
    lastVol[potiNum] = volume;
    cc.sendCommand(action, String(volume));
  }
}

/**
 * @brief Sends initial volume values for all potentiometers
 */
void sendVolumes() {
  for (int i = 0; i < POTI_COUNT; i++) {
    int potInitValue = potis[i]->getValue();   // filtered
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

/**
 * @brief Arduino main loop
 * Continuously reads potentiometer values and sends updates if changed
 */
void loop() {
  // initial states once
  sendVolumes();

  while (true) {
    for (int e = 0; e < POTI_COUNT; e++) {
      int potValue = potis[e]->getValue();   // filtered + hysteresis
      sendVolume(e, (char)(65 + e), potValue);
    }

    /* if (cc.receivedCommand()) {
      COMClient::Command c = cc.readCommand();
      onReceive(c);
    } */

    delay(20);
  }
}
