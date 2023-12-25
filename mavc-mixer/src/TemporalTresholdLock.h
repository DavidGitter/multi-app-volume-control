#include "Timer.h"

/**
 * @brief A lock that stays unlocked for "msUnlocked" seconds when the difference is over a specified threshold "unlockDiff"
 * 
 */
class TemporalTresholdLock {
public:
    TemporalTresholdLock() : timer(0), unlockDiff(0), initVal(-999) {}
    TemporalTresholdLock(int unlockDiff, long msUnlocked) : unlockDiff(unlockDiff), initVal(-999)  {
        timer.setInterval(msUnlocked);
    }

    /**
     * @brief checks if its unlocked or if the passed value unlocks the lock
     * 
     * @param val       current value to check the lock against
     * @return true     if the lock is unlocked
     * @return false    if the lock is locked
     */
    bool isUnlocked(int val);

    /**
     * @brief set the difference needed to unlock
     * 
     * @param unlockDiff    unlock difference
     */
    void setUnlockDiff(int unlockDiff);

    /**
     * @brief sets the time the lock is unlocked until it closes again
     * 
     * @param msUnlocked 
     */
    void setMsUnlocked(long msUnlocked);

private:
    int initVal;
    int unlockDiff;
    Timer timer;
};