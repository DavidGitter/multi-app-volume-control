using System.IO.Ports;

/**
 * A simple but fast communication class for stream-like commands bidirectional
 */
class COM
{
    // the serial port for com.
    private SerialPort serialPort;

    // stores the callback function for the receiver
    private Action<Word> onReceiveFunc;

    // stores the callback function for logging errors
    private Action<string> onErrorLogFunc;

    /** Fired on any send or receive port error so the caller can tear down and reconnect. */
    public event Action OnDisconnected;

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
        serialPort.DtrEnable = true;

        // Caps how long ReadByte() waits for the next byte.
        // Without this, a frame that never receives its '#' terminator
        // blocks the DataReceived thread permanently.
        serialPort.ReadTimeout = 500;

        serialPort.Open();

        // set callback proxy function
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
    public void sendCommand(char action, String args)
    {
        try
        {
            serialPort.Write(action + "," + args + "#");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            onErrorLogFunc?.Invoke($"Send failed ({ex.GetType().Name}): {ex.Message}");
            OnDisconnected?.Invoke();
        }
    }

    /**
     * Updates the currecnt values of all volumes
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
    public void sendCommand(Word w)
    {
        // routes through sendCommand(char, string) so error handling applies to both overloads
        sendCommand(w.action, w.args);
    }

    /**
     * A callback funtion that gets triggered on data input.
     * Reads incoming bytes into words delimited by '#' and dispatches them
     * to the registered word handler.
     * A 500 ms ReadTimeout is enforced per byte; if a frame is truncated or
     * corrupt the partial word is discarded and the thread returns cleanly so
     * the next DataReceived event can start fresh.
     *
     * @param sender    the sender of the data
     * @param e         additional event args
     */
    private void receivingCallbackProxy(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            while (serialPort.BytesToRead > 0)
            {
                char rbyte;
                try
                {
                    rbyte = (char)serialPort.ReadByte();
                }
                catch (TimeoutException)
                {
                    return;
                }

                string word = "";
                while (rbyte != '#')
                {
                    word += rbyte;
                    try
                    {
                        rbyte = (char)serialPort.ReadByte();
                    }
                    catch (TimeoutException)
                    {
                        // Partial or corrupt frame — no '#' arrived within the timeout.
                        // Discard the fragment, log it for diagnostics, and stop processing
                        // this burst. The next DataReceived event starts from a clean state.
                        onErrorLogFunc?.Invoke(
                            $"Read timeout mid-frame, discarding partial word '{word}' " +
                            $"({word.Length} chars, bytes: {string.Join(" ",
                                System.Text.Encoding.ASCII.GetBytes(word).Select(b => b.ToString("X2")))})");
                        return;
                    }
                }

                try
                {
                    Word w = extractWord(word);
                    onReceiveFunc(w);
                }
                catch (Exception ex)
                {
                    onErrorLogFunc?.Invoke(
                        $"{ex.Message} | raw: '{word}' ({word.Length} chars, bytes: {string.Join(" ",
                            System.Text.Encoding.ASCII.GetBytes(word).Select(b => b.ToString("X2")))})");
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            onErrorLogFunc?.Invoke($"Port error in receive callback: {ex.Message}");
            OnDisconnected?.Invoke();
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
     * Function to set the callback function for error logging
     *
     * @param errorLogger   the function that handles error logging
     */
    public void SetErrorLogger(Action<string> errorLogger)
    {
        this.onErrorLogFunc = errorLogger;
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