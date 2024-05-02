#include "TemporalTresholdLock.h"

bool TemporalTresholdLock::isUnlocked(int val) {
    if(timer.isRunning() && !timer.isExpired()){
        initVal = val;
        return true;
    }
    timer.stop();
    if(val >= initVal + unlockDiff || val <= initVal - unlockDiff) {
        initVal = val;
        timer.start();
        return true;
    }
    else {
        return false;
    }
}

void TemporalTresholdLock::setUnlockDiff(int unlockDiff) {
    this->unlockDiff = unlockDiff;
}

void TemporalTresholdLock::setMsUnlocked(long msUnlocked){
    this->timer.setInterval(msUnlocked);
}