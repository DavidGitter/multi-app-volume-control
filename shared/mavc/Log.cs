using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Log
{
    private string logFilePath;
    public Log(string logFilePath)
    {
        this.logFilePath = logFilePath;
        File.Create(logFilePath).Dispose();
        Info("Started logger");
    }

    private void write(string content)
    {
        using (StreamWriter writer = new StreamWriter(logFilePath))
        {
            writer.WriteLine(content);
        }
    }

    public void Info(string content)
    {
        write(DateTime.Now + " | INFO: " + content);
    }

    public void Warning(string content)
    {
        write(DateTime.Now + " | Warning: " + content);
    }

    public void Error(string content)
    {
        write(DateTime.Now + " | Error: " + content);
    }
}
