@echo off
REM Quick Start - Start all services in background and return
echo Starting SmartShop Platform...
start /min cmd /c "start-smartshop.bat"
echo.
echo ✅ Services are starting in background...
echo    Use stop-smartshop.bat to stop all services
echo    Use smartshop-control.bat for full control
echo.
timeout /t 3 /nobreak >nul
