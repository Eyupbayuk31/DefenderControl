# 🛡️ Windows Defender Kontrol Paneli

**Gelistirici:** Eyüp  
**Kaynak:** github.com/Eyupbayuk31

Windows Defender antivirüs yazılımını komut satırından açıp kapatmaya yarayan C# Console Application.

## 📋 Ozellikler

- ✅ **Durum Goruntuleme:** Windows Defender'ın mevcut koruma durumunu gosterir
- 🔴 **Defender'ı Gecici Kapatma:** Sistem yeniden baslatilinca otomatik acilir
- 🔴 **Defender'ı Kalici Kapatma:** Sistem yeniden baslatildiginda da kapali kalir
- 🟢 **Defender'ı Gecici Acma:** Sistem yeniden baslatilinca otomatik kapanir
- 🟢 **Defender'ı Kalici Acma:** Sistem yeniden baslatildiginda da acik kalir
- 🔐 **Admin Yetkisi:** Otomatik yonetici yetkisi kontrolu
- ⚠️ **Guvenlik Uyariari:** Kritik islemler için onay mesajlari

## 🚀 Kullanim

### Gereksinimler

- Windows 10/11
- .NET 8.0 SDK veya uzeri
- Yonetici (Administrator) yetkisi

### Derleme

```bash
cd src/DefenderControl
dotnet build -c Release
```

### Calistirma

```bash
dotnet run
```

veya derleme sonrası:

```
src/DefenderControl/bin/Release/net8.0-windows/DefenderControl.exe
```

## 📖 Menu Secenekleri

### Ana Menu

| Secenek | Aciklama |
|---------|----------|
| `1` | Mevcut Defender durumunu goruntuler |
| `2` | Kapatma secenekleri menusune gider |
| `3` | Acma secenekleri menusune gider |
| `4` | Uygulamadan cikar |

### Kapatma Menu

| Secenek | Aciklama |
|---------|----------|
| `1` | **Gecici Kapat** - Sistem yeniden baslatinca otomatik acilir |
| `2` | **Kalici Kapat** - Sistem yeniden baslatinca da kapali kalir |
| `3` | Geri don |

### Acma Menu

| Secenek | Aciklama |
|---------|----------|
| `1` | **Gecici Ac** - Sistem yeniden baslatinca otomatik kapanir |
| `2` | **Kalici Ac** - Sistem yeniden baslatinca da acik kalir |
| `3` | Geri don |

## ⚠️ Onemli Uyari

1. **Gecici Kapatma:** Sistem yeniden baslatildiginda Defender otomatik olarak acilir
2. **Kalici Kapatma:** Registry ve Group Policy ile kalici olarak devre disi birakilir
3. **Guvenlik:** Sadece guvenilir ortamlarda kullanin

## 🔧 Teknik Detaylar

### Kullanilan Teknolojiler

- **Dil:** C# 12
- **Framework:** .NET 8.0
- **Kontrol Yontemi:** PowerShell `Set-MpPreference` komutlari
- **Kalici Mod:** Windows Registry `HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender`

### PowerShell Komutlari

**Gecici Kapatma:**
```powershell
Set-MpPreference -DisableRealtimeMonitoring $true
Set-MpPreference -DisableIOAVProtection $true
Set-MpPreference -DisableBehaviorMonitoring $true
Set-MpPreference -DisableAntivirus $true
```

**Kalici Kapatma:**
```powershell
# Registry ile
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender' -Name 'DisableAntiSpyware' -Value 1
# Ve sonra Set-MpPreference
```

**Acma:**
```powershell
Set-MpPreference -DisableRealtimeMonitoring $false
Set-MpPreference -DisableIOAVProtection $false
Set-MpPreference -DisableBehaviorMonitoring $false
Set-MpPreference -DisableAntivirus $false
```

## 📝 Lisans

Bu proje egitim amacli gelistirilmistir. Kendi sorumlulugunuzda kullanin.
