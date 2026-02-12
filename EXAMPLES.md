# Örnek Senaryolar

## Senaryo 1: KOF Dosyalarını Migration

### Hazırlık
1. Test dosyaları oluşturun:
```bash
mkdir -p TestFiles
echo "Test content" > TestFiles/KOF_12345678_Document.pdf
echo "Test content" > TestFiles/KOF_87654321_Report.pdf
echo "Test content" > TestFiles/KOF_11111111_Manual.pdf
```

2. `appsettings.json` dosyasını düzenleyin:
```json
{
  "MigrationSettings": {
    "SourcePath": "TestFiles",
    "TargetPath": "dmsfiles"
  }
}
```

3. Uygulamayı çalıştırın:
```bash
dotnet run
# Seçenek 1: Sıfırdan başlat
```

### Beklenen Sonuç
```
=== DMS Dosya Migration Uygulaması ===
1. Sıfırdan başlat
2. Hatalıları tekrar çalıştır
3. Kaldığı yerden devam et

Seçiminiz: 1

DMS Migration başlatılıyor...
Toplam 3 dosya bulundu, 3 dosya işlenecek
✓ Başarılı: KOF_12345678_Document.pdf (ID: 1)
✓ Başarılı: KOF_87654321_Report.pdf (ID: 2)
✓ Başarılı: KOF_11111111_Manual.pdf (ID: 3)
İlerleme: 3/3 dosya işlendi

=== Migration Sonuçları ===
✓ Başarılı: 3
✗ Hatalı: 0
⏱ Süre: 00:00:05
```

### Veritabanı Kontrolü
```sql
-- Documents
SELECT ID, FILE_NAME, TYPE_ID FROM DMS_DOCUMENT;
-- Beklenen: 3 kayıt, TYPE_ID = 1 (KOF)

-- Indexes
SELECT d.FILE_NAME, di.INDEX_KEY, di.INDEX_VALUE 
FROM DMS_DOCUMENT_INDEX di
JOIN DMS_DOCUMENT d ON di.DOCUMENT_ID = d.ID;
-- Beklenen: 
-- ProjectNo: 12345678, 87654321, 11111111
-- DocumentType: KOF (3 kez)

-- Versions
SELECT d.FILE_NAME, dv.VERSION_NUMBER 
FROM DMS_DOCUMENT_VERSION dv
JOIN DMS_DOCUMENT d ON dv.DOCUMENT_ID = d.ID;
-- Beklenen: 3 kayıt, VERSION_NUMBER = 1
```

## Senaryo 2: Karışık Dosya Tipleri

### Hazırlık
```bash
mkdir -p TestFiles
echo "KOF content" > TestFiles/KOF_99999999_Test.pdf
echo "Regular content" > TestFiles/Invoice_2024.pdf
echo "Image content" > TestFiles/Photo.jpg
echo "Document" > TestFiles/Contract.docx
```

### Çalıştırma
```bash
dotnet run
# Seçenek 1: Sıfırdan başlat
```

### Beklenen Sonuç
- KOF_99999999_Test.pdf: TYPE_ID = 1, Indexes: ProjectNo + DocumentType
- Invoice_2024.pdf: TYPE_ID = 99, Index: FileName
- Photo.jpg: TYPE_ID = 99, Index: FileName
- Contract.docx: TYPE_ID = 99, Index: FileName

## Senaryo 3: Hata Durumu ve Retry

### Hazırlık
1. Dosyalar oluşturun:
```bash
mkdir -p TestFiles
echo "Test" > TestFiles/Valid.pdf
echo "Test" > TestFiles/Test2.pdf
```

2. İlk migration'ı çalıştırın (Seçenek 1)

3. Veritabanı bağlantısını kesin (test için)

4. Yeni dosyalar ekleyin:
```bash
echo "New" > TestFiles/New.pdf
```

5. Migration'ı devam ettirin (Seçenek 3)

### Beklenen Durum
- Valid.pdf ve Test2.pdf: Status = Success (işlenmez)
- New.pdf: Status = Failed (bağlantı hatası)

