# 🚀 DMS Migration - Enterprise File Migration Tool

**4 TB+ dosya setlerini yüksek performansla taşıyan ve Oracle veritabanına metadata kaydeden .NET 9 uygulaması.**

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Oracle](https://img.shields.io/badge/Oracle-Database-red)](https://www.oracle.com/database/)
[![Dapper](https://img.shields.io/badge/Dapper-ORM-blue)](https://github.com/DapperLib/Dapper)

## ✨ Özellikler

- ✅ **Yüksek Performans**: Dapper ile optimize edilmiş DB işlemleri
- ✅ **4 TB+ Destek**: Büyük dosya setleri için özel olarak tasarlandı
- ✅ **State Management**: Kaldığı yerden devam edebilme
- ✅ **Batch Processing**: Bellek dostu toplu işleme
- ✅ **Retry Mechanism**: Otomatik hata yönetimi ve tekrar deneme
- ✅ **Template System**: Dosya tipine göre özelleştirilebilir metadata (KOF ve Default)
- ✅ **Transaction Safety**: Veri bütünlüğü garantisi
- ✅ **Detailed Logging**: Kapsamlı log kayıtları
- ✅ **Oracle Database**: Enterprise-grade veritabanı desteği
- ✅ **Duplicate Management**: Duplicate dosya yönetimi

## Proje Yapısı

```
DMSMigration/
├── Program.cs                      # Ana program ve DI yapılandırması
├── appsettings.json                # Konfigürasyon
├── DMSMigration.csproj             # Proje dosyası
├── Core/
│   ├── Entities/                   # Veritabanı entity'leri
│   │   ├── DmsDocument.cs
│   │   ├── DmsDocumentIndex.cs
│   │   └── DmsDocumentVersion.cs
│   ├── Enums/
│   │   └── MigrationStatus.cs      # Migration durumları
│   └── Models/
│       ├── MigrationResult.cs      # Migration sonuç modeli
│       ├── FileMetadata.cs         # Dosya metadata modeli
│       └── FileState.cs            # State management modeli
├── Services/
│   ├── Interfaces/                 # Servis interface'leri
│   ├── FileService.cs              # Dosya işlemleri
│   ├── DocumentService.cs          # Veritabanı işlemleri
│   ├── MigrationService.cs         # Ana migration servisi
│   └── Templates/
│       ├── KofTemplateService.cs   # KOF dosyaları için template
│       └── DefaultTemplateService.cs # Diğer dosyalar için template
├── Data/
│   └── ApplicationDbContext.cs     # EF Core DbContext
├── Infrastructure/
│   └── MigrationStateManager.cs    # State yönetimi
├── dmsfiles/                       # Hedef dizin (oluşturulacak)
└── Logs/                           # Log dosyaları (oluşturulacak)
```

## Kurulum

### Gereksinimler

- .NET 9 SDK
- Oracle Database (12c veya üzeri)

### Adımlar

1. Repository'yi clone'layın:
```bash
git clone <repository-url>
cd DmsMigrate
```

2. `appsettings.json` dosyasında Oracle bağlantı bilgilerini güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=your-host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=your-service)));User Id=your-user;Password=your-password;"
  }
}
```

3. `MigrationSettings` bölümünde source ve target path'leri ayarlayın:
```json
{
  "MigrationSettings": {
    "SourcePath": "C:\\YourSourcePath",
    "TargetPath": "dmsfiles"
  }
}
```

4. Veritabanı tablolarını oluşturun (gerekirse migration kullanın veya manuel SQL script çalıştırın).

5. Paketleri yükleyin:
```bash
dotnet restore
```

6. Uygulamayı çalıştırın:
```bash
dotnet run
```

## Kullanım

Uygulama başladığında size 3 seçenek sunar:

```
=== DMS Dosya Migration Uygulaması ===
1. Sıfırdan başlat
2. Hatalıları tekrar çalıştır
3. Kaldığı yerden devam et

