using System;
using System.IO;

class Log
{
    private static Log _instance;
    public static Log Instance => _instance;

    private readonly string logFilePath;
    private readonly object fileLock = new object();

    public Log(string logFilePath)
    {
        this.logFilePath = logFilePath;
        _instance = this;

        // Ensure directory exists and start with a fresh file.
        string logDir = Path.GetDirectoryName(logFilePath);
        Directory.CreateDirectory(logDir);
        File.WriteAllText(logFilePath, string.Empty);

        Write("INFO", "Logger started");
    }

    private void Write(string level, string content)
    {
        string line = $"{DateTime.Now:HH:mm:ss.fff} | {level,-7} | {content}";

        // Mirror to console (no-op before AllocConsole, harmless after).
        Console.WriteLine(line);

        // Append to log file.
        lock (fileLock)
        {
            using (StreamWriter sw = new StreamWriter(logFilePath, append: true))
            {
                sw.WriteLine(line);
            }
        }
    }

    public void Info(string content)    => Write("INFO",    content);
    public void Warning(string content) => Write("WARNING", content);
    public void Error(string content)   => Write("ERROR",   content);
}
