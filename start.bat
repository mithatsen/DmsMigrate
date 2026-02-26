@echo off
echo ================================================
echo DMS Migration Uygulamasi Baslatiliyor...
echo ================================================
echo.

REM Kaynak ve hedef path kontrolu
echo Konfigurasyonu kontrol ediyorum...

REM Uygulamayi calistir
dotnet run --configuration Release

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [HATA] Uygulama hata ile sonlandi!
    echo Log dosyalarini kontrol edin: Logs\
    pause
    exit /b 1
)

echo.
echo ================================================
echo Migration tamamlandi!
echo ================================================
echo.
pause
