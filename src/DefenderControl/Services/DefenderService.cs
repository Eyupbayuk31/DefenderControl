using System.Diagnostics;
using DefenderControl.Models;
using DefenderControl.Helpers;

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
            Logger.Log($"Windows Defender devre dışı bırakma işlemi başladı. Mod: {mode}");
            Console.WriteLine($"  [*] Windows Defender ({mode}) devre disi birakiliyor...");

            string disableScript;

            if (permanent)
            {
                // Kalici - Registry ve Group Policy ile
                disableScript = @"
                    # Ensure Registry Paths Exist
                    $defenderPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender'
                    $rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
                    
                    if (!(Test-Path $defenderPath)) {
                        New-Item -Path $defenderPath -Force | Out-Null
                    }
                    if (!(Test-Path $rtPath)) {
                        New-Item -Path $rtPath -Force | Out-Null
                    }

                    # Disable Real-time Protection
                    Set-ItemProperty -Path $defenderPath -Name 'DisableAntiSpyware' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force
                    
                    # Disable via Set-MpPreference
                    Set-MpPreference -DisableRealtimeMonitoring $true -Force -ErrorAction SilentlyContinue
                    Set-MpPreference -DisableIOAVProtection $true -Force -ErrorAction SilentlyContinue
                    Set-MpPreference -DisableBehaviorMonitoring $true -Force -ErrorAction SilentlyContinue
                    
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
                    Write-Output ""SUCCESS_TEMPORARY""
                ";
            }

            Logger.Log($"Kapatma scripti çalıştırılıyor...");
            var result = await RunPowerShellAsync(disableScript);
            Logger.Log($"Kapatma scripti tamamlandı. Çıktı: {result.Trim()}");
            
            if (result.Contains("SUCCESS_PERMANENT"))
            {
                Logger.Log("Windows Defender kalıcı olarak başarıyla kapatıldı.");
                Console.WriteLine("  [OK] Windows Defender kalici olarak devre disi birakildu!");
                Console.WriteLine("       (Sistem yeniden baslatildiktan sonra da devre disi kalacak)");
                return true;
            }
            else if (result.Contains("SUCCESS_TEMPORARY"))
            {
                Logger.Log("Windows Defender geçici olarak başarıyla kapatıldı.");
                Console.WriteLine("  [OK] Windows Defender gecici olarak devre disi birakildu!");
                Console.WriteLine("       (Sistem yeniden baslatildiginda otomatik acilacak)");
                return true;
            }
            
            Logger.Log("Kapatma işlemi başarısız oldu. Beklenen başarı yanıtı alınamadı.", "WARNING");
            Console.WriteLine("  [!] Islem sirinda bir sorun olustu.");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError("Kapatma işlemi sırasında kritik hata oluştu", ex);
            Console.WriteLine($"  [!] Hata: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EnableDefenderAsync(bool permanent = false)
    {
        try
        {
            string mode = permanent ? "kalici" : "gecici";
            Logger.Log($"Windows Defender etkinleştirme işlemi başladı. Mod: {mode}");
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
                    Write-Output ""SUCCESS""
                ";
            }

            Logger.Log($"Etkinleştirme scripti çalıştırılıyor...");
            var result = await RunPowerShellAsync(enableScript);
            Logger.Log($"Etkinleştirme scripti tamamlandı. Çıktı: {result.Trim()}");
            
            if (result.Contains("SUCCESS"))
            {
                Logger.Log("Windows Defender başarıyla etkinleştirildi.");
                Console.WriteLine("  [OK] Windows Defender basariyla etkinlestirildi!");
                if (permanent)
                {
                    Console.WriteLine("       (Kalici koruma aktif - sistem yeniden baslatsaniz dahi)");
                }
                return true;
            }
            
            Logger.Log("Etkinleştirme işlemi başarısız oldu. Beklenen başarı yanıtı alınamadı.", "WARNING");
            Console.WriteLine("  [!] Islem sirinda bir sorun olustu.");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError("Etkinleştirme işlemi sırasında kritik hata oluştu", ex);
            Console.WriteLine($"  [!] Hata: {ex.Message}");
            return false;
        }
    }

    private async Task<string> RunPowerShellAsync(string script)
    {
        byte[] scriptBytes = System.Text.Encoding.Unicode.GetBytes(script);
        string encodedScript = Convert.ToBase64String(scriptBytes);

        Logger.Log($"PowerShell çalıştırılıyor. Script uzunluğu: {script.Length} karakter. Base64 uzunluğu: {encodedScript.Length}");

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encodedScript}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            Logger.LogError("PowerShell işlemi başlatılamadı (Process.Start null döndü).");
            throw new InvalidOperationException("PowerShell baslatilamadi.");
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        if (!string.IsNullOrEmpty(error))
        {
            // Log everything to the desktop file for debugging
            Logger.Log($"PowerShell StandardError Çıktısı: {error.Trim()}", "POWERSHELL_ERROR");
            
            // Only print actual error messages to the console (filtering out verbose XML progress and preparing module logs)
            if (!error.Contains("#< CLIXML") || error.Contains("<S S=\"Error\">"))
            {
                string cleanError = error;
                if (error.Contains("<S S=\"Error\">"))
                {
                    // Extract and clean actual error if it is in CLIXML format
                    var matches = System.Text.RegularExpressions.Regex.Matches(error, @"<S S=""Error"">(.*?)<\/S>");
                    var errorLines = new System.Collections.Generic.List<string>();
                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        string line = match.Groups[1].Value.Replace("_x000D__x000A_", "").Trim();
                        if (!string.IsNullOrEmpty(line))
                        {
                            errorLines.Add(line);
                        }
                    }
                    cleanError = string.Join(Environment.NewLine + "  |  ", errorLines);
                }
                
                if (!string.IsNullOrEmpty(cleanError))
                {
                    Console.WriteLine($"  [!] PowerShell hatasi:\n  |  {cleanError}");
                }
            }
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