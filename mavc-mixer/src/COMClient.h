#include <Arduino.h>

class COMClient{
public:
    enum Action {
        VOL1 = (int)'A', 
        VOL2 = (int)'B', 
        VOL3 = (int)'C', 
        VOL4 = (int)'D'
    };

    struct Command{
        char action;
        String args;
    };

    inline char getChar(Action section)
    {
        return (char)section;
    }

    COMClient(String com = "COM3", int baudRate = 9600) {
        Serial.begin(baudRate);  // Start the user-defined serial port
    }

    void sendCommand(Action a, String args) {
        Serial.print(String(getChar(a)) + "," + (String)args + "#");
    }

    void sendCommand(char a, String args) {
        Serial.print(String(a) + "," + (String)args + "#");
    }

    Command readCommand() {
        Command c;
        char read = Serial.read();
        if(read == '#')
            read = Serial.read();

        c.action = read;
        if(Serial.read() != ',') {
            while(Serial.read() != '#'){}
            throw std::invalid_argument("received invalid command");
        } else {
            while((read = Serial.read()) != '#') {
                c.args += read;
            }
            return c;
        }
    }

    bool receivedCommand() {
        return Serial.available();
    }

};