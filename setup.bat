@echo off
echo ================================================
echo DMS Migration Uygulamasi - Hizli Baslangic
echo ================================================
echo.

REM Gerekli klasorleri olustur
if not exist "Logs" mkdir Logs
if not exist "Database" mkdir Database
echo [OK] Klasorler kontrol edildi.

REM NuGet paketlerini yükle
echo.
echo NuGet paketleri yukleniyor...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] NuGet paketleri yuklenemedi!
    pause
    exit /b 1
)
echo [OK] NuGet paketleri yuklendi.

REM Uygulamayi build et
echo.
echo Uygulama build ediliyor...
dotnet build --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] Build basarisiz!
    pause
    exit /b 1
)
echo [OK] Build basarili.

REM appsettings kontrolu
echo.
if not exist "appsettings.json" (
    echo [UYARI] appsettings.json bulunamadi!
    echo Lutfen appsettings.json dosyasini olusturun ve yapilandirin.
    pause
    exit /b 1
)
echo [OK] appsettings.json bulundu.

echo.
echo ================================================
echo Hazirlik tamamlandi!
echo ================================================
echo.
echo Uygulamayi calistirmak icin:
echo   dotnet run
echo.
echo Veya dogrudan:
echo   start.bat
echo.
pause