6. Bağlantıyı düzeltin ve retry çalıştırın (Seçenek 2)

### Beklenen Sonuç
- New.pdf: Status = Success

## Senaryo 4: Duplicate Dosya Yönetimi

### Hazırlık
```bash
mkdir -p TestFiles
mkdir -p dmsfiles

# Hedef dizinde duplicate dosya oluştur
echo "Existing" > dmsfiles/Document.pdf

# Aynı isimli dosyayı source'a ekle
echo "New version" > TestFiles/Document.pdf
```

### Çalıştırma
```bash
dotnet run
# Seçenek 1
```

### Beklenen Sonuç
- Dosya `Document_1.pdf` olarak kopyalanır
- Warning logu: "Duplicate file found. Renamed to: Document_1.pdf"
- Veritabanında FILE_NAME = "Document.pdf" (orijinal isim)
- Fiziksel dosya: dmsfiles/Document_1.pdf

## Senaryo 5: Batch İşleme

### Hazırlık
```bash
# 250 test dosyası oluştur
mkdir -p TestFiles
for i in {1..250}; do
    echo "Content $i" > TestFiles/File_$i.pdf
done
```

### Konfigürasyon
```json
{
  "MigrationSettings": {
    "BatchSize": 100
  }
}
```

### Beklenen Çıktı
```
DMS Migration başlatılıyor...
Toplam 250 dosya bulundu, 250 dosya işlenecek
...
İlerleme: 100/250 dosya işlendi
...
İlerleme: 200/250 dosya işlendi
...
İlerleme: 250/250 dosya işlendi
```

## Senaryo 6: State Management

### Test 1: Kesinti Sonrası Devam
```bash
# 1. Migration başlat
dotnet run  # Seçenek 1

# 2. Ctrl+C ile yarıda kes

# 3. Tekrar başlat
dotnet run  # Seçenek 3 (Kaldığı yerden devam)
```

### Test 2: State Reset
```bash
# State dosyasını kontrol et
cat migration-state.json

# Reset ile temizle
dotnet run  # Seçenek 1 (Sıfırdan başlat)

# State dosyası yeniden oluşturuldu
cat migration-state.json
```

## Log İnceleme

### Console Log
Real-time olarak ekranda görüntülenir:
- Başarılı işlemler: ✓
- Hatalar: ✗
- İlerleme raporları
- Özet istatistikler

### File Log
```bash
# Bugünün log dosyası
cat Logs/migration-20240115.txt

# Hata logları filtrele
grep ERROR Logs/migration-20240115.txt

# Warning'leri filtrele
grep WARN Logs/migration-20240115.txt
```

## Performance Test

### Küçük Dosyalar (1-10 KB)
```bash
# 1000 küçük dosya
for i in {1..1000}; do
    echo "Small content" > TestFiles/Small_$i.pdf
done

# BatchSize = 100
# Beklenen süre: < 30 saniye
```

### Büyük Dosyalar (1-10 MB)
```bash
# 100 büyük dosya
for i in {1..100}; do
    dd if=/dev/urandom of=TestFiles/Large_$i.pdf bs=1M count=5
done

# BatchSize = 50
# Beklenen süre: < 2 dakika
```

## Troubleshooting

### Problem: "Source directory does not exist"
**Çözüm**: appsettings.json'da SourcePath'i kontrol edin

### Problem: Oracle bağlantı hatası
**Çözüm**: 
1. Connection string'i kontrol edin
2. Oracle sunucusunun çalıştığını doğrulayın
3. Kullanıcı yetkilerini kontrol edin

### Problem: "Table or view does not exist"
**Çözüm**: Database/CreateSchema.sql scriptini çalıştırın

### Problem: "Access denied" dosya kopyalama hatası
**Çözüm**: 
1. Source ve target dizinlere yazma yetkisi olduğunu kontrol edin
2. Dosyaların başka bir uygulama tarafından kullanılmadığını kontrol edin
