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
            System.Diagnostics.Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [!] Yonetimci olarak baslatilamadi: {ex.Message}");
            Environment.Exit(1);
        }
    }

    public static void CheckAndElevate()
    {
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
    }
}