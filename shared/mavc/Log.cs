using System;
using System.IO;
using System.Text;

/**
 * Singleton logger class that writes timestamped log entries to both console and file.
 * 
 * Provides thread-safe logging with support for different log levels (INFO, WARNING, ERROR, MIXER).
 * All log messages are formatted with timestamps and severity levels.
 */
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

        string logDir = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(logDir))
            Directory.CreateDirectory(logDir);
        File.WriteAllText(logFilePath, string.Empty);

        Write("INFO", "Logger started");
    }

    private void Write(string level, string content)
    {
        string line = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} | {1,-7} | {2}", DateTime.Now, level, content);
        Console.WriteLine(line);

        lock (fileLock)
        {
            using (var fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                sw.WriteLine(line);
            }
        }
    }

    public void Info(string content) { Write("INFO", content); }
    public void Mixer(string content) { Write("MIXER", content); }
    public void Warning(string content) { Write("WARNING", content); }
    public void Error(string content) { Write("ERROR", content); }
}