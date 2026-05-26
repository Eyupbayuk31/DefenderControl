# 🛡️ Windows Defender Kontrol Paneli

**Geliştirici:** Eyüp  
**GitHub:** [github.com/Eyupbayuk31/DefenderControl](https://github.com/Eyupbayuk31/DefenderControl)
**Versiyon:** v2.0.0

Windows Defender antivirüs yazılımını komut satırından açıp kapatmaya yarayan C# Console Application.

---

## ⚠️ ÖNEMLİ UYARI - ANTİVİRÜS UYARISI

> **DİKKAT: Bu uygulama Windows Defender'ı devre dışı bırakmak için tasarlanmıştır!**
> 
> - Uygulama çalıştırıldığında antivirüs yazılımınız kapatılacaktır
> - Geçici kapatma seçeneği sistem yeniden başlatıldığında otomatik açılır
> - Kalıcı kapatma seçeneği sistem yeniden başlatıldığında da kapalı kalır
> - **Sadece güvenilir ve kontrol ettiğiniz ortamlarda kullanın**
> - **Antivirüs kapatıldıktan sonra sistem güvensiz kalacaktır**
> - İşiniz bittiğinde Defender'ı mutlaka tekrar açın

---

## 📋 Özellikler

| Özellik | Açıklama |
|---------|----------|
| ✅ **Durum Görüntüleme** | Windows Defender'ın mevcut koruma durumunu gösterir |
| 🔴 **Defender'ı Geçici Kapatma** | Sistem yeniden başlatılınca otomatik açılır |
| 🔴 **Defender'ı Kalıcı Kapatma** | Sistem yeniden başlatıldığında da kapalı kalır |
| 🟢 **Defender'ı Geçici Açma** | Sistem yeniden başlatılınca otomatik kapanır |
| 🟢 **Defender'ı Kalıcı Açma** | Sistem yeniden başlatıldığında da açık kalır |
| 🔐 **Admin Yetkisi** | Otomatik yönetici yetkisi kontrolü |
| 📝 **Loglama** | Tüm işlemler Desktop'ta log dosyasına kaydedilir |
| 🎨 **ANSI Renkli Arayüz** | Modern ve şık konsol arayüzü |

---

## 🚀 Kullanım

### Gereksinimler

- Windows 10/11
- .NET 8.0 SDK veya üzeri
- Yönetici (Administrator) yetkisi

### Kurulum

```bash
# Projeyi klonlayın
git clone https://github.com/Eyupbayuk31/DefenderControl.git

# Proje dizinine gidin
cd Defender
```

### Derleme

```bash
cd src/DefenderControl
dotnet build -c Release
```

### Çalıştırma

```bash
dotnet run
```

veya derleme sonrası:

```
src\DefenderControl\bin\Release\net8.0-windows\DefenderControl.exe
```

### Yayınlama (Tek Dosya)

```bash
cd src/DefenderControl
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## 📖 Menü Seçenekleri

### Ana Menü

| Seçenek | Açıklama |
|---------|----------|
| `1` | Mevcut Defender durumunu görüntüler |
| `2` | Kapatma seçenekleri menüsüne gider |
| `3` | Açma seçenekleri menüsüne gider |
| `4` | Uygulamadan çıkar |

### Kapatma Menüsü

| Seçenek | Açıklama |
|---------|----------|
| `1` | **Geçici Kapat** - Sistem yeniden başlatınca otomatik açılır |
| `2` | **Kalıcı Kapat** - Sistem yeniden başlatıldığında da kapalı kalır |
| `3` | Geri dön |

### Açma Menüsü

| Seçenek | Açıklama |
|---------|----------|
| `1` | **Geçici Aç** - Sistem yeniden başlatınca otomatik kapanır |
| `2` | **Kalıcı Aç** - Sistem yeniden başlatıldığında da açık kalır |
| `3` | Geri dön |

---

## 🔧 Teknik Detaylar

### Kullanılan Teknolojiler

| Teknoloji | Değer |
|-----------|-------|
| **Dil** | C# 12 |
| **Framework** | .NET 8.0 |
| **Hedef Platform** | Windows 10/11 |
| **Kontrol Yöntemi** | PowerShell `Set-MpPreference` komutları |
| **Kalıcı Mod** | Windows Registry |

### Mimari Yapı

```
DefenderControl/
├── Program.cs              # Ana uygulama ve menü sistemi
├── DefenderControl.csproj  # Proje dosyası
├── app.manifest           # Admin yetkisi manifesti
├── Services/
│   └── DefenderService.cs # Defender kontrol PowerShell komutları
├── Helpers/
│   ├── AdminHelper.cs     # Admin yetkisi kontrolü
│   └── Logger.cs          # Loglama sistemi
└── Models/
    └── DefenderStatus.cs  # Durum modeli
```

### PowerShell Komutları

**Geçici Kapatma:**
```powershell
Set-MpPreference -DisableRealtimeMonitoring $true
Set-MpPreference -DisableIOAVProtection $true
Set-MpPreference -DisableBehaviorMonitoring $true
```

**Kalıcı Kapatma (Registry ile):**
```powershell
# Registry yolu oluştur
New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender' -Force
New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Force

# Registry değerlerini ayarla
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender' -Name 'DisableAntiSpyware' -Value 1
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection' -Name 'DisableRealtimeMonitoring' -Value 1
```

**Açma:**
```powershell
Set-MpPreference -DisableRealtimeMonitoring $false
Set-MpPreference -DisableIOAVProtection $false
Set-MpPreference -DisableBehaviorMonitoring $false
```

---

## 📝 Log Dosyası

Tüm işlemler otomatik olarak Desktop'ta `DefenderControl_Log.txt` dosyasına kaydedilir.

---

## ⚠️ Güvenlik Notları

1. **Geçici Kapatma:** Sistem yeniden başlatıldığında Defender otomatik olarak açılır
2. **Kalıcı Kapatma:** Registry ve Group Policy ile kalıcı olarak devre dışı bırakılır
3. **Güvenlik:** Sadece güvenilir ortamlarda kullanın
4. **Kullanım Sonrası:** İşiniz bittiğinde Defender'ı mutlaka tekrar açın

---

## 📝 Lisans

Bu proje eğitim amaçlı geliştirilmiştir. Kendi sorumluluğunuzda kullanın.

---

**💡 İpucu:** Bu uygulamayı kullanırken antivirüs yazılımınız devre dışı kalacağından, indirdiğiniz dosyalara dikkat edin ve sadece güvenilir kaynaklardan dosya indirin.
