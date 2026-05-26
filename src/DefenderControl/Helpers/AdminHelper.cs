using System.Security.Principal;
using System.Runtime.InteropServices;

namespace DefenderControl.Helpers;

public static class AdminHelper
{
    // Windows API sabitleri - ANSI renk destegi icin
    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    // Windows API fonksiyonlari - Konsol renk destegi icin
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    // Konsolda ANSI renk kodlarini etkinlestirir
    public static void EnableAnsiColors()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
                if (GetConsoleMode(iStdOut, out uint outConsoleMode))
                {
                    outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                    SetConsoleMode(iStdOut, outConsoleMode);
                }
            }
        }
        catch
        {
            // Hata durumunda sessizce devam et
        }
    }

    // Uygulamanin yonetici yetkisiyle calisip calismadigini kontrol eder
    public static bool IsRunningAsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    // Uygulamayi yonetici yetkisiyle yeniden baslatir
    public static void RestartAsAdmin()
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
            FileName = Environment.ProcessPath
        };

        // Yonetici olarak calistirmak icin "runas" fiilini kullan
        startInfo.Verb = "runas";

        try
        {
            Logger.Log("Yonetici yetkileri ile yeniden baslatiliyor...");
            System.Diagnostics.Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Logger.LogError("Yonetici olarak baslatilamadi", ex);
            Console.WriteLine($"  [!] Yonetici olarak baslatilamadi: {ex.Message}");
            Environment.Exit(1);
        }
    }

    // PowerShell calistirma politikasini Bypass olarak ayarlar
    public static void SetExecutionPolicyToBypass()
    {
        try
        {
            Logger.Log("Kayit defteri uzerinden PowerShell ExecutionPolicy Bypass ayarlaniyor...");
            // PowerShell calistirma politikasini kayit defterinde Bypass olarak ayarla
            Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell", "ExecutionPolicy", "Bypass");
            
            Logger.Log("PowerShell CLI uzerinden Set-ExecutionPolicy calistiriliyor...");
            // Ayrica komut satiriyla da ayarla
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -Command \"Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope LocalMachine -Force\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(startInfo)?.WaitForExit();
            
            Logger.Log("PowerShell ExecutionPolicy basariyla Bypass olarak ayarlandi.");
        }
        catch (Exception ex)
        {
            Logger.LogError("PowerShell ExecutionPolicy ayarlanirken hata olustu", ex);
        }
    }

    // Yonetici yetkisi kontrolu yapar, gerekirse yukseltir
    public static void CheckAndElevate()
    {
        Logger.Log($"Uygulama baslatildi. Yonetici yetkisi durumu: {IsRunningAsAdmin()}");
        if (!IsRunningAsAdmin())
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("+=============================================================+");
            Console.WriteLine("|              ! YONETICI YETKISI GEREKLI                |");
            Console.WriteLine("+=============================================================+");
            Console.WriteLine();
            Console.WriteLine("  Bu uygulama Windows Defender'i kontrol etmek icin");
            Console.WriteLine("  yonetici (Administrator) yetkisi gerektirmektedir.");
            Console.WriteLine();
            Console.WriteLine("  Yonetici yetkisi ile yeniden baslatiliyor...");
            Console.WriteLine();
            Console.ResetColor();
            
            Thread.Sleep(1500);
            RestartAsAdmin();
        }
        else
        {
            // Yonetici yetkisi varsa PowerShell calistirma politikasini ayarla
            SetExecutionPolicyToBypass();
        }
    }
}