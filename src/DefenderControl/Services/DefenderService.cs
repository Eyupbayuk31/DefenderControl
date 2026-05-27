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
                disableScript = @"
                    # Tum yolları olustur
                    $defenderPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender'
                    $rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
                    $scanPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Scan'
                    $mpEnginePath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\MpEngine'
                    $policyPaths = @($defenderPath, $rtPath, $scanPath, $mpEnginePath)
                    
                    foreach ($path in $policyPaths) {
                        if (!(Test-Path $path)) {
                            New-Item -Path $path -Force | Out-Null
                        }
                    }
                    
                    # ANA DEVRE DISI BIRAKMA - DisableAntiSpyware (en onemli)
                    Set-ItemProperty -Path $defenderPath -Name 'DisableAntiSpyware' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $defenderPath -Name 'DisableRealtimeProtection' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $defenderPath -Name 'DisableOnAccessProtection' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $defenderPath -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $defenderPath -Name 'DisableScriptScanning' -Value 1 -Type DWord -Force
                    
                    # GERCEK ZAMANLI KORUMA AYARLARI
                    Set-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableScriptScanning' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableRealtimeProtection' -Value 1 -Type DWord -Force
                    
                    # TARAMA AYARLARI
                    Set-ItemProperty -Path $scanPath -Name 'DisableArchiveScanning' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'DisableAutoExclusions' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'DisableCatchupFullScan' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'DisableCatchupQuickScan' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'DisableCpuThrottleOnIdleScans' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'DisableEmailScanning' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'DisableHeuristics' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'DisableRemovableDriveScanning' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'DisableScanningMappedNetworkDrivesForFullScan' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'DisableScanningNetworkDrives' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $scanPath -Name 'UILockdown' -Value 1 -Type DWord -Force
                    
                    # NAPATCI KORUMA (Tamper Protection) - once kapatilmali
                    Set-ItemProperty -Path $defenderPath -Name 'DisableTamperProtection' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $defenderPath -Name 'TamperProtectionSource' -Value 0 -Type DWord -Force
                    
                    # Set-MpPreference - Tum korumalari devre disi birak
                    Set-MpPreference -DisableRealtimeMonitoring $true -Force
                    Set-MpPreference -DisableIOAVProtection $true -Force
                    Set-MpPreference -DisableBehaviorMonitoring $true -Force
                    Set-MpPreference -DisableScriptScanning $true -Force
                    Set-MpPreference -DisableEmailScanning $true -Force
                    Set-MpPreference -DisableArchiveScanning $true -Force
                    Set-MpPreference -DisableRemovableDriveScanning $true -Force
                    Set-MpPreference -DisableNetworkDriveScanning $true -Force
                    Set-MpPreference -DisableMapperReading $true -Force
                    Set-MpPreference -DisableHttpScanning $true -Force
                    Set-MpPreference -DisableDNSScanning $true -Force
                    Set-MpPreference -DisableIncomingTraffic $true -Force
                    Set-MpPreference -DisableOutgoingTraffic $true -Force
                    Set-MpPreference -DisableRdpScanning $true -Force
                    Set-MpPreference -DisableScriptScanning $true -Force
                    Set-MpPreference -DisableScansCorruptBootFiles $true -Force
                    Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $true -Force
                    Set-MpPreference -DisablePrivacyMode $true -Force
                    Set-MpPreference -DisableRestorePoint $true -Force
                    Set-MpPreference -DisableScout $true -Force
                    Set-MpPreference -DisableCpuThrottleOnIdleScans $true -Force
                    Set-MpPreference -SubmitSamplesConsent 0 -Force
                    
                    # Windows Security Center'da gorunmez yap
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Security Center' -Name 'AntiVirusOverride' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Security Center' -Name 'AntiSpywareOverride' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Security Center' -Name 'FirewallOverride' -Value 1 -Type DWord -Force
                    
                    # Hizli kapatma icin Windows Defender servisini durdur
                    Stop-Service -Name WinDefend -Force -ErrorAction SilentlyContinue
                    Set-Service -Name WinDefend -StartupType Disabled -ErrorAction SilentlyContinue
                    
                    Stop-Service -Name WdNisSvc -Force -ErrorAction SilentlyContinue
                    Set-Service -Name WdNisSvc -StartupType Disabled -ErrorAction SilentlyContinue
                    
                    Write-Output ""SUCCESS_PERMANENT""
                ";
            }
            else
            {
                // Gecici mod - Tum korumalari devre disi birak
                disableScript = @"
                    # Tum Set-MpPreference ayarlari
                    Set-MpPreference -DisableRealtimeMonitoring $true -Force
                    Set-MpPreference -DisableIOAVProtection $true -Force
                    Set-MpPreference -DisableBehaviorMonitoring $true -Force
                    Set-MpPreference -DisableScriptScanning $true -Force
                    Set-MpPreference -DisableEmailScanning $true -Force
                    Set-MpPreference -DisableArchiveScanning $true -Force
                    Set-MpPreference -DisableRemovableDriveScanning $true -Force
                    Set-MpPreference -DisableNetworkDriveScanning $true -Force
                    Set-MpPreference -DisableMapperReading $true -Force
                    Set-MpPreference -DisableHttpScanning $true -Force
                    Set-MpPreference -DisableDNSScanning $true -Force
                    Set-MpPreference -DisableIncomingTraffic $true -Force
                    Set-MpPreference -DisableOutgoingTraffic $true -Force
                    Set-MpPreference -DisableRdpScanning $true -Force
                    Set-MpPreference -DisableScansCorruptBootFiles $true -Force
                    Set-MpPreference -DisablePrivacyMode $true -Force
                    Set-MpPreference -DisableCpuThrottleOnIdleScans $true -Force
                    Set-MpPreference -SubmitSamplesConsent 0 -Force
                    
                    # Registry ile destekle
                    $rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
                    if (!(Test-Path $rtPath)) { New-Item -Path $rtPath -Force | Out-Null }
                    Set-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force
                    Set-ItemProperty -Path $rtPath -Name 'DisableScriptScanning' -Value 1 -Type DWord -Force
                    
                    Write-Output ""SUCCESS_TEMPORARY""
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
                    # Set-MpPreference ile tum korumalari etkinlestir
                    Set-MpPreference -DisableRealtimeMonitoring $false -Force
                    Set-MpPreference -DisableIOAVProtection $false -Force
                    Set-MpPreference -DisableBehaviorMonitoring $false -Force
                    Set-MpPreference -DisableScriptScanning $false -Force
                    Set-MpPreference -DisableEmailScanning $false -Force
                    Set-MpPreference -DisableArchiveScanning $false -Force
                    Set-MpPreference -DisableRemovableDriveScanning $false -Force
                    Set-MpPreference -DisableNetworkDriveScanning $false -Force
                    Set-MpPreference -DisableMapperReading $false -Force
                    Set-MpPreference -DisableHttpScanning $false -Force
                    Set-MpPreference -DisableDNSScanning $false -Force
                    Set-MpPreference -DisableIncomingTraffic $false -Force
                    Set-MpPreference -DisableOutgoingTraffic $false -Force
                    Set-MpPreference -DisableRdpScanning $false -Force
                    Set-MpPreference -DisableScansCorruptBootFiles $false -Force
                    Set-MpPreference -DisablePrivacyMode $false -Force
                    Set-MpPreference -DisableCpuThrottleOnIdleScans $false -Force
                    Set-MpPreference -SubmitSamplesConsent 3 -Force
                    
                    # Tum Registry kisitlamalarini kaldir
                    $defenderPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender'
                    $rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
                    $scanPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Scan'
                    $mpEnginePath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\MpEngine'
                    
                    # Ana Defender registry degerlerini kaldir
                    Remove-ItemProperty -Path $defenderPath -Name 'DisableAntiSpyware' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $defenderPath -Name 'DisableRealtimeProtection' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $defenderPath -Name 'DisableOnAccessProtection' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $defenderPath -Name 'DisableBehaviorMonitoring' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $defenderPath -Name 'DisableScriptScanning' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $defenderPath -Name 'DisableTamperProtection' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $defenderPath -Name 'TamperProtectionSource' -ErrorAction SilentlyContinue
                    
                    # Gercek zamanli koruma registry degerlerini kaldir
                    Remove-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $rtPath -Name 'DisableBehaviorMonitoring' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $rtPath -Name 'DisableScriptScanning' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $rtPath -Name 'DisableRealtimeProtection' -ErrorAction SilentlyContinue
                    
                    # Tarama registry degerlerini kaldir
                    Remove-ItemProperty -Path $scanPath -Name 'DisableArchiveScanning' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'DisableAutoExclusions' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'DisableCatchupFullScan' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'DisableCatchupQuickScan' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'DisableCpuThrottleOnIdleScans' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'DisableEmailScanning' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'DisableHeuristics' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'DisableRemovableDriveScanning' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'DisableScanningMappedNetworkDrivesForFullScan' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'DisableScanningNetworkDrives' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $scanPath -Name 'UILockdown' -ErrorAction SilentlyContinue
                    
                    # Security Center override degerlerini sifirla
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Security Center' -Name 'AntiVirusOverride' -Value 0 -Type DWord -Force
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Security Center' -Name 'AntiSpywareOverride' -Value 0 -Type DWord -Force
                    Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Security Center' -Name 'FirewallOverride' -Value 0 -Type DWord -Force
                    
                    # Servisleri yeniden baslat
                    Set-Service -Name WinDefend -StartupType Automatic -ErrorAction SilentlyContinue
                    Start-Service -Name WinDefend -ErrorAction SilentlyContinue
                    
                    Set-Service -Name WdNisSvc -StartupType Automatic -ErrorAction SilentlyContinue
                    Start-Service -Name WdNisSvc -ErrorAction SilentlyContinue
                    
                    Write-Output ""SUCCESS""
                ";
            }
            else
            {
                // Gecici mod - Tum korumalari etkinlestir
                enableScript = @"
                    Set-MpPreference -DisableRealtimeMonitoring $false -Force
                    Set-MpPreference -DisableIOAVProtection $false -Force
                    Set-MpPreference -DisableBehaviorMonitoring $false -Force
                    Set-MpPreference -DisableScriptScanning $false -Force
                    Set-MpPreference -DisableEmailScanning $false -Force
                    Set-MpPreference -DisableArchiveScanning $false -Force
                    Set-MpPreference -DisableRemovableDriveScanning $false -Force
                    Set-MpPreference -DisableNetworkDriveScanning $false -Force
                    Set-MpPreference -DisableMapperReading $false -Force
                    Set-MpPreference -DisableHttpScanning $false -Force
                    Set-MpPreference -DisableDNSScanning $false -Force
                    Set-MpPreference -DisableIncomingTraffic $false -Force
                    Set-MpPreference -DisableOutgoingTraffic $false -Force
                    Set-MpPreference -DisableRdpScanning $false -Force
                    Set-MpPreference -DisableScansCorruptBootFiles $false -Force
                    Set-MpPreference -DisablePrivacyMode $false -Force
                    Set-MpPreference -DisableCpuThrottleOnIdleScans $false -Force
                    Set-MpPreference -SubmitSamplesConsent 3 -Force
                    
                    # Registry gecici ayarlarini da kaldir
                    $rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
                    Remove-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $rtPath -Name 'DisableBehaviorMonitoring' -ErrorAction SilentlyContinue
                    Remove-ItemProperty -Path $rtPath -Name 'DisableScriptScanning' -ErrorAction SilentlyContinue
                    
                    Write-Output ""SUCCESS""
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
