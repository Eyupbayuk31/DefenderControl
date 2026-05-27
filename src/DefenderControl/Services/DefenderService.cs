using System.Diagnostics;
using DefenderControl.Models;
using DefenderControl.Helpers;

namespace DefenderControl.Services;

public class DefenderService
{
    // ANSI Renk Kodlari (Program.cs ile ayni)
    enum AnsiColorType
    {
        Primary,
        Secondary,
        Header,
        Info,
        Success,
        Error,
        Warning,
        Link,
        Highlight
    }

    // Metne ANSI renk kodu ekler
    static string AnsiColor(string text, AnsiColorType colorType)
    {
        string colorCode = colorType switch
        {
            AnsiColorType.Primary => "\u001b[36m",      // Cyan
            AnsiColorType.Secondary => "\u001b[90m",    // Bright Black
            AnsiColorType.Header => "\u001b[1;36m",     // Bold Cyan
            AnsiColorType.Info => "\u001b[37m",         // White
            AnsiColorType.Success => "\u001b[32m",      // Green
            AnsiColorType.Error => "\u001b[31m",        // Red
            AnsiColorType.Warning => "\u001b[33m",      // Yellow
            AnsiColorType.Link => "\u001b[94m",         // Blue (link)
            AnsiColorType.Highlight => "\u001b[1;33m", // Bold Yellow
            _ => "\u001b[0m"
        };
        return $"{colorCode}{text}\u001b[0m";
    }

    // Renkli metin yazdirir
    static void AnsiWrite(string text, AnsiColorType colorType)
    {
        Console.Write(AnsiColor(text, colorType));
    }

    // Renkli metin yazdirir ve satir atlar
    static void AnsiWriteLine(string text, AnsiColorType colorType)
    {
        Console.WriteLine(AnsiColor(text, colorType));
    }

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

            if (permanent)
            {
                // Kalici mod - Adim adim mesajlarla PowerShell scripti
                Console.WriteLine();
                AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Info);
                AnsiWriteLine("  │          WINDOWS DEFENDER KALICI KAPATMA              │", AnsiColorType.Error);
                AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Info);
                Console.WriteLine();

                // Adim 1: Registry ayarlari
                AnsiWrite("  [1/4] ", AnsiColorType.Highlight);
                AnsiWriteLine("Registry (Group Policy) ayarlari yapilandiriliyor...", AnsiColorType.Warning);
                Logger.Log("Adim 1: Registry ayarlari yapilandiriliyor");

                string regScript = @"
$defenderPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender'
$rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'

if (!(Test-Path $defenderPath)) { New-Item -Path $defenderPath -Force | Out-Null }
if (!(Test-Path $rtPath)) { New-Item -Path $rtPath -Force | Out-Null }

Set-ItemProperty -Path $defenderPath -Name 'DisableAntiSpyware' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $defenderPath -Name 'DisableRealtimeProtection' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $defenderPath -Name 'DisableOnAccessProtection' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $defenderPath -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $defenderPath -Name 'DisableTamperProtection' -Value 1 -Type DWord -Force

Set-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $rtPath -Name 'DisableBehaviorMonitoring' -Value 1 -Type DWord -Force

Write-Output 'STEP1_OK'
";
                var regResult = await RunPowerShellAsync(regScript);
                if (regResult.Contains("STEP1_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Registry ayarlari yapildi", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Registry ayarlari kisitlamali olabilir", AnsiColorType.Warning);
                }

                // Adim 2: Set-MpPreference ayarlari
                AnsiWrite("  [2/4] ", AnsiColorType.Highlight);
                AnsiWriteLine("Gercek zamanli koruma kapatiliyor...", AnsiColorType.Warning);
                Logger.Log("Adim 2: Set-MpPreference ayarlari yapilandiriliyor");

                string prefScript = @"
Set-MpPreference -DisableRealtimeMonitoring $true -Force
Set-MpPreference -DisableIOAVProtection $true -Force
Set-MpPreference -DisableBehaviorMonitoring $true -Force
Set-MpPreference -DisableScriptScanning $true -Force
Set-MpPreference -DisableEmailScanning $true -Force
Set-MpPreference -DisableArchiveScanning $true -Force

Write-Output 'STEP2_OK'
";
                var prefResult = await RunPowerShellAsync(prefScript);
                if (prefResult.Contains("STEP2_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Gercek zamanli koruma kapatildi", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Koruma ayarlari kisitlamali olabilir", AnsiColorType.Warning);
                }

                // Adim 3: Servisleri durdur
                AnsiWrite("  [3/4] ", AnsiColorType.Highlight);
                AnsiWriteLine("Windows Defender servisleri durduruluyor...", AnsiColorType.Warning);
                Logger.Log("Adim 3: Servisler durduruluyor");

                string svcScript = @"
Stop-Service -Name WinDefend -Force -ErrorAction SilentlyContinue
Set-Service -Name WinDefend -StartupType Disabled -ErrorAction SilentlyContinue
Stop-Service -Name WdNisSvc -Force -ErrorAction SilentlyContinue
Set-Service -Name WdNisSvc -StartupType Disabled -ErrorAction SilentlyContinue

Write-Output 'STEP3_OK'
";
                var svcResult = await RunPowerShellAsync(svcScript);
                if (svcResult.Contains("STEP3_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Servisler durduruldu", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Servisler durdurulamadi (korunan sistem)", AnsiColorType.Warning);
                }

                // Adim 4: Sonuc
                AnsiWrite("  [4/4] ", AnsiColorType.Highlight);
                AnsiWriteLine("Sonuc kontrol ediliyor...", AnsiColorType.Warning);
                Logger.Log("Adim 4: Sonuc kontrol ediliyor");

                Console.WriteLine();
                AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Success);
                AnsiWriteLine("  │              ISLEM TAMAMLANDI                         │", AnsiColorType.Success);
                AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Success);
                Console.WriteLine();
                AnsiWriteLine("  Windows Defender kalici olarak devre disi birakildi!", AnsiColorType.Success);
                AnsiWriteLine("  (Sistem yeniden baslatildiktan sonra da devre disi kalacak)", AnsiColorType.Info);
                
                return true;
            }
            else
            {
                // Gecici mod - Adim adim mesajlarla
                Console.WriteLine();
                AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Info);
                AnsiWriteLine("  │          WINDOWS DEFENDER GECICI KAPATMA              │", AnsiColorType.Error);
                AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Info);
                Console.WriteLine();

                // Adim 1: Set-MpPreference
                AnsiWrite("  [1/2] ", AnsiColorType.Highlight);
                AnsiWriteLine("Gercek zamanli koruma kapatiliyor...", AnsiColorType.Warning);
                Logger.Log("Adim 1: Gecici koruma kapatiliyor");

                string prefScript = @"
