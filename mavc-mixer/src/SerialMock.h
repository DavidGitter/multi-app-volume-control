#ifndef SERIAL_MOCK_H
#define SERIAL_MOCK_H

#include <Arduino.h>

class SerialMock {
  private:
    const char* buffer;
    int index;
    int length;

  public:
    // Konstruktor
    SerialMock(const char* data);

    // verfügbare Bytes
    int available();

    // liest ein Zeichen (wie Serial.read)
    int read();

    // optional: zurücksetzen
    void reset();

    // optional: alles als String lesen
    String readString();
};

#endif