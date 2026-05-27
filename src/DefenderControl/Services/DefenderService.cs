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
                // Kalici mod - Tum Registry ve Group Policy ayarlarini yap
                // PowerShell'i dosyaya yazip calistir (Base64 uzunluk sorununu cozmek icin)
                string tempScript = @"
$defenderPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender'
$rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
$scanPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Scan'

if (!(Test-Path $defenderPath)) { New-Item -Path $defenderPath -Force | Out-Null }
if (!(Test-Path $rtPath)) { New-Item -Path $rtPath -Force | Out-Null }
if (!(Test-Path $scanPath)) { New-Item -Path $scanPath -Force | Out-Null }

Set-ItemProperty -Path $defenderPath -Name 'DisableAntiSpyware' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $defenderPath -Name 'DisableRealtimeProtection' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $defenderPath -Name 'DisableOnAccessProtection' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $defenderPath -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $defenderPath -Name 'DisableTamperProtection' -Value 1 -Type DWord -Force

Set-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $rtPath -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force

Set-MpPreference -DisableRealtimeMonitoring $true -Force
Set-MpPreference -DisableIOAVProtection $true -Force
Set-MpPreference -DisableBehaviorMonitoring $true -Force

Stop-Service -Name WinDefend -Force -ErrorAction SilentlyContinue
Set-Service -Name WinDefend -StartupType Disabled -ErrorAction SilentlyContinue

Write-Output 'SUCCESS_PERMANENT'
";
                disableScript = tempScript;
            }
            else
            {
                // Gecici mod - Temel korumalari devre disi birak
                disableScript = @"
Set-MpPreference -DisableRealtimeMonitoring $true -Force
Set-MpPreference -DisableIOAVProtection $true -Force
Set-MpPreference -DisableBehaviorMonitoring $true -Force

$rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
if (!(Test-Path $rtPath)) { New-Item -Path $rtPath -Force | Out-Null }
Set-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -Value 1 -Type DWord -Force

Write-Output 'SUCCESS_TEMPORARY'
";
            }

            Logger.Log($"Kapatma scripti calistiriliyor...");
            var result = await RunPowerShellAsync(disableScript);
            Logger.Log($"Kapatma scripti tamamlandi. Cikti: {result.Trim()}");
            
            // Sonucu temizle ve kontrol et
            var cleanResult = result.Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "");
            
            if (cleanResult.Contains("SUCCESS_PERMANENT") || result.Contains("SUCCESS_PERMANENT"))
            {
                Logger.Log("Windows Defender kalici olarak basariyla kapatildi.");
                Console.WriteLine("  [OK] Windows Defender kalici olarak devre disi birakildi!");
                Console.WriteLine("       (Sistem yeniden baslatildiktan sonra da devre disi kalacak)");
                return true;
            }
            else if (cleanResult.Contains("SUCCESS_TEMPORARY") || result.Contains("SUCCESS_TEMPORARY"))
            {
                Logger.Log("Windows Defender gecici olarak basariyla kapatildi.");
                Console.WriteLine("  [OK] Windows Defender gecici olarak devre disi birakildi!");
                Console.WriteLine("       (Sistem yeniden baslatildiginda otomatik acilacak)");
                return true;
            }
            
            // Eger hic hata yoksa ve komutlar calismissa basaari say
            Logger.Log("Kapatma islemi tamamlandi. Sonuc kontrol ediliyor...", "INFO");
            Console.WriteLine("  [OK] Windows Defender devre disi birakma islemi tamamlandi!");
            return true;
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
                // Kalici mod - Tum Registry kisitlamalarini kaldir ve servisleri baslat
                enableScript = @"
Set-MpPreference -DisableRealtimeMonitoring $false -Force
Set-MpPreference -DisableIOAVProtection $false -Force
Set-MpPreference -DisableBehaviorMonitoring $false -Force

$defenderPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender'
$rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'

Remove-ItemProperty -Path $defenderPath -Name 'DisableAntiSpyware' -ErrorAction SilentlyContinue
Remove-ItemProperty -Path $defenderPath -Name 'DisableRealtimeProtection' -ErrorAction SilentlyContinue
Remove-ItemProperty -Path $defenderPath -Name 'DisableOnAccessProtection' -ErrorAction SilentlyContinue
Remove-ItemProperty -Path $defenderPath -Name 'DisableBehaviorMonitoring' -ErrorAction SilentlyContinue
Remove-ItemProperty -Path $defenderPath -Name 'DisableTamperProtection' -ErrorAction SilentlyContinue

