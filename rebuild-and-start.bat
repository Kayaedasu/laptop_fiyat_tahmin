@echo off
echo ============================================
echo SmartShop - Quick Rebuild and Start
echo ============================================
echo.

cd /d "%~dp0"

echo [1/4] Stopping existing services...
call quick-stop.bat

echo.
echo [2/4] Cleaning build...
cd 1-Presentation-Layer\SmartShop.Web
dotnet clean
echo.

echo [3/4] Building application...
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Build failed!
    pause
    exit /b 1
)
echo.

echo [4/4] Starting services...
cd ..\..
call quick-start.bat

echo.
echo ============================================
echo ✅ Rebuild and start complete!
echo.
echo Check console output for logs
echo Navigate to: http://localhost:5000/Products
echo ============================================
pause
