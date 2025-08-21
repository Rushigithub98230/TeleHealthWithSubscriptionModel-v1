# Quick Test Runner for SmartTelehealth Tests
# Run this script to quickly test the newly created test files

Write-Host "🧪 Quick Test Runner for SmartTelehealth" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan

# Change to test directory
Set-Location $PSScriptRoot

# Check if dotnet is available
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host "📦 Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Build the solution
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
dotnet build --no-restore

# Run the newly created tests
Write-Host "🧪 Running newly created tests..." -ForegroundColor Magenta

# Run controller tests
Write-Host "`n🎯 Testing Controllers..." -ForegroundColor Green
dotnet test --filter "FullyQualifiedName~ControllerTests" --verbosity normal

# Run service tests
Write-Host "`n🔧 Testing Services..." -ForegroundColor Green
dotnet test --filter "FullyQualifiedName~ServiceTests" --verbosity normal

# Run all tests to ensure no regressions
Write-Host "`n🌐 Running all tests..." -ForegroundColor Green
dotnet test --verbosity normal

Write-Host "`n🎉 Quick test run completed!" -ForegroundColor Green
Write-Host "📊 Check the output above for test results" -ForegroundColor White
