@echo off
chcp 65001 >nul
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   SmartShop 6-Layer SOA E-Commerce Platform                  ║
echo ║   Automated Shutdown Script                                   ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

echo [1/6] Stopping ASP.NET MVC Application...
echo.

REM ASP.NET process'lerini bul ve durdur
for /f "tokens=2" %%a in ('tasklist /FI "WINDOWTITLE eq SmartShop-Web-ASP.NET*" /FO LIST ^| findstr /i "PID"') do (
    echo Stopping ASP.NET process (PID: %%a)...
    taskkill /PID %%a /F >nul 2>&1
)

REM Alternatif: dotnet run process'lerini bul
tasklist /FI "IMAGENAME eq dotnet.exe" | findstr "dotnet.exe" >nul
if not errorlevel 1 (
    echo Stopping dotnet processes...
    taskkill /IM dotnet.exe /F >nul 2>&1
)

echo ✅ ASP.NET stopped

echo.
echo [2/6] Stopping ML Service (Python/Flask)...
echo.

REM Python Flask process'lerini durdur
for /f "tokens=2" %%a in ('tasklist /FI "WINDOWTITLE eq ML-Service-Python*" /FO LIST ^| findstr /i "PID"') do (
    echo Stopping ML Service process (PID: %%a)...
    taskkill /PID %%a /F >nul 2>&1
)

REM Port 5000'de çalışan Python process'lerini bul
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5000" ^| findstr "LISTENING"') do (
    echo Stopping process on port 5000 (PID: %%a)...
    taskkill /PID %%a /F >nul 2>&1
)

echo ✅ ML Service stopped

echo.
echo [3/6] Stopping OrderService (SOAP)...
echo.

REM OrderService process'ini durdur
for /f "tokens=2" %%a in ('tasklist /FI "WINDOWTITLE eq OrderService-SOAP*" /FO LIST ^| findstr /i "PID"') do (
    echo Stopping OrderService process (PID: %%a)...
    taskkill /PID %%a /F >nul 2>&1
)

REM Port 3002'de çalışan Node process'lerini bul
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":3002" ^| findstr "LISTENING"') do (
    echo Stopping process on port 3002 (PID: %%a)...
    taskkill /PID %%a /F >nul 2>&1
)

echo ✅ OrderService stopped

echo.
echo [4/6] Stopping ProductService (REST)...
echo.

REM ProductService process'ini durdur
for /f "tokens=2" %%a in ('tasklist /FI "WINDOWTITLE eq ProductService-REST*" /FO LIST ^| findstr /i "PID"') do (
    echo Stopping ProductService process (PID: %%a)...
    taskkill /PID %%a /F >nul 2>&1
)

REM Port 3001'de çalışan Node process'lerini bul
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":3001" ^| findstr "LISTENING"') do (
    echo Stopping process on port 3001 (PID: %%a)...
    taskkill /PID %%a /F >nul 2>&1
)

echo ✅ ProductService stopped

echo.
echo [5/6] Stopping UserService (gRPC)...
echo.

REM UserService process'ini durdur
for /f "tokens=2" %%a in ('tasklist /FI "WINDOWTITLE eq UserService-gRPC*" /FO LIST ^| findstr /i "PID"') do (
    echo Stopping UserService process (PID: %%a)...
    taskkill /PID %%a /F >nul 2>&1
)

REM Port 50051'de çalışan Node process'lerini bul
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":50051" ^| findstr "LISTENING"') do (
    echo Stopping process on port 50051 (PID: %%a)...
    taskkill /PID %%a /F >nul 2>&1
)

echo ✅ UserService stopped

echo.
echo [6/6] Cleanup remaining processes...
echo.

REM Kalan Node.js process'lerini temizle (dikkatli!)
REM tasklist /FI "IMAGENAME eq node.exe" | findstr "node.exe" >nul
REM if not errorlevel 1 (
REM     echo ⚠️  Cleaning up remaining Node.js processes...
REM     taskkill /IM node.exe /F >nul 2>&1
REM )

REM Kalan Python process'lerini temizle (dikkatli!)
REM tasklist /FI "IMAGENAME eq python.exe" | findstr "python.exe" >nul
REM if not errorlevel 1 (
REM     echo ⚠️  Cleaning up remaining Python processes...
REM     taskkill /IM python.exe /F >nul 2>&1
REM )

echo ✅ Cleanup complete

echo.
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   SmartShop Platform Stopped Successfully!                    ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
echo 📊 All Services Stopped:
echo    ├─ ASP.NET MVC Web App    ✅
echo    ├─ ML Service (Python)    ✅
echo    ├─ OrderService (SOAP)    ✅
echo    ├─ ProductService (REST)  ✅
echo    └─ UserService (gRPC)     ✅
echo.
echo 📝 Note: MySQL Database was not stopped (might be used by other apps)
echo    To stop MySQL manually: net stop MySQL80
echo.
echo Press any key to exit...
pause >nul
