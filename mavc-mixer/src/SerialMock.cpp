#include "SerialMock.h"

SerialMock::SerialMock(const char* data) {
  buffer = data;
  index = 0;
  length = strlen(data);
}

int SerialMock::available() {
  return length - index;
}

int SerialMock::read() {
  if (index >= length) return -1;
  return buffer[index++];
}

void SerialMock::reset() {
  index = 0;
}

String SerialMock::readString() {
  String result = "";
  while (available()) {
    result += (char)read();
  }
  return result;
}