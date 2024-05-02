#include "Timer.h"
#include <Arduino.h>

Timer::Timer() {
  this->msInterval = 0;
  this->running = false;
}

Timer::Timer(unsigned long msInterval) {
  this->msInterval = msInterval;
  this->running = false;
}

void Timer::start() {
  this->startTime = millis();
  this->running = true;
}

void Timer::stop() {
  this->running = false;
}

bool Timer::isExpired() {
  if (running) {
    unsigned long elapsedTime = millis() - startTime;
    return (elapsedTime >= msInterval);
  }
  return true;
}

bool Timer::isRunning() {
    return running;
}

void Timer::setInterval(long ms) {
    this->msInterval = ms;
}