# 🛡️ Windows Defender Kontrol Paneli

**Geliştirici:** Eyüp
**GitHub:** [github.com/Eyupbayuk31/DefenderControl](https://github.com/Eyupbayuk31/DefenderControl)
**Versiyon:** v2.1.0

> ⚠️ **BU BİR ZARARLI YAZILIM DEĞİLDİR!** Bu uygulama tamamen eğitim amaçlı geliştirilmiştir.
> Windows Defender'ı açmak ve kapatmak için kullanılır. Tüm kaynak kodlar açık kaynaklıdır ve incelenebilir.

---

## ❓ Bu Uygulama Nedir?

Bu uygulama, **kendi bilgisayarınızda** Windows Defender antivirüs yazılımını
komut satırından (CLI) kontrol etmenizi sağlar. PowerShell komutları kullanarak
Defender'ın hangi özelliklerinin açık veya kapalı olduğunu görüntüleyebilir
ve bu özellikleri yönetebilirsiniz.

### Neden Bu Uygulama?

- **Eğitim Amaçlı:** Windows Defender'ın nasıl çalıştığını öğrenmek için
- **Geliştiriciler İçin:** Yazılım geliştirirken Defender'ın tarama davranışını test etmek için
- **PowerShell Öğrenme:** PowerShell komutlarının nasıl kullanıldığını görmek için

---

## ⚠️ ÖNEMLİ UYARI

> **DİKKAT: Bu uygulama Windows Defender'ı devre dışı bırakmak için tasarlanmıştır!**

- Uygulama çalıştırıldığında antivirüs yazılımınız kapatılacaktır
- Geçici kapatma seçeneği sistem yeniden başlatıldığında otomatik açılır
- Kalıcı kapatma seçeneği sistem yeniden başlatıldığında da kapalı kalır
- **Sadece güvenilir ve kontrol ettiğiniz ortamlarda kullanın**
- **Antivirüs kapatıldıktan sonra sistem güvensiz kalacaktır**
- **İşiniz bittiğinde Defender'ı mutlaka tekrar açın**

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

### v2.1.0 Yenilikleri

- **Tam Kapatma Desteği:** Artık tüm Windows Defender bileşenleri kapatılıyor
- **Genişletilmiş Koruma Kapatma:** Gerçek zamanlı koruma, IOAV, davranış izleme, betik tarama, e-posta tarama, arşiv tarama ve daha fazlası
- **Tamper Protection Kapatma:** Windows Defender'ın kendini koruması devre dışı bırakılıyor
- **Servis Yönetimi:** WinDefend ve WdNisSvc servisleri durduruluyor/baslatılıyor
- **Windows Security Center:** Güvenlik merkezi ayarları yönetiliyor
- **Güvenilir Sonuç Kontrolü:** İşlem sonrası başarı mesajı gösterimi düzeltildi

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
| **Kalıcı Mod** | Windows Registry (Group Policy) |
| **UI** | ANSI Renkli Konsol Arayüzü |

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

### Kapatırken Yapılan İşlemler (Açık Kaynak Kod)

**Geçici Kapatma:**
1. `Set-MpPreference` komutları ile anlık koruma ayarlarını değiştirir
2. Registry'de geçici ayarlar yapar
3. Sistem yeniden başlatılınca Defender otomatik açılır

**Kalıcı Kapatma:**
1. `HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender` yolunu oluşturur
2. Group Policy ayarlarını yapılandırır
3. DisableAntiSpyware, DisableRealtimeProtection, DisableOnAccessProtection ayarlarını yapar
4. Tamper Protection'ı devre dışı bırakır
5. WinDefend ve WdNisSvc servislerini durdurur ve devre dışı bırakır
6. Windows Security Center ayarlarını değiştirir

### PowerShell Komutları (Eğitim Amaçlı)

```powershell
# Durum bilgisi alma
Get-MpComputerStatus

# Korumayı kapatma (Geçici)
Set-MpPreference -DisableRealtimeMonitoring $true

# Korumayı açma (Geçici)
Set-MpPreference -DisableRealtimeMonitoring $false

# Registry ile kalıcı kapatma
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows Defender' -Name 'DisableAntiSpyware' -Value 1
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

**🔍 Şüphe mi?:**
Kaynak kodları inceleyebilirsiniz. Bu uygulama:
- Hiçbir veri göndermez
- Arkakapı oluşturmaz
- Kullanıcı izlemez
- Sadece yerel PowerShell komutları çalıştırır
