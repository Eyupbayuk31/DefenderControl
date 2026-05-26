using System;
using System.IO;

namespace DefenderControl.Helpers;

public static class Logger
{
    private static readonly string LogFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
        "DefenderControl_Log.txt"
    );

    static Logger()
    {
        // Start log file with an intro header if it doesn't exist
        if (!File.Exists(LogFilePath))
        {
            Log("==============================================", "SYSTEM");
            Log("Windows Defender Control Panel - Action Log File", "SYSTEM");
            Log("==============================================", "SYSTEM");
        }
    }

    public static void Log(string message, string type = "INFO")
    {
        try
        {
            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {message}";
            File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
        }
        catch
        {
            // Fail silently so logging never crashes the app
        }
    }

    public static void LogError(string message, Exception? ex = null)
    {
        string exceptionDetails = ex != null ? $" | Exception: {ex.Message}\n{ex.StackTrace}" : "";
        Log($"{message}{exceptionDetails}", "ERROR");
    }
}
