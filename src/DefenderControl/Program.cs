using DefenderControl.Helpers;
using DefenderControl.Services;
using DefenderControl.Models;

namespace DefenderControl;

class Program
{
    private static readonly DefenderService _defenderService = new();
    private const string AppVersion = "v2.0.0";
    private const string Developer = "Eyup";
    private const string Github = "github.com/Eyupbayuk31";
    private const int BoxWidth = 58;

    static async Task Main(string[] args)
    {
        Console.Title = $"Windows Defender Kontrol Paneli - {AppVersion}";
        
        AdminHelper.CheckAndElevate();
        
        await ShowMainMenuAsync();
    }

    static async Task ShowMainMenuAsync()
    {
        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            PrintHeader();
            PrintFooter();
            
            Console.WriteLine();
            PrintMenuItem("1", "Durumu Goruntule", "Mevcut koruma durumunu gosterir");
            Console.WriteLine();
            PrintMenuItem("2", "Defender'i Kapat", "Koruma ozelliklerini devre disi birakir");
            Console.WriteLine();
            PrintMenuItem("3", "Defender'i Ac", "Koruma ozelliklerini etkinlestirir");
            Console.WriteLine();
            PrintMenuItem("4", "Cikis", "Uygulamadan cikis yap");
            
            Console.WriteLine();
            Console.WriteLine("  +" + new string('-', BoxWidth - 4) + "+");
            Console.Write($"  |  Seciminiz: ");
            
            var choice = Console.ReadLine();

            Console.WriteLine("  +" + new string('-', BoxWidth - 4) + "+");
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await ShowStatusAsync();
                    break;
                case "2":
                    await ShowDisableMenuAsync();
                    break;
                case "3":
                    await ShowEnableMenuAsync();
                    break;
                case "4":
                    exit = true;
                    break;
                default:
                    PrintError("Gecersiz secim! Lutfen 1-4 arasinda bir deger girin.");
                    break;
            }

