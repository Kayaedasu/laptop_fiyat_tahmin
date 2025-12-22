@echo off
chcp 65001 >nul
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   SmartShop 6-Layer SOA E-Commerce Platform                  ║
echo ║   Automated Startup Script                                    ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

REM Değişkenler
set PROJECT_ROOT=%~dp0
set MYSQL_CMD=
set NODE_CHECK=node
set PYTHON_CHECK=python

echo [1/7] Checking prerequisites...
echo.

REM MySQL kontrolü - PATH'de ara veya yaygın lokasyonlarda bul
where mysql >nul 2>&1
if %errorlevel% equ 0 (
    set MYSQL_CMD=mysql
) else (
    if exist "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" (
        set "MYSQL_CMD=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
    ) else if exist "C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe" (
        set "MYSQL_CMD=C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe"
    ) else if exist "C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysql.exe" (
        set "MYSQL_CMD=C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysql.exe"
    )
)

if "%MYSQL_CMD%"=="" (
    echo ❌ MySQL is not installed or could not be found
    echo    Please install MySQL 8.0+ or add to PATH
    pause
    exit /b 1
) else (
    echo ✅ MySQL is installed: %MYSQL_CMD%
)

REM Node.js kontrolü
%NODE_CHECK% --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Node.js is not installed or not in PATH
    echo    Please install Node.js 18+ and add to PATH
    pause
    exit /b 1
) else (
    echo ✅ Node.js is installed
)

REM Python kontrolü
%PYTHON_CHECK% --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Python is not installed or not in PATH
    echo    Please install Python 3.9+ and add to PATH
    pause
    exit /b 1
) else (
    echo ✅ Python is installed
)

REM .NET kontrolü
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET SDK is not installed or not in PATH
    echo    Please install .NET 9.0 SDK
    pause
    exit /b 1
) else (
    echo ✅ .NET SDK is installed
)

echo.
echo [2/7] Starting MySQL Database...
echo      Port: 3306
echo.

REM MySQL zaten çalışıyor olabilir, kontrol et
net start | findstr /i "mysql" >nul
if errorlevel 1 (
    echo Starting MySQL service...
    net start MySQL80 >nul 2>&1
    if errorlevel 1 (
        echo ⚠️  Could not start MySQL service automatically
        echo    Please start MySQL manually
    ) else (
        echo ✅ MySQL started
    )
) else (
    echo ✅ MySQL is already running
)

timeout /t 2 /nobreak >nul

echo.
echo [3/7] Starting Microservices (Node.js)...
echo.

REM UserService (gRPC) - Port 50051
echo Starting UserService (gRPC) on port 50051...
start "UserService-gRPC" cmd /k "cd /d %PROJECT_ROOT%3-Service-Layer\UserService && npm install && node server.js"
timeout /t 3 /nobreak >nul

REM ProductService (REST) - Port 3001
echo Starting ProductService (REST) on port 3001...
start "ProductService-REST" cmd /k "cd /d %PROJECT_ROOT%3-Service-Layer\ProductService && npm install && node server.js"
timeout /t 3 /nobreak >nul

REM OrderService (SOAP) - Port 3002
echo Starting OrderService (SOAP) on port 3002...
start "OrderService-SOAP" cmd /k "cd /d %PROJECT_ROOT%3-Service-Layer\OrderService && npm install && node server.js"
timeout /t 3 /nobreak >nul

echo ✅ All microservices starting...

echo.
echo [4/7] Starting ML Service (Python/Flask)...
echo      Port: 5000
echo.

REM ML Service - Port 5000
echo Installing Python dependencies...
start "ML-Service-Python" cmd /k "cd /d %PROJECT_ROOT%ML-Service && pip install -r requirements.txt && python app.py"
timeout /t 5 /nobreak >nul

echo ✅ ML Service starting...

echo.
echo [5/7] Building Integration Layer...
echo.
cd /d "%PROJECT_ROOT%4-Integration-Layer\SmartShop.Integration"
dotnet build --nologo -v q
if errorlevel 1 (
    echo ❌ Integration Layer build failed
    pause
    exit /b 1
)
echo ✅ Integration Layer built successfully

echo.
echo [6/7] Building and Starting ASP.NET MVC Application...
echo      Port: 5000 (or dynamic)
echo.

cd /d "%PROJECT_ROOT%1-Presentation-Layer\SmartShop.Web"
echo Building ASP.NET application...
dotnet build --nologo -v q
if errorlevel 1 (
    echo ❌ ASP.NET application build failed
    pause
    exit /b 1
)

echo Starting ASP.NET application...
start "SmartShop-Web-ASP.NET" cmd /k "dotnet run --no-build"

echo ✅ ASP.NET MVC application starting...

echo.
echo [7/7] All services started!
echo.
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║   SmartShop Platform Started Successfully!                    ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.
echo 📊 Service Status:
echo    ├─ MySQL Database         → Port 3306  ✅
echo    ├─ UserService (gRPC)     → Port 50051 ✅
echo    ├─ ProductService (REST)  → Port 3001  ✅
echo    ├─ OrderService (SOAP)    → Port 3002  ✅
echo    ├─ ML Service (Python)    → Port 5000  ✅
echo    └─ ASP.NET MVC Web App    → Port 5000+ ✅
echo.
echo 🌐 Access Points:
echo    → Web Application: Check the ASP.NET window for URL
echo    → Usually: http://localhost:5000 or https://localhost:5001
echo.
echo 📝 Notes:
echo    - Each service runs in a separate terminal window
echo    - Close individual windows to stop services
echo    - Check each window for service status and logs
echo.
echo Press any key to open the web browser...
pause >nul

REM Web tarayıcısını aç (ASP.NET başlaması için bekle)
timeout /t 5 /nobreak >nul
start http://localhost:5000

echo.
echo ✅ Browser opened. Check the ASP.NET MVC window for the actual URL.
echo.
echo Press any key to exit this window (services will continue running)...
pause >nul
