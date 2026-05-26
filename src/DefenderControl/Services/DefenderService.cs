using System.Diagnostics;
using DefenderControl.Models;
using DefenderControl.Helpers;

namespace DefenderControl.Services;

public class DefenderService
{
    // Windows Defender durumunu alir
    public async Task<DefenderStatus> GetStatusAsync()
    {
        var status = new DefenderStatus();
        
        try
        {
            // PowerShell ile Defender durumunu alma scripti
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

    // Windows Defender'i devre disi birakir
    public async Task<bool> DisableDefenderAsync(bool permanent = false)
    {
        try
        {
            string mode = permanent ? "kalici" : "gecici";
            Logger.Log($"Windows Defender devre disi birakma islemi basladi. Mod: {mode}");
            Console.WriteLine($"  [*] Windows Defender ({mode}) devre disi birakiliyor...");

            string disableScript;

            if (permanent)
            {
                // Kalici mod - Registry ve Group Policy ile devre disi birakma
                disableScript = @"
                    # Registry yollarini olustur
                    $defenderPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender'
                    $rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
                    
                    if (!(Test-Path $defenderPath)) {
                        New-Item -Path $defenderPath -Force | Out-Null
                    }
                    if (!(Test-Path $rtPath)) {
                        New-Item -Path $rtPath -Force | Out-Null
                    }

                    # Gercek zamanli korumayi devre disi birak
                    Set-ItemProperty -Path $defenderPath -Name 'DisableAntiSpyware' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force
                    
                    # Set-MpPreference ile de devre disi birak
                    Set-MpPreference -DisableRealtimeMonitoring $true -Force -ErrorAction SilentlyContinue
                    Set-MpPreference -DisableIOAVProtection $true -Force -ErrorAction SilentlyContinue
                    Set-MpPreference -DisableBehaviorMonitoring $true -Force -ErrorAction SilentlyContinue
                    
                    Write-Output ""SUCCESS_PERMANENT""
                ";
            }
            else
            {
                // Gecici mod - Sadece Set-MpPreference kullan
                disableScript = @"
                    Set-MpPreference -DisableRealtimeMonitoring $true
                    Set-MpPreference -DisableIOAVProtection $true
                    Set-MpPreference -DisableBehaviorMonitoring $true
                    Write-Output ""SUCCESS_TEMPORARY""
                ";
            }

            Logger.Log($"Kapatma scripti calistiriliyor...");
            var result = await RunPowerShellAsync(disableScript);
            Logger.Log($"Kapatma scripti tamamlandi. Cikti: {result.Trim()}");
            
            if (result.Contains("SUCCESS_PERMANENT"))
            {
                Logger.Log("Windows Defender kalici olarak basariyla kapatildi.");
                Console.WriteLine("  [OK] Windows Defender kalici olarak devre disi birakildi!");
                Console.WriteLine("       (Sistem yeniden baslatildiktan sonra da devre disi kalacak)");
                return true;
            }
            else if (result.Contains("SUCCESS_TEMPORARY"))
            {
                Logger.Log("Windows Defender gecici olarak basariyla kapatildi.");
                Console.WriteLine("  [OK] Windows Defender gecici olarak devre disi birakildi!");
                Console.WriteLine("       (Sistem yeniden baslatildiginda otomatik acilacak)");
                return true;
            }
            
            Logger.Log("Kapatma islemi basarisiz oldu. Beklenen basari yaniti alinamadi.", "WARNING");
            Console.WriteLine("  [!] Islem sirinda bir sorun olustu.");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError("Kapatma islemi sirasinda kritik hata olustu", ex);
            Console.WriteLine($"  [!] Hata: {ex.Message}");
            return false;
        }
    }

    // Windows Defender'i etkinlestirir
    public async Task<bool> EnableDefenderAsync(bool permanent = false)
    {
        try
        {
            string mode = permanent ? "kalici" : "gecici";
            Logger.Log($"Windows Defender etkinlestirme islemi basladi. Mod: {mode}");
            Console.WriteLine($"  [*] Windows Defender ({mode}) etkinlestiriliyor...");

            string enableScript;

            if (permanent)
            {
                // Kalici mod - Registry ve Group Policy'yi temizle
                enableScript = @"
                    # Set-MpPreference ile etkinlestir
                    Set-MpPreference -DisableRealtimeMonitoring $false -Force
                    Set-MpPreference -DisableIOAVProtection $false -Force
                    Set-MpPreference -DisableBehaviorMonitoring $false -Force
                    
                    # Registry kisitlamalarini kaldir
                    Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender' -Name 'DisableAntiSpyware' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableRealtimeMonitoring' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableIOAVProtection' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableBehaviorMonitoring' -ErrorAction SilentlyContinue
                    
                    Write-Output ""SUCCESS""
                ";
            }
            else
            {
                // Gecici mod - Sadece Set-MpPreference kullan
                enableScript = @"
                    Set-MpPreference -DisableRealtimeMonitoring $false
                    Set-MpPreference -DisableIOAVProtection $false
                    Set-MpPreference -DisableBehaviorMonitoring $false
                    Write-Output ""SUCCESS""
                ";
            }

            Logger.Log($"Etkinlestirme scripti calistiriliyor...");
            var result = await RunPowerShellAsync(enableScript);
            Logger.Log($"Etkinlestirme scripti tamamlandi. Cikti: {result.Trim()}");
            
            if (result.Contains("SUCCESS"))
            {
                Logger.Log("Windows Defender basariyla etkinlestirildi.");
                Console.WriteLine("  [OK] Windows Defender basariyla etkinlestirildi!");
                if (permanent)
                {
                    Console.WriteLine("       (Kalici koruma aktif - sistem yeniden baslatsaniz dahi)");
                }
                return true;
            }
            
            Logger.Log("Etkinlestirme islemi basarisiz oldu. Beklenen basari yaniti alinamadi.", "WARNING");
            Console.WriteLine("  [!] Islem sirinda bir sorun olustu.");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError("Etkinlestirme islemi sirasinda kritik hata olustu", ex);
            Console.WriteLine($"  [!] Hata: {ex.Message}");
            return false;
        }
    }

    // PowerShell scriptini calistirir ve sonucu dondurur
    private async Task<string> RunPowerShellAsync(string script)
    {
        // Script'i Base64 formatina cevir (ozel karakter sorunlarini onlemek icin)
        byte[] scriptBytes = System.Text.Encoding.Unicode.GetBytes(script);
        string encodedScript = Convert.ToBase64String(scriptBytes);

        Logger.Log($"PowerShell calistiriliyor. Script uzunlugu: {script.Length} karakter. Base64 uzunlugu: {encodedScript.Length}");

        // PowerShell prosesini baslat
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
            Logger.LogError("PowerShell islemi baslatilamadi (Process.Start null dondurdu).");
            throw new InvalidOperationException("PowerShell baslatilamadi.");
        }

        // Ciktilari oku
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        // Hata varsa isle
        if (!string.IsNullOrEmpty(error))
        {
            // Hatalari log dosyasina yaz (hata ayiklama icin)
            Logger.Log($"PowerShell StandardError Ciktisi: {error.Trim()}", "POWERSHELL_ERROR");
            
            // Sadece gercek hata mesajlarini yazdir (gereksiz XML ciktilarini filtrele)
            if (!error.Contains("#< CLIXML") || error.Contains("<S S=\"Error\">"))
            {
                string cleanError = error;
                if (error.Contains("<S S=\"Error\">"))
                {
                    // CLIXML formatindaki gercek hatayi cikar ve temizle
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

    // PowerShell ciktisini DefenderStatus nesnesine donusturur
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