            if (!exit)
            {
                Console.WriteLine();
                AnsiWrite("  Devam etmek icin bir tusa basin...", AnsiColorType.Info);
                Console.ReadKey();
            }
        }

        Console.Clear();
        PrintGoodbye();
    }

    static async Task ShowDisableMenuAsync()
    {
        Console.Clear();
        PrintHeader();
        
        Console.WriteLine();
        AnsiWriteLine("  +" + AnsiColor(" KAPATMA SECENEKLERI ", AnsiColorType.Error) + new string('-', 33) + "+", AnsiColorType.Error);
        Console.WriteLine($"  |{new string(' ', BoxWidth - 2)}|");
        
        PrintMenuItem("1", "Gecici Kapat", "Sistem yeniden baslatilinca otomatik acilir");
        Console.WriteLine();
        PrintMenuItem("2", "Kalici Kapat", "Sistem yeniden baslatildiginda da kapali kalir");
        Console.WriteLine();
        PrintMenuItem("3", "Geri", "Ana menuye don");
        
        Console.WriteLine();
        Console.WriteLine("  +" + new string('-', BoxWidth - 4) + "+");
        Console.Write($"  |  Seciminiz: ");
        
        var choice = Console.ReadLine();
        Console.WriteLine("  +" + new string('-', BoxWidth - 4) + "+");
        Console.WriteLine();

        bool success = false;
        
        switch (choice)
        {
            case "1":
                success = await _defenderService.DisableDefenderAsync(false);
                break;
            case "2":
                success = await _defenderService.DisableDefenderAsync(true);
                break;
            case "3":
                return;
            default:
                PrintError("Gecersiz secim!");
                break;
        }

        if (success)
        {
            Console.WriteLine();
            AnsiWriteLine("  +=============================================================+", AnsiColorType.Success);
            AnsiWriteLine("  |                    ISLEM BASARILI                           |", AnsiColorType.Success);
            AnsiWriteLine("  +=============================================================+", AnsiColorType.Success);
        }
    }

    static async Task ShowEnableMenuAsync()
    {
        Console.Clear();
        PrintHeader();
        
        Console.WriteLine();
        AnsiWriteLine("  +" + AnsiColor(" ACMA SECENEKLERI ", AnsiColorType.Success) + new string('-', 35) + "+", AnsiColorType.Success);
        Console.WriteLine($"  |{new string(' ', BoxWidth - 2)}|");
        
        PrintMenuItem("1", "Gecici Ac", "Sistem yeniden baslatilinca otomatik kapanir");
        Console.WriteLine();
        PrintMenuItem("2", "Kalici Ac", "Sistem yeniden baslatildiginda da acik kalir");
        Console.WriteLine();
        PrintMenuItem("3", "Geri", "Ana menuye don");
        
        Console.WriteLine();
        Console.WriteLine("  +" + new string('-', BoxWidth - 4) + "+");
        Console.Write($"  |  Seciminiz: ");
        
        var choice = Console.ReadLine();
        Console.WriteLine("  +" + new string('-', BoxWidth - 4) + "+");
        Console.WriteLine();

        bool success = false;
        
        switch (choice)
        {
            case "1":
                success = await _defenderService.EnableDefenderAsync(false);
                break;
            case "2":
                success = await _defenderService.EnableDefenderAsync(true);
                break;
            case "3":
                return;
            default:
                PrintError("Gecersiz secim!");
                break;
        }

        if (success)
        {
            Console.WriteLine();
            AnsiWriteLine("  +=============================================================+", AnsiColorType.Success);
            AnsiWriteLine("  |                    ISLEM BASARILI                           |", AnsiColorType.Success);
            AnsiWriteLine("  +=============================================================+", AnsiColorType.Success);
        }
    }

    static void PrintHeader()
    {
        string topLine = "+" + new string('=', BoxWidth - 2) + "+";
        
        Console.WriteLine();
        AnsiWriteLine("  " + topLine, AnsiColorType.Primary);
        
        // ASCII Art
        string[] asciiArt = new[]
        {
            "   █████╗ ██████╗  ██████╗██╗  ██╗",
            "  ██╔══██╗██╔══██╗██╔════╝██║ ██╔╝",
            "  ███████║██████╔╝██║     █████╔╝ ",
            "  ██╔══██║██╔══██╗██║     ██╔═██╗ ",
            "  ██║  ██║██║  ██║╚██████╗██║  ██╗",
            "  ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝"
        };
        
        foreach (var line in asciiArt)
        {
            Console.WriteLine($"  |{AnsiColor(line.PadRight(BoxWidth - 2), AnsiColorType.Primary)}|");
        }
        
        Console.WriteLine("  |" + new string(' ', BoxWidth - 2) + "|");
        
        // Title
        string title = "WINDOWS DEFENDER KONTROL PANELI";
        int titlePadding = (BoxWidth - 2 - title.Length) / 2;
        Console.WriteLine($"  |{new string(' ', titlePadding)}{AnsiColor(title, AnsiColorType.Header)}{new string(' ', BoxWidth - 2 - titlePadding - title.Length)}|");
        
        // Version
        string version = $"{AppVersion}";
        int versionPadding = (BoxWidth - 2 - version.Length) / 2;
        Console.WriteLine($"  |{new string(' ', versionPadding)}{AnsiColor(version, AnsiColorType.Info)}{new string(' ', BoxWidth - 2 - versionPadding - version.Length)}|");
        
        Console.WriteLine("  |" + new string(' ', BoxWidth - 2) + "|");
        AnsiWriteLine("  " + topLine, AnsiColorType.Primary);
    }

    static void PrintFooter()
    {
        Console.WriteLine();
        AnsiWrite("  |  Developer: ", AnsiColorType.Info);
        AnsiWriteLine($"{Developer} {AnsiColor("<3", AnsiColorType.Error)}", AnsiColorType.Success);
        AnsiWrite("  |  Source: ", AnsiColorType.Info);
        AnsiWriteLine(Github, AnsiColorType.Link);
    }

    static void PrintMenuItem(string number, string title, string description)
    {
        string topLine = "+-" + AnsiColor($"[{number}]", AnsiColorType.Highlight) + "-" + AnsiColor(title, AnsiColorType.Primary) + new string('-', 40 - title.Length) + "+";
        Console.WriteLine($"  {topLine}");
        
        string descLine = "|   " + AnsiColor(">>", AnsiColorType.Highlight) + " " + AnsiColor(description, AnsiColorType.Secondary) + new string(' ', 42 - description.Length) + "|";
        Console.WriteLine($"  {descLine}");
        
        Console.WriteLine($"  +{new string('-', BoxWidth - 2)}+");
    }

    static async Task ShowStatusAsync()
    {
        AnsiWrite("  [*] Windows Defender durumu aliniyor...\n", AnsiColorType.Warning);

        var status = await _defenderService.GetStatusAsync();

        // Status box header
        string headerBox = "+" + AnsiColor($" DURUM BILGILERI ", AnsiColorType.Header) + new string('-', 40) + "+";
        Console.WriteLine($"  {headerBox}");
        Console.WriteLine($"  |{new string(' ', BoxWidth - 2)}|");
        
        // Protection statuses
        PrintStatusRow("Gercek Zamanli Koruma", status.IsRealTimeProtectionEnabled);
        PrintStatusRow("IOAV Korumasi", status.IsIoavProtectionEnabled);
        PrintStatusRow("Davranis Izleme", status.IsBehaviorMonitorEnabled);
        PrintStatusRow("Antivirus Korumasi", status.IsAntivirusEnabled);
        
        Console.WriteLine($"  |{new string(' ', BoxWidth - 2)}|");
        Console.WriteLine($"  +{new string('-', BoxWidth - 2)}+");
        Console.WriteLine($"  |{new string(' ', BoxWidth - 2)}|");
        
        // Info rows
        PrintInfoRow("Son Tarama", status.LastScanTime ?? "Bilinmiyor");
        PrintInfoRow("Virus Tanimlari", status.AntivirusSignatureVersion ?? "Bilinmiyor");
        PrintInfoRow("Casus Yazilim Tanim", status.AntispywareSignatureVersion ?? "Bilinmiyor");
        PrintInfoRow("Motor Versiyonu", status.EngineVersion ?? "Bilinmiyor");
        
        Console.WriteLine($"  |{new string(' ', BoxWidth - 2)}|");
        Console.WriteLine($"  +{new string('-', BoxWidth - 2)}+");
    }

    static void PrintStatusRow(string label, bool isEnabled)
    {
        var statusText = isEnabled ? "[AKTIF]" : "[DEVRE DISI]";
        var statusColor = isEnabled ? AnsiColorType.Success : AnsiColorType.Error;
        
        Console.Write($"  |  {AnsiColor(">>", AnsiColorType.Info)} {label}: ");
        Console.Write(AnsiColor(statusText, statusColor));
        Console.WriteLine(new string(' ', 35 - label.Length - statusText.Length) + "|");
    }

    static void PrintInfoRow(string label, string value)
    {
        var displayValue = value.Length > 28 ? value.Substring(0, 25) + "..." : value;
        Console.Write($"  |  {AnsiColor("--", AnsiColorType.Secondary)} {AnsiColor(label + ":", AnsiColorType.Secondary)} ");
        Console.Write(AnsiColor(displayValue.PadRight(28), AnsiColorType.Info));
        Console.WriteLine(new string(' ', 36 - label.Length - displayValue.Length) + "|");
    }

    static void PrintGoodbye()
    {
        string byeBox = "+" + AnsiColor($" GORUSURUZ ", AnsiColorType.Success) + new string('-', 42) + "+";
        Console.WriteLine($"  {byeBox}");
        Console.WriteLine($"  |{new string(' ', BoxWidth - 2)}|");
        
        AnsiWriteLine("  |  Cikis yaptiniz. Guvenli kalin!                  |", AnsiColorType.Success);
        Console.WriteLine($"  |{new string(' ', BoxWidth - 2)}|");
        AnsiWriteLine($"  |  {Developer} tarafindan kodlandi                        |", AnsiColorType.Info);
        AnsiWriteLine($"  |  {Github}                 |", AnsiColorType.Link);
        Console.WriteLine($"  |{new string(' ', BoxWidth - 2)}|");
        Console.WriteLine($"  +{new string('-', BoxWidth - 2)}+");
        Console.WriteLine();
    }

    static void PrintError(string message)
    {
        AnsiWrite($"  ! {message}", AnsiColorType.Error);
        Console.WriteLine();
    }

    // ANSI Renk Kodlari
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
            AnsiColorType.Highlight => "\u001b[1;33m",  // Bold Yellow
            _ => "\u001b[0m"
        };
        
        return $"{colorCode}{text}\u001b[0m";
    }

    static void AnsiWrite(string text, AnsiColorType colorType)
    {
        Console.Write(AnsiColor(text, colorType));
    }

    static void AnsiWriteLine(string text, AnsiColorType colorType)
    {
        Console.WriteLine(AnsiColor(text, colorType));
    }
}
