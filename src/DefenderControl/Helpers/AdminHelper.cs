using System.Security.Principal;

namespace DefenderControl.Helpers;

public static class AdminHelper
{
    public static bool IsRunningAsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void RestartAsAdmin()
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
            FileName = Environment.ProcessPath
        };

        startInfo.Verb = "runas";

        try
        {
            Logger.Log("Yönetici yetkileri ile yeniden başlatılıyor...");
            System.Diagnostics.Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Logger.LogError("Yönetici olarak başlatılamadı", ex);
            Console.WriteLine($"  [!] Yonetimci olarak baslatilamadi: {ex.Message}");
            Environment.Exit(1);
        }
    }

    public static void SetExecutionPolicyToBypass()
    {
        try
        {
            Logger.Log("Kayıt defteri üzerinden PowerShell ExecutionPolicy Bypass ayarlanıyor...");
            // Set PowerShell execution policy to Bypass in Registry
            Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell", "ExecutionPolicy", "Bypass");
            
            Logger.Log("PowerShell CLI üzerinden Set-ExecutionPolicy çalıştırılıyor...");
            // Also run a process to set it via command line
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -Command \"Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope LocalMachine -Force\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            System.Diagnostics.Process.Start(startInfo)?.WaitForExit();
            
            Logger.Log("PowerShell ExecutionPolicy başarıyla Bypass olarak ayarlandı.");
        }
        catch (Exception ex)
        {
            Logger.LogError("PowerShell ExecutionPolicy ayarlanırken hata oluştu", ex);
        }
    }

    public static void CheckAndElevate()
    {
        Logger.Log($"Uygulama başlatıldı. Yönetici yetkisi durumu: {IsRunningAsAdmin()}");
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
            SetExecutionPolicyToBypass();
        }
    }
}