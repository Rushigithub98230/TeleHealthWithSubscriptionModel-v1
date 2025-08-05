# Test Local File Storage Service
Write-Host "🧪 Testing LocalFileStorageService..." -ForegroundColor Green

# Navigate to the test project directory
Set-Location "backend/SmartTelehealth.Infrastructure.Tests"

# Run the tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test --verbosity normal

# Check if tests passed
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All LocalFileStorageService tests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failed. Please check the output above." -ForegroundColor Red
}

# Return to original directory
Set-Location "../.." 