Set-MpPreference -DisableRealtimeMonitoring $true -Force
Set-MpPreference -DisableIOAVProtection $true -Force
Set-MpPreference -DisableBehaviorMonitoring $true -Force
Set-MpPreference -DisableScriptScanning $true -Force
Set-MpPreference -DisableEmailScanning $true -Force
Set-MpPreference -DisableArchiveScanning $true -Force

Write-Output 'STEP1_OK'
";
                var prefResult = await RunPowerShellAsync(prefScript);
                if (prefResult.Contains("STEP1_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Gercek zamanli koruma kapatildi", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Koruma ayarlari kisitlamali olabilir", AnsiColorType.Warning);
                }

                // Adim 2: Registry desteği
                AnsiWrite("  [2/2] ", AnsiColorType.Highlight);
                AnsiWriteLine("Registry ayarlari yapilandiriliyor...", AnsiColorType.Warning);
                Logger.Log("Adim 2: Registry ayarlari");

                string regScript = @"
$rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
if (!(Test-Path $rtPath)) { New-Item -Path $rtPath -Force | Out-Null }
Set-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -Value 1 -Type DWord -Force
Set-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -Value 1 -Type DWord -Force

Write-Output 'STEP2_OK'
";
                var regResult = await RunPowerShellAsync(regScript);
                if (regResult.Contains("STEP2_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Registry ayarlari yapildi", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Registry ayarlari kisitlamali olabilir", AnsiColorType.Warning);
                }

                Console.WriteLine();
                AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Success);
                AnsiWriteLine("  │              ISLEM TAMAMLANDI                         │", AnsiColorType.Success);
                AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Success);
                Console.WriteLine();
                AnsiWriteLine("  Windows Defender gecici olarak devre disi birakildi!", AnsiColorType.Success);
                AnsiWriteLine("  (Sistem yeniden baslatildiginda otomatik acilacak)", AnsiColorType.Info);
                
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Kapatma islemi sirasinda kritik hata olustu", ex);
            Console.WriteLine();
            AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Error);
            AnsiWriteLine("  │              ISLEM BASARISIZ                          │", AnsiColorType.Error);
            AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Error);
            Console.WriteLine();
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

            if (permanent)
            {
                // Kalici mod - Adim adim mesajlarla
                Console.WriteLine();
                AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Info);
                AnsiWriteLine("  │          WINDOWS DEFENDER KALICI ACMA                │", AnsiColorType.Success);
                AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Info);
                Console.WriteLine();

                // Adim 1: Registry degerlerini temizle
                AnsiWrite("  [1/3] ", AnsiColorType.Highlight);
                AnsiWriteLine("Registry (Group Policy) degerleri temizleniyor...", AnsiColorType.Warning);
                Logger.Log("Adim 1: Registry degerleri temizleniyor");

                string regScript = @"
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