Seçiminiz: 
```

### 1. Sıfırdan Başlat
- Tüm state'i temizler
- Source dizinindeki tüm desteklenen dosyaları işler
- Her dosya için state kaydı oluşturur

### 2. Hatalıları Tekrar Çalıştır
- Sadece `Failed` durumundaki dosyaları yeniden işler
- Retry sayacını artırır
- Max retry count'a ulaşan dosyaları atlar

### 3. Kaldığı Yerden Devam Et
- `Pending` durumundaki dosyaları işler
- Önceki başarılı işlemleri tekrarlamaz
- Kesintiye uğramış migration'ları tamamlar

## Konfigürasyon

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Oracle bağlantı string'i"
  },
  "MigrationSettings": {
    "SourcePath": "Kaynak dosya dizini",
    "TargetPath": "Hedef dosya dizini (dmsfiles)",
    "BatchSize": 100,                          // Her batch'te işlenecek dosya sayısı
    "MaxRetryCount": 3,                        // Maksimum yeniden deneme sayısı
    "StateFilePath": "migration-state.json",   // State dosyası yolu
    "ErrorLogPath": "migration-errors.log",    // Hata log dosyası
    "SupportedExtensions": [                   // Desteklenen dosya uzantıları
      ".pdf", ".docx", ".xlsx", ".jpg", ".png"
    ],
    "DefaultTenantId": 1,                      // Varsayılan tenant ID
    "DefaultCreatorUserId": 1                  // Varsayılan creator user ID
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

## Template Servisleri

### KofTemplateService
- `KOF_` ile başlayan dosyaları işler
- Dosya adı pattern'i: `KOF_12345678_Document.pdf`
- İlk 8 haneyi (KOF_ prefix'inden sonra) proje numarası olarak parse eder
- TypeId: 1
- Index'ler:
  - `ProjectNo`: Proje numarası (8 hane)
  - `DocumentType`: "KOF"

### DefaultTemplateService
- Diğer tüm dosyaları işler
- TypeId: 99
- Index'ler:
  - `FileName`: Dosya adı

### Yeni Template Ekleme

Yeni bir template eklemek için:

1. `ITemplateService` interface'ini implement edin
2. `CanHandle` metodunda dosya adı kontrolü yapın
3. `EnrichMetadata` metodunda TypeId ve Index'leri set edin
4. `Program.cs` içinde DI container'a ekleyin

```csharp
public class CustomTemplateService : ITemplateService
{
    public bool CanHandle(string fileName)
    {
        return fileName.StartsWith("CUSTOM_");
    }

