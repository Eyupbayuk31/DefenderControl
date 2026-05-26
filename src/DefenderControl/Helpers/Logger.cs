using System;
using System.IO;

namespace DefenderControl.Helpers;

public static class Logger
{
    // Log dosyasi yolu - Desktop'ta DefenderControl_Log.txt olarak kaydedilir
    private static readonly string LogFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
        "DefenderControl_Log.txt"
    );

    static Logger()
    {
        // Log dosyasi yoksa baslik bilgisiyle baslat
        if (!File.Exists(LogFilePath))
        {
            Log("==============================================", "SYSTEM");
            Log("Windows Defender Kontrol Paneli - Islem Log Dosyasi", "SYSTEM");
            Log("==============================================", "SYSTEM");
        }
    }

    // Log dosyasina mesaj yazar
    public static void Log(string message, string type = "INFO")
    {
        try
        {
            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {message}";
            File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
        }
        catch
        {
            // Loglama hatasi uygulamayi cokmeye neden olmamali
        }
    }

    // Hata mesajini log dosyasina yazar
    public static void LogError(string message, Exception? ex = null)
    {
        string exceptionDetails = ex != null ? $" | Exception: {ex.Message}\n{ex.StackTrace}" : "";
        Log($"{message}{exceptionDetails}", "ERROR");
    }
}
