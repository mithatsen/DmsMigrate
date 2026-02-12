# Geliştirme Kılavuzu

## Geliştirme Ortamı Kurulumu

### Gereksinimler
- .NET 9 SDK
- Visual Studio 2024 veya VS Code
- Oracle Database (12c veya üzeri)
- Git

### Başlangıç
1. Repository'yi fork edin
2. Local'e clone edin:
   ```bash
   git clone https://github.com/your-username/DmsMigrate.git
   cd DmsMigrate
   ```

3. Paketleri yükleyin:
   ```bash
   dotnet restore
   ```

4. Veritabanını kurun:
   - `Database/CreateSchema.sql` scriptini Oracle'da çalıştırın
   - Bağlantı bilgilerini `appsettings.Development.json` içinde ayarlayın

5. Test için örnek dosyalar oluşturun:
   ```bash
   mkdir TestFiles
   # Örnek dosyalar ekleyin
   ```

## Kod Standartları

### Naming Conventions
- Class'lar: PascalCase (`FileService`, `MigrationService`)
- Interface'ler: IPascalCase (`IFileService`)
- Methods: PascalCase (`GetFileMetadataAsync`)
- Private fields: _camelCase (`_logger`, `_context`)
- Local variables: camelCase

### Async/Await
- Async metodlar `Async` suffix'i ile bitmeli
- Her zaman `ConfigureAwait(false)` kullanın (library kodunda)
- Exception handling async metodlarda yapılmalı

### Logging
- Structured logging kullanın
- Log level'ları doğru seçin:
  - Debug: Geliştirme detayları
  - Information: Normal akış
  - Warning: Beklenmeyen ama recoverable durumlar
  - Error: Hatalar ve exception'lar

### Dependency Injection
- Constructor injection tercih edin
- Interface'ler üzerinden bağımlılıkları enjekte edin
- Service lifetime'ları doğru seçin (Singleton, Scoped, Transient)

## Yeni Template Service Ekleme

1. `ITemplateService` interface'ini implement edin:
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
        // Parse filename and add indexes
        metadata.Indexes["CustomKey"] = "CustomValue";
    }
}
```

2. `Program.cs` içinde service'i register edin:
```csharp
services.AddScoped<ITemplateService, CustomTemplateService>();
services.AddScoped<ITemplateService, DefaultTemplateService>(); // En sona ekleyin
```

## Test Etme

### Manuel Test
```bash
# Uygulamayı çalıştır
dotnet run

# Seçenekleri test et:
# 1 - Sıfırdan başlat
# 2 - Hatalıları tekrar çalıştır
# 3 - Kaldığı yerden devam et
```

### Build ve Restore
```bash
# Restore
dotnet restore

# Build
dotnet build

# Release build
dotnet build -c Release
```

### Debugging
Visual Studio veya VS Code'da F5 ile debug modunda çalıştırın.

Breakpoint'ler önerilen yerler:
- `MigrationService.ProcessBatchAsync`: Her dosya işleme
- Template service'lerin `EnrichMetadata`: Metadata parse
- `FileService.CopyFileToTargetAsync`: Dosya kopyalama

## Veritabanı Migration'ları

Entity Framework Core migration'ları kullanarak şema değişikliklerini yönetin:

```bash
# Yeni migration oluştur
dotnet ef migrations add MigrationName

# Migration'ı uygula
dotnet ef database update

# Migration'ı geri al
dotnet ef database update PreviousMigrationName
```

## Hata Ayıklama İpuçları

### Bağlantı Problemleri
1. `appsettings.json` dosyasında connection string'i kontrol edin
2. Oracle sunucusuna erişim olduğunu doğrulayın
3. Kullanıcı yetkilerini kontrol edin

### State Problemleri
`migration-state.json` dosyasını silin ve yeniden başlatın:
```bash
rm migration-state.json
dotnet run
# 1. Sıfırdan başlat seçeneğini seçin
```

### Log İnceleme
```bash
# Son log dosyasını görüntüle
cat Logs/migration-$(date +%Y%m%d).txt

# Real-time log takibi
tail -f Logs/migration-$(date +%Y%m%d).txt
```

## Pull Request Süreci

1. Feature branch oluşturun:
   ```bash
   git checkout -b feature/my-new-feature
   ```

2. Değişikliklerinizi commit edin:
   ```bash
   git add .
   git commit -m "feat: Add new feature description"
   ```

3. Push edin:
   ```bash
   git push origin feature/my-new-feature
   ```

4. GitHub'da Pull Request açın

### Commit Message Formatı
```
type: subject

body (optional)

footer (optional)
```

Types:
- `feat`: Yeni özellik
- `fix`: Bug fix
- `docs`: Dokümantasyon
- `style`: Code style (formatting)
- `refactor`: Refactoring
- `test`: Test ekleme/düzeltme
- `chore`: Build, dependencies

## Kod İnceleme Checklist

- [ ] Kod standartlarına uygun mu?
- [ ] Yeterli hata yönetimi var mı?
- [ ] Logging uygun mu?
- [ ] Performance etkileri değerlendirildi mi?
- [ ] Breaking change var mı?
- [ ] README güncel mi?

## Performans İyileştirmeleri

### Batch Size Optimizasyonu
Batch size'ı test ederek optimal değeri bulun:
```json
{
  "MigrationSettings": {
    "BatchSize": 100  // Test: 50, 100, 200, 500
  }
}
```

### Database Connection Pooling
Oracle connection pooling ayarlarını optimize edin:
```
Min Pool Size=10;Max Pool Size=100;Connection Lifetime=300;
```

## Güvenlik Notları

- Hassas bilgileri git'e commit etmeyin
- `appsettings.Development.json` `.gitignore` içinde
- Connection string'leri environment variable'lar ile de sağlanabilir
- SQL Injection'a karşı EF Core parametreli sorgular kullanın

## Destek

Sorularınız için:
- Issue açın
- Mevcut dokümantasyonu kontrol edin
- README.md dosyasına bakın

## Lisans

Bu proje MIT lisansı altındadır. Detaylar için LICENSE dosyasına bakın.