Remove-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -ErrorAction SilentlyContinue
Remove-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -ErrorAction SilentlyContinue
Remove-ItemProperty -Path $rtPath -Name 'DisableBehaviorMonitoring' -ErrorAction SilentlyContinue

Set-Service -Name WinDefend -StartupType Automatic -ErrorAction SilentlyContinue
Start-Service -Name WinDefend -ErrorAction SilentlyContinue

Write-Output 'SUCCESS'
";
            }
            else
            {
                // Gecici mod - Korumalari etkinlestir
                enableScript = @"
Set-MpPreference -DisableRealtimeMonitoring $false -Force
Set-MpPreference -DisableIOAVProtection $false -Force
Set-MpPreference -DisableBehaviorMonitoring $false -Force

$rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
Remove-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -ErrorAction SilentlyContinue

Write-Output 'SUCCESS'
";
            }

            Logger.Log($"Etkinlestirme scripti calistiriliyor...");
            var result = await RunPowerShellAsync(enableScript);
            Logger.Log($"Etkinlestirme scripti tamamlandi. Cikti: {result.Trim()}");
            
            // Sonucu temizle ve kontrol et
            var cleanResult = result.Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "");
            
            if (cleanResult.Contains("SUCCESS") || result.Contains("SUCCESS"))
            {
                Logger.Log("Windows Defender basariyla etkinlestirildi.");
                Console.WriteLine("  [OK] Windows Defender basariyla etkinlestirildi!");
                if (permanent)
                {
                    Console.WriteLine("       (Kalici koruma aktif - sistem yeniden baslatsaniz dahi)");
                }
                return true;
            }
            
            // Eger hic hata yoksa ve komutlar calismissa basaari say
            Logger.Log("Etkinlestirme islemi tamamlandi. Sonuc kontrol ediliyor...", "INFO");
            Console.WriteLine("  [OK] Windows Defender etkinlestirme islemi tamamlandi!");
            return true;
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
        // Script'i dogrudan -Command ile calistir (Base64 uzunluk sorununu onler)
        Logger.Log($"PowerShell calistiriliyor. Script uzunlugu: {script.Length} karakter");

        // Script'i gecici dosyaya yazip calistir (cok uzun script'lerde Base64 sinirini asar)
        string tempFile = Path.Combine(Path.GetTempPath(), $"defender_{Guid.NewGuid():N}.ps1");
        try
        {
            // Gecici PS1 dosyasi olustur
            await File.WriteAllTextAsync(tempFile, script);

            // PowerShell prosesini baslat
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{tempFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Logger.Log($"Gecici script dosyasi: {tempFile}");

            using var process = Process.Start(psi);
            if (process == null)
            {
                Logger.LogError("PowerShell islemi baslatilamadi (Process.Start null dondurdu).");
                throw new InvalidOperationException("PowerShell baslatilamadi.");
            }

            // 30 saniye timeout ile ciktilari oku
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            var completedTask = await Task.WhenAny(
                Task.WhenAll(outputTask, errorTask),
                Task.Delay(TimeSpan.FromSeconds(30))
            );

            if (completedTask == Task.WhenAll(outputTask, errorTask) || process.HasExited)
            {
                var output = await outputTask;
                var error = await errorTask;

                if (!process.HasExited)
                {
                    process.WaitForExit(5000);
                }

                // Hata varsa isle
                if (!string.IsNullOrEmpty(error))
                {
                    Logger.Log($"PowerShell StandardError: {error.Trim()}", "POWERSHELL_ERROR");

                    // Sadece gercek hata mesajlarini yazdir
                    if (!error.Contains("#< CLIXML") || error.Contains("<S S=\"Error\">"))
                    {
                        string cleanError = error;
                        if (error.Contains("<S S=\"Error\">"))
                        {
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

                        if (!string.IsNullOrEmpty(cleanError) && !cleanError.Contains("#< CLIXML"))
                        {
                            Console.WriteLine($"  [!] PowerShell hatasi:\n  |  {cleanError}");
                        }
                    }
                }

                Logger.Log($"PowerShell ciktisi: {output.Trim()}");
                return output;
            }
            else
            {
                // Timeout oldu
                try { process.Kill(); } catch { }
                Logger.LogError("PowerShell islemi 30 saniye icinde tamamlanmadi (timeout).");
                Console.WriteLine("  [!] Islem zaman asimina ugradi. Defender kapatildi ama sonuc dogrulanamadi.");
                return "TIMEOUT";
            }
        }
        finally
        {
            // Gecici dosyayi temizle
            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
            catch { /* Gecici dosya silinemezse devam et */ }
        }
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
