# 🛡️ Windows Defender Kontrol Paneli

**Gelistirici:** Eyüp  
**Kaynak:** github.com/Eyupbayuk31

Windows Defender antivirüs yazılımını komut satırından açıp kapatmaya yarayan C# Console Application.

## 📋 Ozellikler

- ✅ **Durum Goruntuleme:** Windows Defender'ın mevcut koruma durumunu gosterir
- 🔴 **Defender'ı Kapatma:** Tüm koruma ozelliklerini devre disi birakir
- 🟢 **Defender'ı Acma:** Tüm koruma ozelliklerini etkinlestirir
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

| Secenek | Aciklama |
|---------|----------|
| `1` | Mevcut Defender durumunu goruntuler |
| `2` | Defender'ı kapatır (koruma devre disi) |
| `3` | Defender'ı acar (koruma aktif) |
| `4` | Uygulamadan cıkar |

## ⚠️ Onemli Uyari

1. **Defender Kapatildiginda:** Sisteminiz virus ve kotu amacli yazilimlara karsi savunmasiz kalacaktir
2. **Gecici Degisiklikler:** Degisiklikler sistem yeniden baslatilinana kadar kalicidir
3. **Guvenlik:** Sadece guvenilir ortamlarda kullanin

## 🔧 Teknik Detaylar

### Kullanilan Teknolojiler

- **Dil:** C# 12
- **Framework:** .NET 8.0
- **Kontrol Yontemi:** PowerShell `Set-MpPreference` komutlari

### PowerShell Komutlari

**Kapatma:**
```powershell
Set-MpPreference -DisableRealtimeMonitoring $true
Set-MpPreference -DisableIOAVProtection $true
Set-MpPreference -DisableBehaviorMonitoring $true
Set-MpPreference -DisableAntivirus $true
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
