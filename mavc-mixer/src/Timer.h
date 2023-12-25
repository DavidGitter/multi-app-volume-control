class Timer {
public:
  Timer();
  Timer(unsigned long msInterval);
  void start();
  void stop();
  bool isExpired();
  bool isRunning();
  void setInterval(long ms);
  
private:
  unsigned long startTime;
  unsigned long msInterval;
  bool running;
};