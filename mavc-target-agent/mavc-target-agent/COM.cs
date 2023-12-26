using System.IO.Ports;

/**
 * A simple but fast communication class for stream-like commands bidirectional
 */
class COM
{
    //the serial port for com.
    private SerialPort serialPort;

    //stores the callback function for the receiver
    private Action<Word> onReceiveFunc;

    /**
     * a class that expresses a word (command/data) in the com protocoll
     */
    public class Word
    {
        public Word(char action, String args)
        {
            this.action = action;
            this.args = args;
        }
        public char action;
        public String args;

        public override string ToString()
        {
            return action + "," + args + "#";
        }
    }

    /**
     * @param portName  the name of the port (std: COM3)
     * @param baudRate  the baud rate of the com. session
     */
    public COM(String portName = "COM3", int baudRate = 9600)
    {
        serialPort = new SerialPort(portName, baudRate);

        serialPort.DataBits = 8;
        serialPort.StopBits = StopBits.One;
        serialPort.Parity = Parity.None;
        //serialPort.DtrEnable = true;

        serialPort.Open();

        //set callback proxy function
        serialPort.DataReceived += receivingCallbackProxy;
    }

    /**
     * Returns the name of the port (e.g. "COM3")
     */
    public String GetPortName()
    {
        return serialPort.PortName;    
    }

    /**
     * Returns the baud rate of the communication session
     */
    public int GetBaudRate()
    {
        return serialPort.BaudRate;
    }

    /*** ########## SENDER */

    /**
    * Sends a word by its two propeties
    * 
    * @param action    a letter that is related to a action or data (freely chooseable)
    * @param args      the data in form of a string (formless)
    */
    public void sendCommand(char action, String args) {
        serialPort.Write(action + "," + args + "#");
    }

    /** Updates the currecnt values of all volumes 
    */
    public void updateVolumes()
    {
        sendCommand('A', "");
    }

    /**
    * Sends a word 
    * 
    * @param action    a letter that is related to a action or data (freely chooseable)
    * @param args      the data in form of a string (formless)
    */
    public  void sendCommand(Word w)
    {
        serialPort.Write(w.ToString());
    }

    /*** ########## RECEIVER */

    /**
    * A callback funtion that gets triggered on data input
    * 
    * @param sender    the sender of the data
    * @param e         additional event args
    */
    private void receivingCallbackProxy(object sender, SerialDataReceivedEventArgs e)
    {

        //Console.WriteLine("Info: Received data stream");
        while (serialPort.BytesToRead > 0)
        {
            char rbyte = (char)serialPort.ReadByte();
            String word = "";
            while (rbyte != '#')
            {
                word += rbyte;
                rbyte = (char)serialPort.ReadByte();
            }
            try
            {
                Word w = extractWord(word);
                onReceiveFunc(w);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    /**
     * Function to set the callback function on word receiving event
     * 
     * @param wordInterpreter   the function that interprets and actions on the received word(s)
     */
    public void OnWordStreamReceive(Action<Word> wordInterpreter)
    {
        this.onReceiveFunc = wordInterpreter;
    }

    /**
     * This function interprets a collected word and executes the action related to it
     * 
     * Example for a word: A,123#
     * A word consists of 
     *  - a char (action) that represents the action taken
     *  - the delimiter ','
     *  - a argument value for the action (volume in % as example)
     *  - a # seperator that delimits it from the next word
     *  
     *  With this schema the protocoll can stream data
     *  Example: A,45#B,54#U,2356#E,4353#.....
     *  
     *  @param word     the word to interpret
     *  @note you can extend the function with new actions for new features
     */
    private Word extractWord(String word)
    {
        char action = word.ElementAt(0);   
        char sep = word.ElementAt(1);
        String arg = word.Substring(2);

        if (!char.IsLetter(action))
            throw new InvalidDataException();
        if (!sep.Equals(','))
            throw new InvalidDataException();

        return new Word(action, arg);
    }

    public bool IsOpen()
    {
        return serialPort.IsOpen;
    }
}