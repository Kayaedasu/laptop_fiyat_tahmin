# SmartShop 6-Layer SOA E-Commerce Platform
# Automated Startup Script (PowerShell)
# UTF-8 encoding for Turkish characters

$ErrorActionPreference = "Continue"

Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   SmartShop 6-Layer SOA E-Commerce Platform                  ║" -ForegroundColor Cyan
Write-Host "║   Automated Startup Script (PowerShell)                       ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$ProjectRoot = $PSScriptRoot

Write-Host "[1/7] Checking prerequisites..." -ForegroundColor Yellow
Write-Host ""

Write-Host "[1/7] Checking prerequisites..." -ForegroundColor Yellow
Write-Host ""

# MySQL Check - Search in PATH or common installation directories
$mysqlCmd = $null
$mysqlPaths = @(
    "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe",
    "C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe",
    "C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysql.exe",
    "C:\Program Files (x86)\MySQL\MySQL Server 8.4\bin\mysql.exe"
)

# Check if mysql is in PATH
try {
    $mysqlCmd = (Get-Command mysql -ErrorAction SilentlyContinue).Source
    if ($mysqlCmd) {
        $mysqlVersion = & $mysqlCmd --version 2>$null
        Write-Host "✅ MySQL is installed (in PATH): $($mysqlVersion)" -ForegroundColor Green
    }
} catch {
    # Not in PATH
}

# If not in PATH, check common installation directories
if (-not $mysqlCmd) {
    foreach ($path in $mysqlPaths) {
        if (Test-Path $path) {
            $mysqlCmd = $path
            $mysqlVersion = & $mysqlCmd --version 2>$null
            Write-Host "✅ MySQL is installed (found at $path)" -ForegroundColor Green
            Write-Host "   Version: $($mysqlVersion)" -ForegroundColor Gray
            break
        }
    }
}

if (-not $mysqlCmd) {
    Write-Host "❌ MySQL is not installed or could not be found" -ForegroundColor Red
    Write-Host "   Please install MySQL 8.0+ or add to PATH" -ForegroundColor Yellow
    Write-Host "   Checked locations:" -ForegroundColor Gray
    foreach ($path in $mysqlPaths) {
        Write-Host "     - $path" -ForegroundColor Gray
    }
    Read-Host "Press Enter to exit"
    exit 1
}

