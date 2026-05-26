using System.Diagnostics;
using DefenderControl.Models;

namespace DefenderControl.Services;

public class DefenderService
{
    public async Task<DefenderStatus> GetStatusAsync()
    {
        var status = new DefenderStatus();
        
        try
        {
            var script = @"
                $status = Get-MpComputerStatus
                Write-Output ""RealTimeProtection:$($status.RealTimeProtectionEnabled)""
                Write-Output ""IoavProtection:$($status.IOAVProtectionEnabled)""
                Write-Output ""BehaviorMonitor:$($status.BehaviorMonitorEnabled)""
                Write-Output ""AntivirusEnabled:$($status.AntivirusEnabled)""
                Write-Output ""LastScanTime:$($status.FullScanEndTime)""
                Write-Output ""AntivirusSignature:$($status.AntivirusSignatureVersion)""
                Write-Output ""AntispywareSignature:$($status.AntispywareSignatureVersion)""
                Write-Output ""EngineVersion:$($status.AntivirusEngineVersion)""
            ";

            var result = await RunPowerShellAsync(script);
            ParseStatusOutput(result, status);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [!] Durum alinirken hata: {ex.Message}");
        }

        return status;
    }

    public async Task<bool> DisableDefenderAsync(bool permanent = false)
    {
        try
        {
            string mode = permanent ? "kalici" : "gecici";
            Console.WriteLine($"  [*] Windows Defender ({mode}) devre disi birakiliyor...");

            string disableScript;

            if (permanent)
            {
                // Kalici - Registry ve Group Policy ile
                disableScript = @"
                    # Disable Real-time Protection
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender' -Name 'DisableAntiSpyware' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableRealtimeMonitoring' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableIOAVProtection' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force
                    
                    # Disable via Set-MpPreference
                    Set-MpPreference -DisableRealtimeMonitoring $true -Force
                    Set-MpPreference -DisableIOAVProtection $true -Force
                    Set-MpPreference -DisableBehaviorMonitoring $true -Force
                    Set-MpPreference -DisableAntivirus $true -Force
                    
                    Write-Output ""SUCCESS_PERMANENT""
                ";
            }
            else
            {
                // Gecici - Sadece Set-MpPreference
                disableScript = @"
                    Set-MpPreference -DisableRealtimeMonitoring $true
                    Set-MpPreference -DisableIOAVProtection $true
                    Set-MpPreference -DisableBehaviorMonitoring $true
                    Set-MpPreference -DisableAntivirus $true
                    Write-Output ""SUCCESS_TEMPORARY""
                ";
            }

            var result = await RunPowerShellAsync(disableScript);
            
            if (result.Contains("SUCCESS_PERMANENT"))
            {
                Console.WriteLine("  [OK] Windows Defender kalici olarak devre disi birakildu!");
                Console.WriteLine("       (Sistem yeniden baslatildiktan sonra da devre disi kalacak)");
                return true;
            }
            else if (result.Contains("SUCCESS_TEMPORARY"))
            {
                Console.WriteLine("  [OK] Windows Defender gecici olarak devre disi birakildu!");
                Console.WriteLine("       (Sistem yeniden baslatildiginda otomatik acilacak)");
                return true;
            }
            
            Console.WriteLine("  [!] Islem sirinda bir sorun olustu.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [!] Hata: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EnableDefenderAsync(bool permanent = false)
    {
        try
        {
            string mode = permanent ? "kalici" : "gecici";
            Console.WriteLine($"  [*] Windows Defender ({mode}) etkinlestiriliyor...");

            string enableScript;

            if (permanent)
            {
                // Kalici - Registry ve Group Policy'yi temizle
                enableScript = @"
                    # Enable via Set-MpPreference
                    Set-MpPreference -DisableRealtimeMonitoring $false -Force
                    Set-MpPreference -DisableIOAVProtection $false -Force
                    Set-MpPreference -DisableBehaviorMonitoring $false -Force
                    Set-MpPreference -DisableAntivirus $false -Force
                    
                    # Remove Registry restrictions
                    Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender' -Name 'DisableAntiSpyware' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableRealtimeMonitoring' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableIOAVProtection' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableBehaviorMonitoring' -ErrorAction SilentlyContinue
                    
                    Write-Output ""SUCCESS""
                ";
            }
            else
            {
                // Gecici - Sadece Set-MpPreference
                enableScript = @"
                    Set-MpPreference -DisableRealtimeMonitoring $false
                    Set-MpPreference -DisableIOAVProtection $false
                    Set-MpPreference -DisableBehaviorMonitoring $false
                    Set-MpPreference -DisableAntivirus $false
                    Write-Output ""SUCCESS""
                ";
            }

            var result = await RunPowerShellAsync(enableScript);
            
            if (result.Contains("SUCCESS"))
            {
                Console.WriteLine("  [OK] Windows Defender basariyla etkinlestirildi!");
                if (permanent)
                {
                    Console.WriteLine("       (Kalici koruma aktif - sistem yeniden baslatsaniz dahi)");
                }
                return true;
            }
            
            Console.WriteLine("  [!] Islem sirinda bir sorun olustu.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [!] Hata: {ex.Message}");
            return false;
        }
    }

    private async Task<string> RunPowerShellAsync(string script)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("PowerShell baslatilamadi.");
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"  [!] PowerShell hatasi: {error}");
        }

        return output;
    }

    private void ParseStatusOutput(string output, DefenderStatus status)
    {
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var parts = line.Trim().Split(':', 2);
            if (parts.Length != 2) continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            switch (key)
            {
                case "RealTimeProtection":
                    status.IsRealTimeProtectionEnabled = value.Equals("True", StringComparison.OrdinalIgnoreCase);
                    break;
                case "IoavProtection":
                    status.IsIoavProtectionEnabled = value.Equals("True", StringComparison.OrdinalIgnoreCase);
                    break;
                case "BehaviorMonitor":
                    status.IsBehaviorMonitorEnabled = value.Equals("True", StringComparison.OrdinalIgnoreCase);
                    break;
                case "AntivirusEnabled":
                    status.IsAntivirusEnabled = value.Equals("True", StringComparison.OrdinalIgnoreCase);
                    break;
                case "LastScanTime":
                    status.LastScanTime = value;
                    break;
                case "AntivirusSignature":
                    status.AntivirusSignatureVersion = value;
                    break;
                case "AntispywareSignature":
                    status.AntispywareSignatureVersion = value;
                    break;
                case "EngineVersion":
                    status.EngineVersion = value;
                    break;
            }
        }
    }
}