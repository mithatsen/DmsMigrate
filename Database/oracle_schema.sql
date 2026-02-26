-- =============================================
-- DMS Migration - Oracle Database Schema
-- =============================================
-- Bu script DMS dosya migration için gerekli
-- tabloların varlığını kontrol eder ve index'leri ekler.
-- =============================================

-- Tablolar zaten mevcut olduğu için sadece kontrol ve index ekleme yapıyoruz

-- =============================================
-- Tablo Varlık Kontrolü
-- =============================================

SELECT 'DMS_DOCUMENT tablosu: ' || 
       CASE WHEN COUNT(*) > 0 THEN 'MEVCUT' ELSE 'BULUNAMADI!' END as Status
FROM user_tables WHERE table_name = 'DMS_DOCUMENT';

SELECT 'DMS_DOCUMENT_TYPE tablosu: ' || 
       CASE WHEN COUNT(*) > 0 THEN 'MEVCUT' ELSE 'BULUNAMADI!' END as Status
FROM user_tables WHERE table_name = 'DMS_DOCUMENT_TYPE';

SELECT 'DMS_DOCUMENT_INDEX tablosu: ' || 
       CASE WHEN COUNT(*) > 0 THEN 'MEVCUT' ELSE 'BULUNAMADI!' END as Status
FROM user_tables WHERE table_name = 'DMS_DOCUMENT_INDEX';

SELECT 'DMS_INDEX tablosu: ' || 
       CASE WHEN COUNT(*) > 0 THEN 'MEVCUT' ELSE 'BULUNAMADI!' END as Status
FROM user_tables WHERE table_name = 'DMS_INDEX';

-- =============================================
-- Performance İndexleri (Yoksa Ekle)
-- =============================================

-- DMS_DOCUMENT için indexler
BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_DOCUMENT_FILENAME ON DMS_DOCUMENT(FILE_NAME)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL; -- Index already exists
        ELSE RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_DOCUMENT_EXTENSION ON DMS_DOCUMENT(EXTENSION)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_DOCUMENT_TENANT ON DMS_DOCUMENT(TENANT_ID)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_DOCUMENT_ISDELETED ON DMS_DOCUMENT(IS_DELETED)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_DOCUMENT_CREATETIME ON DMS_DOCUMENT(CREATION_TIME)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

-- DMS_DOCUMENT_INDEX için indexler
BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_DOCINDEX_DOCID ON DMS_DOCUMENT_INDEX(DOCUMENT_ID)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_DOCINDEX_INDEXID ON DMS_DOCUMENT_INDEX(INDEX_ID)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_DOCINDEX_VALUE ON DMS_DOCUMENT_INDEX(VALUE)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

-- Composite index (sık kullanılan sorgular için)
BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_DOCINDEX_COMPOSITE ON DMS_DOCUMENT_INDEX(DOCUMENT_ID, INDEX_ID, VALUE)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

-- DMS_INDEX için indexler
BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_DMS_INDEX_KEY ON DMS_INDEX(KEY)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX UQ_DMS_INDEX_KEY_TENANT ON DMS_INDEX(KEY, TENANT_ID) WHERE IS_DELETED = 0';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN NULL;
        ELSE RAISE;
        END IF;
END;
/

-- =============================================
-- İstatistikler ve Optimizasyon
-- =============================================

BEGIN
    DBMS_STATS.GATHER_TABLE_STATS(
        ownname => USER,
        tabname => 'DMS_DOCUMENT',
        estimate_percent => DBMS_STATS.AUTO_SAMPLE_SIZE,
        method_opt => 'FOR ALL COLUMNS SIZE AUTO'
    );

    DBMS_STATS.GATHER_TABLE_STATS(
        ownname => USER,
        tabname => 'DMS_DOCUMENT_INDEX',
        estimate_percent => DBMS_STATS.AUTO_SAMPLE_SIZE,
        method_opt => 'FOR ALL COLUMNS SIZE AUTO'
    );

    DBMS_STATS.GATHER_TABLE_STATS(
        ownname => USER,
        tabname => 'DMS_INDEX',
        estimate_percent => DBMS_STATS.AUTO_SAMPLE_SIZE,
        method_opt => 'FOR ALL COLUMNS SIZE AUTO'
    );

    DBMS_STATS.GATHER_TABLE_STATS(
        ownname => USER,
        tabname => 'DMS_DOCUMENT_TYPE',
        estimate_percent => DBMS_STATS.AUTO_SAMPLE_SIZE,
        method_opt => 'FOR ALL COLUMNS SIZE AUTO'
    );
END;
/

-- =============================================
-- Yardımcı Stored Procedure'ler
-- =============================================

-- Doküman sayısını getir
CREATE OR REPLACE FUNCTION GetDocumentCount
RETURN NUMBER
IS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM DMS_DOCUMENT WHERE IS_DELETED = 0;
    RETURN v_count;
END;
/

-- Toplam dosya boyutunu getir
CREATE OR REPLACE FUNCTION GetTotalFileSize
RETURN NUMBER
IS
    v_total NUMBER;
BEGIN
    SELECT NVL(SUM(SIZE), 0) INTO v_total FROM DMS_DOCUMENT WHERE IS_DELETED = 0;
    RETURN v_total;
END;
/

-- =============================================
-- Test Sorguları
-- =============================================

-- Tablo ve index kontrolü
SELECT 
    table_name, 
    num_rows, 
    blocks, 
    avg_row_len 
FROM user_tables 
WHERE table_name LIKE 'DMS%'
ORDER BY table_name;

-- Index durumu
SELECT 
    index_name, 
    table_name, 
    uniqueness, 
    status 
FROM user_indexes 
WHERE table_name LIKE 'DMS%'
ORDER BY table_name, index_name;

-- Toplam istatistikler
SELECT 
    (SELECT COUNT(*) FROM DMS_DOCUMENT WHERE IS_DELETED = 0) as TotalDocuments,
    (SELECT COUNT(*) FROM DMS_DOCUMENT_INDEX WHERE IS_DELETED = 0) as TotalIndexes,
    (SELECT COUNT(*) FROM DMS_INDEX WHERE IS_DELETED = 0) as TotalIndexDefinitions,
    (SELECT ROUND(SUM(SIZE) / 1024 / 1024 / 1024, 2) FROM DMS_DOCUMENT WHERE IS_DELETED = 0) as TotalSizeGB
FROM DUAL;

-- =============================================
-- Temizlik ve Bakım Sorguları
-- =============================================

-- Silinen dokümanları tamamen kaldır (30 günden eski)
-- DELETE FROM DMS_DOCUMENT WHERE IS_DELETED = 1 AND DELETION_TIME < SYSDATE - 30;

-- Orphan kayıtları temizle
-- DELETE FROM DMS_DOCUMENT_INDEX WHERE DOCUMENT_ID NOT IN (SELECT ID FROM DMS_DOCUMENT);

-- =============================================
-- Script Tamamlandı
-- =============================================

PROMPT '';
PROMPT '=============================================';
PROMPT 'DMS Migration database check tamamlandı!';
PROMPT '=============================================';
PROMPT '';
PROMPT 'Tablo sayısı:';
SELECT COUNT(*) as TableCount FROM user_tables WHERE table_name LIKE 'DMS%';
PROMPT '';
PROMPT 'Index sayısı:';
SELECT COUNT(*) as IndexCount FROM user_indexes WHERE table_name LIKE 'DMS%';