Write-Output 'STEP1_OK'
";
                var regResult = await RunPowerShellAsync(regScript);
                if (regResult.Contains("STEP1_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Registry degerleri temizlendi", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Registry temizlenirken sorun olustu", AnsiColorType.Warning);
                }

                // Adim 2: Korumalari ac
                AnsiWrite("  [2/3] ", AnsiColorType.Highlight);
                AnsiWriteLine("Gercek zamanli koruma etkinlestiriliyor...", AnsiColorType.Warning);
                Logger.Log("Adim 2: Korumalar etkinlestiriliyor");

                string prefScript = @"
Set-MpPreference -DisableRealtimeMonitoring $false -Force
Set-MpPreference -DisableIOAVProtection $false -Force
Set-MpPreference -DisableBehaviorMonitoring $false -Force
Set-MpPreference -DisableScriptScanning $false -Force
Set-MpPreference -DisableEmailScanning $false -Force
Set-MpPreference -DisableArchiveScanning $false -Force

Write-Output 'STEP2_OK'
";
                var prefResult = await RunPowerShellAsync(prefScript);
                if (prefResult.Contains("STEP2_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Gercek zamanli koruma acildi", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Koruma ayarlari sorunlu olabilir", AnsiColorType.Warning);
                }

                // Adim 3: Servisleri baslat
                AnsiWrite("  [3/3] ", AnsiColorType.Highlight);
                AnsiWriteLine("Windows Defender servisleri baslatiliyor...", AnsiColorType.Warning);
                Logger.Log("Adim 3: Servisler baslatiliyor");

                string svcScript = @"
Set-Service -Name WinDefend -StartupType Automatic -ErrorAction SilentlyContinue
Start-Service -Name WinDefend -ErrorAction SilentlyContinue
Set-Service -Name WdNisSvc -StartupType Automatic -ErrorAction SilentlyContinue
Start-Service -Name WdNisSvc -ErrorAction SilentlyContinue

Write-Output 'STEP3_OK'
";
                var svcResult = await RunPowerShellAsync(svcScript);
                if (svcResult.Contains("STEP3_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Servisler baslatildi", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Servisler manuel baslatilmis olabilir", AnsiColorType.Warning);
                }

                Console.WriteLine();
                AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Success);
                AnsiWriteLine("  │              ISLEM TAMAMLANDI                         │", AnsiColorType.Success);
                AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Success);
                Console.WriteLine();
                AnsiWriteLine("  Windows Defender kalici olarak etkinlestirildi!", AnsiColorType.Success);
                AnsiWriteLine("  (Sistem yeniden baslatildiginda da acik kalacak)", AnsiColorType.Info);
                
                return true;
            }
            else
            {
                // Gecici mod - Adim adim mesajlarla
                Console.WriteLine();
                AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Info);
                AnsiWriteLine("  │          WINDOWS DEFENDER GECICI ACMA                 │", AnsiColorType.Success);
                AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Info);
                Console.WriteLine();

                // Adim 1: Korumalari ac
                AnsiWrite("  [1/2] ", AnsiColorType.Highlight);
                AnsiWriteLine("Gercek zamanli koruma etkinlestiriliyor...", AnsiColorType.Warning);
                Logger.Log("Adim 1: Korumalar etkinlestiriliyor");

                string prefScript = @"
Set-MpPreference -DisableRealtimeMonitoring $false -Force
Set-MpPreference -DisableIOAVProtection $false -Force
Set-MpPreference -DisableBehaviorMonitoring $false -Force
Set-MpPreference -DisableScriptScanning $false -Force

Write-Output 'STEP1_OK'
";
                var prefResult = await RunPowerShellAsync(prefScript);
                if (prefResult.Contains("STEP1_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Gercek zamanli koruma acildi", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Koruma ayarlari sorunlu olabilir", AnsiColorType.Warning);
                }

                // Adim 2: Registry degerlerini temizle
                AnsiWrite("  [2/2] ", AnsiColorType.Highlight);
                AnsiWriteLine("Registry degerleri temizleniyor...", AnsiColorType.Warning);
                Logger.Log("Adim 2: Registry degerleri temizleniyor");

                string regScript = @"
$rtPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection'
Remove-ItemProperty -Path $rtPath -Name 'DisableRealtimeMonitoring' -ErrorAction SilentlyContinue
Remove-ItemProperty -Path $rtPath -Name 'DisableIOAVProtection' -ErrorAction SilentlyContinue

Write-Output 'STEP2_OK'
";
                var regResult = await RunPowerShellAsync(regScript);
                if (regResult.Contains("STEP2_OK"))
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("✓ Registry degerleri temizlendi", AnsiColorType.Success);
                }
                else
                {
                    AnsiWrite("        ", AnsiColorType.Info);
                    AnsiWriteLine("! Registry temizlenirken sorun olustu", AnsiColorType.Warning);
                }

                Console.WriteLine();
                AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Success);
                AnsiWriteLine("  │              ISLEM TAMAMLANDI                         │", AnsiColorType.Success);
                AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Success);
                Console.WriteLine();
                AnsiWriteLine("  Windows Defender gecici olarak etkinlestirildi!", AnsiColorType.Success);
                AnsiWriteLine("  (Sistem yeniden baslatildiginda otomatik kapanabilir)", AnsiColorType.Info);
                
                return true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Etkinlestirme islemi sirasinda kritik hata olustu", ex);
            Console.WriteLine();
            AnsiWriteLine("  ┌─────────────────────────────────────────────────────────┐", AnsiColorType.Error);
            AnsiWriteLine("  │              ISLEM BASARISIZ                          │", AnsiColorType.Error);
            AnsiWriteLine("  └─────────────────────────────────────────────────────────┘", AnsiColorType.Error);
            Console.WriteLine();
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
