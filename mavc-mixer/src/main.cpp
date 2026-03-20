#include <Arduino.h>
#include "COMClient.h"
#include "ADC.h"
#include "TemporalTresholdLock.h"

COMClient cc("COM3", 115200);

#define PIN_MAX_COUNT 16


void setup() {
  delay(2000);
  analogReadResolution(12);
}

String removeChar(String &s, char c) {
  String result = "";             // neues Ergebnis
  for (int i = 0; i < s.length(); i++) {
    if (s[i] != c) {
      result += s[i];             // nur hinzufügen, wenn nicht der zu entfernende Char
    }
  }
  return result;                     // String überschreiben
}

// Arduino String split Function (removed delimiter)
int splitString(String str, char delimiter, String* arr, int maxParts=PIN_MAX_COUNT) {
  int partCount = 0;
  int start = 0;
  int end = str.indexOf(delimiter);

  while (end != -1 && partCount < maxParts) {
    arr[partCount++] = str.substring(start, end); // delimiter nicht mitnehmen
    start = end + 1; // direkt nach dem delimiter weitermachen
    end = str.indexOf(delimiter, start);
  }

  // letzten Teil nur hinzufügen, wenn er nicht leer ist
  if (start < str.length() && partCount < maxParts) {
    arr[partCount++] = str.substring(start);
  }

  return partCount; // Anzahl der Teile
}


int configuratePinMappings(COMClient::Command c, ADC* potis) { // returns the number of knobs the mixer has
  String parts[PIN_MAX_COUNT]; // max 5 Teile
  String input = c.args;
  input.trim();
  int foundCount = splitString(input, '.', parts);

  for(int i=0; i<foundCount; i++){
    parts[i] = removeChar(parts[i], '.');
    potis[i].setPortPin(parts[i].toInt());
    cc.sendCommand('Q', "Mapped Pin " + parts[i] + " to Poti " + String(i+1));
  }

  return foundCount;
}

void sendVolume(int potiNum, char action, int volume, TemporalTresholdLock* tta, int* lastVol);
void sendVolumes(ADC* potis);

void loop() {
  // initial states once

  while(!Serial){delay(500);} // waiting for agent to connect

  while(!Serial.available()){ // wait for initial mixer configurations
      cc.sendCommand('Q', "Waiting for initial agent configuration...");
      delay(2000);
  }

  
  // init potis and filters
  TemporalTresholdLock tta[PIN_MAX_COUNT];
  int lastVol[PIN_MAX_COUNT];
  ADC potis[PIN_MAX_COUNT];

  // read configurations
  delay(200);
  COMClient::Command pimaps = cc.readCommand();
  cc.sendCommand('Q', "Configurating pin mappings...");
  int potiCount = configuratePinMappings(pimaps, potis);
  cc.sendCommand('Q', "Found " + String(potiCount) + " potis.");

  for (int i = 0; i < PIN_MAX_COUNT; i++) {
    tta[i].setMsUnlocked(2000);
    tta[i].setUnlockDiff(2);
  }

  // sendVolumes(potis);

  while (true) {
    for (int e = 0; e < potiCount; e++) {
      int potValue = potis[e].getValue();   // filtered + hysteresis
      sendVolume(e + 1, (char)(65 + e), potValue, tta, lastVol);
    }

    if(!cc.isConnected())
      throw "Agent disconnected"; // restart mixer

    delay(20);

    // if(cc.availableCommand()){ // mixer got restart command from agent
    //   COMClient::Command agentCommand = cc.readCommand();
    //   if(agentCommand.action == 'Z'){
    //     cc.sendCommand('Q', "Mixer: Got restart command form agent. Restarting mixer...");
    //     delay(50);
    //     return;
    //   } else if(agentCommand.action == 'V') {
    //     cc.sendCommand('Q', "Mixer: Received new pin mappings...");
    //     potiCount = configuratePinMappings(agentCommand, potis);
    //     cc.sendCommand('Q', "Found " + String(potiCount) + " potis.");
    //   }
    //   else {
    //     cc.sendCommand('Q', "Mixer: Received unknown command from agent.");
    //   }
    // }
  }
}


void sendVolume(int potiNum, char action, int volume, TemporalTresholdLock* tta, int* lastVol) {
  if (tta[potiNum - 1].isUnlocked(volume) && lastVol[potiNum - 1] != volume) {
    lastVol[potiNum - 1] = volume;
    cc.sendCommand(action, String(volume));
  }
}

void sendVolumes(ADC* potis) {
  for (int i = 0; i < PIN_MAX_COUNT; i++) {
    int potInitValue = potis[i].getValue();   // filtered
    cc.sendCommand((char)(65 + i), String(potInitValue));
  }
}