# Node.js Check
try {
    $nodeVersion = node --version 2>$null
    Write-Host "✅ Node.js is installed: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js is not installed or not in PATH" -ForegroundColor Red
    Write-Host "   Please install Node.js 18+ and add to PATH" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Python Check
try {
    $pythonVersion = python --version 2>$null
    Write-Host "✅ Python is installed: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "   Please install Python 3.9+ and add to PATH" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# .NET Check
try {
    $dotnetVersion = dotnet --version 2>$null
    Write-Host "✅ .NET SDK is installed: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK is not installed or not in PATH" -ForegroundColor Red
    Write-Host "   Please install .NET 9.0 SDK" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "[2/7] Checking MySQL Database..." -ForegroundColor Yellow
Write-Host "      Port: 3306" -ForegroundColor Gray
Write-Host ""

# Check if MySQL is running
$mysqlRunning = Get-Service -Name "MySQL*" -ErrorAction SilentlyContinue | Where-Object {$_.Status -eq 'Running'}
if ($mysqlRunning) {
    Write-Host "✅ MySQL is already running" -ForegroundColor Green
} else {
    Write-Host "⚠️  MySQL service not detected as running" -ForegroundColor Yellow
    Write-Host "   Please ensure MySQL is running on port 3306" -ForegroundColor Yellow
}

Start-Sleep -Seconds 2

Write-Host ""
Write-Host "[3/7] Starting Microservices (Node.js)..." -ForegroundColor Yellow
Write-Host ""

# UserService (gRPC) - Port 50051
Write-Host "Starting UserService (gRPC) on port 50051..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$ProjectRoot\3-Service-Layer\UserService'; Write-Host '🔹 UserService (gRPC) - Port 50051' -ForegroundColor Cyan; npm install; node server.js"
Start-Sleep -Seconds 3

# ProductService (REST) - Port 3001
Write-Host "Starting ProductService (REST) on port 3001..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$ProjectRoot\3-Service-Layer\ProductService'; Write-Host '🔹 ProductService (REST) - Port 3001' -ForegroundColor Cyan; npm install; node server.js"
Start-Sleep -Seconds 3

# OrderService (SOAP) - Port 3002
Write-Host "Starting OrderService (SOAP) on port 3002..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$ProjectRoot\3-Service-Layer\OrderService'; Write-Host '🔹 OrderService (SOAP) - Port 3002' -ForegroundColor Cyan; npm install; node server.js"
Start-Sleep -Seconds 3

Write-Host "✅ All microservices starting..." -ForegroundColor Green

Write-Host ""
Write-Host "[4/7] Starting ML Service (Python/Flask)..." -ForegroundColor Yellow
Write-Host "      Port: 5000" -ForegroundColor Gray
Write-Host ""

# ML Service - Port 5000
Write-Host "Installing Python dependencies and starting ML Service..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$ProjectRoot\ML-Service'; Write-Host '🤖 ML Service (Python/Flask) - Port 5000' -ForegroundColor Cyan; pip install -r requirements.txt; python app.py"
Start-Sleep -Seconds 5

Write-Host "✅ ML Service starting..." -ForegroundColor Green

Write-Host ""
Write-Host "[5/7] Building Integration Layer..." -ForegroundColor Yellow
Write-Host ""

Set-Location "$ProjectRoot\4-Integration-Layer\SmartShop.Integration"
$buildResult = dotnet build --nologo -v q 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Integration Layer build failed" -ForegroundColor Red
    Write-Host $buildResult -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "✅ Integration Layer built successfully" -ForegroundColor Green

Write-Host ""
Write-Host "[6/7] Building and Starting ASP.NET MVC Application..." -ForegroundColor Yellow
Write-Host "      Port: 5000 (or dynamic)" -ForegroundColor Gray
Write-Host ""

Set-Location "$ProjectRoot\1-Presentation-Layer\SmartShop.Web"
Write-Host "Building ASP.NET application..." -ForegroundColor Cyan
$buildResult = dotnet build --nologo -v q 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ASP.NET application build failed" -ForegroundColor Red
    Write-Host $buildResult -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Starting ASP.NET application..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$ProjectRoot\1-Presentation-Layer\SmartShop.Web'; Write-Host '🌐 SmartShop Web (ASP.NET MVC)' -ForegroundColor Green; dotnet run --no-build"

Write-Host "✅ ASP.NET MVC application starting..." -ForegroundColor Green

Write-Host ""
Write-Host "[7/7] All services started!" -ForegroundColor Green
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   SmartShop Platform Started Successfully!                    ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Service Status:" -ForegroundColor Yellow
Write-Host "   ├─ MySQL Database         → Port 3306  ✅" -ForegroundColor White
Write-Host "   ├─ UserService (gRPC)     → Port 50051 ✅" -ForegroundColor White
Write-Host "   ├─ ProductService (REST)  → Port 3001  ✅" -ForegroundColor White
Write-Host "   ├─ OrderService (SOAP)    → Port 3002  ✅" -ForegroundColor White
Write-Host "   ├─ ML Service (Python)    → Port 5000  ✅" -ForegroundColor White
Write-Host "   └─ ASP.NET MVC Web App    → Port 5000+ ✅" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Access Points:" -ForegroundColor Yellow
Write-Host "   → Web Application: Check the ASP.NET window for URL" -ForegroundColor Cyan
Write-Host "   → Usually: http://localhost:5000 or https://localhost:5001" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 Notes:" -ForegroundColor Yellow
Write-Host "   - Each service runs in a separate PowerShell window" -ForegroundColor Gray
Write-Host "   - Close individual windows to stop services" -ForegroundColor Gray
Write-Host "   - Check each window for service status and logs" -ForegroundColor Gray
Write-Host "   - Use stop-smartshop.ps1 to stop all services at once" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to open the web browser..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Wait for ASP.NET to start
Start-Sleep -Seconds 5

# Open browser
Start-Process "http://localhost:5000"

Write-Host ""
Write-Host "✅ Browser opened. Check the ASP.NET MVC window for the actual URL." -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to exit this window (services will continue running)..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Set-Location $ProjectRoot
