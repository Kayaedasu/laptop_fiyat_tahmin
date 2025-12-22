@echo off
chcp 65001 >nul
title SmartShop - Master Control Panel
color 0A

:MENU
cls
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   SmartShop 6-Layer SOA E-Commerce Platform                  ║
echo ║   Master Control Panel                                        ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
echo Please select an option:
echo.
echo   [1] Start All Services
echo   [2] Stop All Services
echo   [3] Restart All Services
echo   [4] Check Service Status
echo   [5] Open Web Application
echo   [0] Exit
echo.
echo ───────────────────────────────────────────────────────────────
set /p choice="Enter your choice (0-5): "

if "%choice%"=="1" goto START_SERVICES
if "%choice%"=="2" goto STOP_SERVICES
if "%choice%"=="3" goto RESTART_SERVICES
if "%choice%"=="4" goto CHECK_STATUS
if "%choice%"=="5" goto OPEN_WEB
if "%choice%"=="0" goto EXIT
goto MENU

:START_SERVICES
cls
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   Starting All Services...                                    ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
call start-smartshop.bat
echo.
echo Press any key to return to menu...
pause >nul
goto MENU

:STOP_SERVICES
cls
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   Stopping All Services...                                    ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
call stop-smartshop.bat
echo.
echo Press any key to return to menu...
pause >nul
goto MENU

:RESTART_SERVICES
cls
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   Restarting All Services...                                  ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
echo [1/2] Stopping services...
call stop-smartshop.bat
timeout /t 3 /nobreak >nul
echo.
echo [2/2] Starting services...
call start-smartshop.bat
echo.
echo Press any key to return to menu...
pause >nul
goto MENU

:CHECK_STATUS
cls
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   Service Status Check                                        ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

echo Checking MySQL (Port 3306)...
netstat -ano | findstr ":3306" | findstr "LISTENING" >nul
if errorlevel 1 (
    echo    ❌ MySQL - NOT RUNNING
) else (
    echo    ✅ MySQL - RUNNING
)

echo Checking UserService (Port 50051)...
netstat -ano | findstr ":50051" | findstr "LISTENING" >nul
if errorlevel 1 (
    echo    ❌ UserService - NOT RUNNING
) else (
    echo    ✅ UserService - RUNNING
)

echo Checking ProductService (Port 3001)...
netstat -ano | findstr ":3001" | findstr "LISTENING" >nul
if errorlevel 1 (
    echo    ❌ ProductService - NOT RUNNING
) else (
    echo    ✅ ProductService - RUNNING
)

echo Checking OrderService (Port 3002)...
netstat -ano | findstr ":3002" | findstr "LISTENING" >nul
if errorlevel 1 (
    echo    ❌ OrderService - NOT RUNNING
) else (
    echo    ✅ OrderService - RUNNING
)

echo Checking ML Service (Port 5000)...
netstat -ano | findstr ":5000" | findstr "LISTENING" >nul
if errorlevel 1 (
    echo    ❌ ML Service - NOT RUNNING
) else (
    echo    ✅ ML Service - RUNNING
)

echo Checking ASP.NET Web (Port 5000+)...
tasklist /FI "IMAGENAME eq dotnet.exe" | findstr "dotnet.exe" >nul
if errorlevel 1 (
    echo    ❌ ASP.NET Web - NOT RUNNING
) else (
    echo    ✅ ASP.NET Web - RUNNING
)

echo.
echo Press any key to return to menu...
pause >nul
goto MENU

:OPEN_WEB
cls
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   Opening Web Application...                                  ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
echo Trying to open http://localhost:5000...
start http://localhost:5000
timeout /t 2 /nobreak >nul
echo.
echo If the page doesn't load, try:
echo    → http://localhost:5001 (HTTPS)
echo    → Check the ASP.NET window for the actual URL
echo.
echo Press any key to return to menu...
pause >nul
goto MENU

:EXIT
cls
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   Exiting SmartShop Control Panel                             ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
echo ⚠️  Note: Services are still running!
echo.
set /p stop_services="Do you want to stop all services before exit? (Y/N): "
if /i "%stop_services%"=="Y" (
    call stop-smartshop.bat
)
echo.
echo Goodbye! 👋
timeout /t 2 /nobreak >nul
exit