    public void EnrichMetadata(FileMetadata metadata)
    {
        metadata.TypeId = 2;
        metadata.Indexes["CustomKey"] = "CustomValue";
    }
}
```

## Veritabanı Şeması

### DMS_DOCUMENT
| Column | Type | Description |
|--------|------|-------------|
| ID | NUMBER(10) | Primary Key |
| FILE_NAME | NVARCHAR2(500) | Dosya adı |
| EXTENSION | NVARCHAR2(500) | Dosya uzantısı |
| PATH | NVARCHAR2(500) | Dosya yolu |
| SIZE | NUMBER(19) | Dosya boyutu (byte) |
| TYPE_ID | NUMBER(10) | Döküman tipi |
| CURRENT_VERSION | NUMBER(10) | Güncel versiyon |
| CREATION_TIME | TIMESTAMP(7) | Oluşturulma zamanı |
| CREATOR_USER_ID | NUMBER(19) | Oluşturan kullanıcı |
| LAST_MODIFICATION_TIME | TIMESTAMP(7) | Son değişiklik zamanı |
| LAST_MODIFIER_USER_ID | NUMBER(19) | Son değiştiren kullanıcı |
| IS_DELETED | NUMBER(1) | Silinmiş mi? |
| DELETER_USER_ID | NUMBER(19) | Silen kullanıcı |
| DELETION_TIME | TIMESTAMP(7) | Silinme zamanı |
| TENANT_ID | NUMBER(10) | Tenant ID |

### DMS_DOCUMENT_INDEX
| Column | Type | Description |
|--------|------|-------------|
| ID | NUMBER(10) | Primary Key |
| DOCUMENT_ID | NUMBER(10) | Foreign Key -> DMS_DOCUMENT |
| INDEX_KEY | NVARCHAR2(100) | Index anahtarı |
| INDEX_VALUE | NVARCHAR2(500) | Index değeri |
| CREATION_TIME | TIMESTAMP(7) | Oluşturulma zamanı |

### DMS_DOCUMENT_VERSION
| Column | Type | Description |
|--------|------|-------------|
| ID | NUMBER(10) | Primary Key |
| DOCUMENT_ID | NUMBER(10) | Foreign Key -> DMS_DOCUMENT |
| VERSION_NUMBER | NUMBER(10) | Versiyon numarası |
| FILE_NAME | NVARCHAR2(500) | Dosya adı |
| PATH | NVARCHAR2(500) | Dosya yolu |
| SIZE | NUMBER(19) | Dosya boyutu |
| CREATION_TIME | TIMESTAMP(7) | Oluşturulma zamanı |
| CREATOR_USER_ID | NUMBER(19) | Oluşturan kullanıcı |

## State Management

Uygulama her dosyanın durumunu `migration-state.json` dosyasında saklar:

```json
{
  "C:\\SourceFiles\\document.pdf": {
    "FilePath": "C:\\SourceFiles\\document.pdf",
    "Status": "Success",
    "LastUpdated": "2024-01-15T10:30:00",
    "ErrorMessage": null,
    "RetryCount": 0
  }
}
```

### Migration Durumları

- **Pending**: İşlenmeyi bekliyor
- **Processing**: Şu anda işleniyor
- **Success**: Başarıyla tamamlandı
- **Failed**: Hata oluştu
- **Skipped**: Atlandı (max retry'a ulaşıldı)

## Loglama

### Console Logging
- Gerçek zamanlı ilerleme bilgisi
- Başarılı/hatalı işlemler
- Özet istatistikler

### File Logging
- Detaylı log dosyaları: `Logs/migration-{Date}.txt`
- Log seviyeleri:
  - **Information**: Genel bilgiler, ilerleme
  - **Debug**: Detaylı işlem adımları
  - **Warning**: Uyarılar (duplicate, parse hataları)
  - **Error**: Hatalar ve stack trace'ler

## Hata Yönetimi

- Her dosya bağımsız işlenir, bir hatada diğer dosyalar devam eder
- Hatalar state'te saklanır
- Retry mekanizması ile başarısız işlemler yeniden denenebilir
- Max retry count'a ulaşan dosyalar otomatik atlanır
- Duplicate dosyalar için unique isim üretilir

## Performans

- Batch işleme ile veritabanı performansı optimize edilir
- Configurable batch size (varsayılan: 100)
- Transaction'lar batch'ler halinde commit edilir
- Her batch sonrası ilerleme loglanır

## Güvenlik

- Hassas bilgiler `appsettings.Development.json` içinde tutulmalı (git'e eklenmez)
- Veritabanı bağlantı string'leri environment variable'lar ile de sağlanabilir
- State dosyası backup alınmalı

## Sorun Giderme

### Bağlantı Hataları
- Oracle bağlantı string'ini kontrol edin
- Veritabanı erişim yetkilerinizi kontrol edin
- Firewall ayarlarını kontrol edin

### Dosya Bulunamadı Hataları
- Source path doğru mu kontrol edin
- Dosya yetkileri yeterli mi kontrol edin

### Duplicate Hatalar
- Uygulama otomatik olarak unique isim üretir
- Warning loglarını kontrol edin

## Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'Add some amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın