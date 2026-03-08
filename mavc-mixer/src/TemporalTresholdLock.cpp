#include "TemporalTresholdLock.h"

/**
 * @brief Checks if the lock is unlocked based on value and threshold
 * @param val Current value to check
 * @return True if unlocked, false otherwise
 */
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

/**
 * @brief Sets the unlock difference threshold
 * @param unlockDiff Difference threshold for unlocking
 */
void TemporalTresholdLock::setUnlockDiff(int unlockDiff) {
    this->unlockDiff = unlockDiff;
}

/**
 * @brief Sets the unlock duration in milliseconds
 * @param msUnlocked Duration in milliseconds
 */
void TemporalTresholdLock::setMsUnlocked(long msUnlocked){
    this->timer.setInterval(msUnlocked);
}