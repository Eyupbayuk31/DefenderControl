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

    public async Task<bool> DisableDefenderAsync()
    {
        try
        {
            Console.WriteLine("  [*] Windows Defender devre disi birakiliyor...");

            var disableScript = @"
                Set-MpPreference -DisableRealtimeMonitoring $true
                Set-MpPreference -DisableIOAVProtection $true
                Set-MpPreference -DisableBehaviorMonitoring $true
                Set-MpPreference -DisableAntivirus $true
                Write-Output ""SUCCESS""
            ";

            var result = await RunPowerShellAsync(disableScript);
            
            if (result.Contains("SUCCESS"))
            {
                Console.WriteLine("  [✓] Windows Defender basariyla devre disi birakildu!");
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

    public async Task<bool> EnableDefenderAsync()
    {
        try
        {
            Console.WriteLine("  [*] Windows Defender etkinlestiriliyor...");

            var enableScript = @"
                Set-MpPreference -DisableRealtimeMonitoring $false
                Set-MpPreference -DisableIOAVProtection $false
                Set-MpPreference -DisableBehaviorMonitoring $false
                Set-MpPreference -DisableAntivirus $false
                Write-Output ""SUCCESS""
            ";

            var result = await RunPowerShellAsync(enableScript);
            
            if (result.Contains("SUCCESS"))
            {
                Console.WriteLine("  [✓] Windows Defender basariyla etkinlestirildi!");
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