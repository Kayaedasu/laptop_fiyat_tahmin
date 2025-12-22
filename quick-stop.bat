@echo off
REM Quick Stop - Stop all services immediately
echo Stopping SmartShop Platform...
call stop-smartshop.bat
echo.
echo ✅ All services stopped!
timeout /t 2 /nobreak >nul
