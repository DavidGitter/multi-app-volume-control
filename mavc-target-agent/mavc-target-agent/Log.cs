using System;
using System.IO;
using System.Text;

/**
 * Singleton logger class that writes timestamped log entries to both console and file.
 * 
 * Provides thread-safe logging with support for different log levels (INFO, WARNING, ERROR).
 * All log messages are formatted with timestamps and severity levels.
 */
class Log
{
    private static Log? instance;

    /**
     * Gets the singleton instance of the logger.
     * 
     * @return the current Log instance
     */
    public static Log? Instance => instance;

    private readonly string logFilePath;
    private readonly object fileLock = new object();

    /**
     * Initializes the logger with the specified log file path.
     * Creates the necessary directories and initializes a fresh log file.
     * 
     * @param logFilePath  the full path where log entries will be written
     */
    public Log(string logFilePath)
    {
        this.logFilePath = logFilePath;
        instance = this;

        // Ensure directory exists and start with a fresh file.
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        File.WriteAllText(logFilePath, string.Empty);

        Write("INFO", "Logger started");
    }

    /**
     * Internal method that writes a log entry to both console and file.
     * Thread-safe file writing using a lock to prevent concurrent access issues.
     * 
     * @param level    the log level (INFO, WARNING, ERROR)
     * @param content  the log message content
     */
    private void Write(string level, string content)
    {
        string line = $"{DateTime.Now:HH:mm:ss.fff} | {level,-7} | {content}";

        // Mirror to console (no-op before AllocConsole, harmless after).
        Console.WriteLine(line);

        // Append to log file.
        lock (fileLock)
        {
            // FileShare.ReadWrite prevents IOException
            using var fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var sw = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = true };
            sw.WriteLine(line);
        }
    }

    public void Info(string content) => Write("INFO", content);

    public void Mixer(string content) => Write("MIXER", content);

    public void Warning(string content) => Write("WARNING", content);

    public void Error(string content) => Write("ERROR", content);
}