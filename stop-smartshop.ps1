# SmartShop 6-Layer SOA E-Commerce Platform
# Stop All Services Script (PowerShell)

$ErrorActionPreference = "SilentlyContinue"

Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Red
Write-Host "║   SmartShop - Stop All Services                               ║" -ForegroundColor Red
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Red
Write-Host ""

Write-Host "Stopping SmartShop services..." -ForegroundColor Yellow
Write-Host ""

# Stop Node.js processes (Microservices)
Write-Host "[1/4] Stopping Node.js Microservices..." -ForegroundColor Cyan

$nodeProcesses = Get-Process | Where-Object {$_.ProcessName -eq "node" -or $_.MainWindowTitle -like "*UserService*" -or $_.MainWindowTitle -like "*ProductService*" -or $_.MainWindowTitle -like "*OrderService*"}
if ($nodeProcesses) {
    $nodeProcesses | ForEach-Object {
        Write-Host "   Stopping: $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Gray
        Stop-Process -Id $_.Id -Force
    }
    Write-Host "✅ Node.js services stopped" -ForegroundColor Green
} else {
    Write-Host "ℹ️  No Node.js services found" -ForegroundColor Gray
}

# Stop Python processes (ML Service)
Write-Host ""
Write-Host "[2/4] Stopping Python ML Service..." -ForegroundColor Cyan

$pythonProcesses = Get-Process | Where-Object {$_.ProcessName -eq "python" -or $_.ProcessName -eq "pythonw" -or $_.MainWindowTitle -like "*ML-Service*"}
if ($pythonProcesses) {
    $pythonProcesses | ForEach-Object {
        Write-Host "   Stopping: $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Gray
        Stop-Process -Id $_.Id -Force
    }
    Write-Host "✅ Python ML Service stopped" -ForegroundColor Green
} else {
    Write-Host "ℹ️  No Python ML Service found" -ForegroundColor Gray
}

# Stop .NET processes (ASP.NET MVC)
Write-Host ""
Write-Host "[3/4] Stopping ASP.NET MVC Application..." -ForegroundColor Cyan

$dotnetProcesses = Get-Process | Where-Object {
    ($_.ProcessName -eq "dotnet" -and $_.MainWindowTitle -like "*SmartShop*") -or
    $_.MainWindowTitle -like "*SmartShop-Web*"
}
if ($dotnetProcesses) {
    $dotnetProcesses | ForEach-Object {
        Write-Host "   Stopping: $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Gray
        Stop-Process -Id $_.Id -Force
    }
    Write-Host "✅ ASP.NET MVC application stopped" -ForegroundColor Green
} else {
    Write-Host "ℹ️  No ASP.NET application found" -ForegroundColor Gray
}

# Clean up ports (optional - kills processes using specific ports)
Write-Host ""
Write-Host "[4/4] Cleaning up service ports..." -ForegroundColor Cyan

$ports = @(50051, 3001, 3002, 5000, 5001)
foreach ($port in $ports) {
    $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($connection) {
        $processId = $connection.OwningProcess
        $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
        if ($process) {
            Write-Host "   Stopping process on port ${port}: $($process.ProcessName) (PID: $processId)" -ForegroundColor Gray
            Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "✅ Port cleanup completed" -ForegroundColor Green

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   All SmartShop services stopped successfully!                ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Stopped Services:" -ForegroundColor Yellow
Write-Host "   ✅ UserService (gRPC) - Port 50051" -ForegroundColor White
Write-Host "   ✅ ProductService (REST) - Port 3001" -ForegroundColor White
Write-Host "   ✅ OrderService (SOAP) - Port 3002" -ForegroundColor White
Write-Host "   ✅ ML Service (Python) - Port 5000" -ForegroundColor White
Write-Host "   ✅ ASP.NET MVC Web App - Port 5000+" -ForegroundColor White
Write-Host ""
Write-Host "📝 Note: MySQL database service was not stopped (manual control recommended)" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
