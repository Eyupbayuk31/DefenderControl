# Windows Defender Kontrol Uygulamasi - Proje Plani

## 📋 Proje Ozeti

Windows Defender antivirüs yazılımını komut satırından açıp kapatmaya yarayan bir Console Application.

## 🎯 Hedefler

- Tek tıkla Windows Defender'ı kalıcı olarak acma
- Tek tıkla Windows Defender'ı kalıcı olarak kapatma
- Mevcut durumu goruntuleme
- Admin yetkisi gerektirme

## 🛠 Teknik Gereksinimler

| Gereksinim | Deger |
|------------|-------|
| Dil | C# / .NET |
| Proje Tipi | Console Application |
| Hedef Framework | .NET 8.0 |
| Isletim Sistemi | Windows 10/11 |
| Yetki | Administrator |

## 📁 Proje Yapisi

```
DefenderControl/
├── DefenderControl.slnx
├── README.md
├── PLAN.md
└── src/
    └── DefenderControl/
        ├── DefenderControl.csproj
        ├── app.manifest
        ├── Program.cs
        ├── Services/
        │   └── DefenderService.cs
        ├── Helpers/
        │   └── AdminHelper.cs
        └── Models/
            └── DefenderStatus.cs
```

## 🔧 Teknik Yaklasim

### 1. Windows Defender Kontrol Yontemi

Windows Defender'ı kontrol etmek icin **PowerShell** komutları kullanilacak.

### 2. Admin Yetkisi Kontrolü

Uygulama baslatildiginda Windows Identity sınıfı ile admin kontrolü yapilacak.

## ⚠️ Dikkat Edilecek Hususlar

1. **Kalıcılık:** Degisiklikler sistem yeniden baslatilina kadar kalıcıdır
2. **Guvenlik:** Defender kapatildiginda sistem guvensiz kalacagi icin uyari mesaji gosterilecek
3. **Geri Donus:** Acma islemi her zaman basarili olmalı
