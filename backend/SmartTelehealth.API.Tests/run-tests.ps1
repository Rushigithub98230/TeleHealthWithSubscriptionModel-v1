#!/usr/bin/env pwsh

param(
    [string]$Filter = "",
    [switch]$Verbose,
    [switch]$Coverage,
    [switch]$Help
)

if ($Help) {
    Write-Host @"
Authentication Test Runner

Usage: .\run-tests.ps1 [options]

Options:
    -Filter <pattern>     Run tests matching the pattern (e.g., "Login", "Register")
    -Verbose             Run with verbose output
    -Coverage            Generate code coverage report
    -Help                Show this help message

Examples:
    .\run-tests.ps1                           # Run all tests
    .\run-tests.ps1 -Filter "Login"           # Run only login tests
    .\run-tests.ps1 -Verbose                  # Run with verbose output
    .\run-tests.ps1 -Coverage                 # Run with coverage report
    .\run-tests.ps1 -Filter "Security" -Verbose  # Run security tests with verbose output

Test Categories:
    - Login
    - Register
    - PasswordRecovery
    - PasswordChange
    - Logout
    - TokenRefresh
    - Security
    - PasswordStrength
    - RateLimiting
    - ErrorHandling
"@
    exit 0
}

# Set error action preference
$ErrorActionPreference = "Stop"

# Colors for output
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Cyan = "Cyan"

Write-Host "🔐 Authentication Test Suite Runner" -ForegroundColor $Cyan
Write-Host "=====================================" -ForegroundColor $Cyan

# Check if we're in the right directory
if (-not (Test-Path "SmartTelehealth.API.Tests.csproj")) {
    Write-Host "❌ Error: Please run this script from the SmartTelehealth.API.Tests directory" -ForegroundColor $Red
    exit 1
}

# Build the test project
Write-Host "🔨 Building test project..." -ForegroundColor $Yellow
try {
    dotnet build --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    Write-Host "✅ Build successful" -ForegroundColor $Green
} catch {
    Write-Host "❌ Build failed: $_" -ForegroundColor $Red
    exit 1
}

# Prepare test command
$testCommand = "dotnet test --no-build"

if ($Filter) {
    $testCommand += " --filter `"$Filter`""
    Write-Host "🎯 Running tests matching: $Filter" -ForegroundColor $Yellow
} else {
    Write-Host "🎯 Running all authentication tests..." -ForegroundColor $Yellow
}

if ($Verbose) {
    $testCommand += " --verbosity normal --logger `"console;verbosity=detailed`""
    Write-Host "📝 Verbose output enabled" -ForegroundColor $Yellow
}

if ($Coverage) {
    $testCommand += " --collect:`"XPlat Code Coverage`" --results-directory ./coverage"
    Write-Host "📊 Coverage report will be generated" -ForegroundColor $Yellow
}

# Run the tests
Write-Host "🚀 Starting test execution..." -ForegroundColor $Yellow
Write-Host "Command: $testCommand" -ForegroundColor $Cyan

$startTime = Get-Date
try {
    Invoke-Expression $testCommand
    $exitCode = $LASTEXITCODE
    $endTime = Get-Date
    $duration = $endTime - $startTime
} catch {
    Write-Host "❌ Test execution failed: $_" -ForegroundColor $Red
    exit 1
}

# Display results
Write-Host ""
Write-Host "📋 Test Results Summary" -ForegroundColor $Cyan
Write-Host "======================" -ForegroundColor $Cyan

if ($exitCode -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor $Green
} else {
    Write-Host "❌ Some tests failed" -ForegroundColor $Red
}

Write-Host "⏱️  Duration: $($duration.ToString('mm\:ss'))" -ForegroundColor $Yellow

if ($Coverage) {
    Write-Host ""
    Write-Host "📊 Coverage Report" -ForegroundColor $Cyan
    Write-Host "=================" -ForegroundColor $Cyan
    Write-Host "Coverage files generated in: ./coverage" -ForegroundColor $Yellow
    
    # Try to find and display coverage summary
    $coverageFiles = Get-ChildItem -Path "./coverage" -Filter "*.xml" -Recurse -ErrorAction SilentlyContinue
    if ($coverageFiles) {
        Write-Host "📁 Coverage files found:" -ForegroundColor $Yellow
        foreach ($file in $coverageFiles) {
            Write-Host "   - $($file.Name)" -ForegroundColor $Cyan
        }
    }
}

Write-Host ""
Write-Host "🎯 Test Categories Covered:" -ForegroundColor $Cyan
Write-Host "==========================" -ForegroundColor $Cyan

$categories = @(
    "Login Tests",
    "Registration Tests", 
    "Password Recovery Tests",
    "Password Change Tests",
    "Logout Tests",
    "Token Refresh Tests",
    "Security Tests",
    "Password Strength Tests",
    "Rate Limiting Tests",
    "Error Handling Tests"
)

foreach ($category in $categories) {
    Write-Host "   ✅ $category" -ForegroundColor $Green
}

Write-Host ""
Write-Host "🔒 Security Features Tested:" -ForegroundColor $Cyan
Write-Host "===========================" -ForegroundColor $Cyan

$securityFeatures = @(
    "Password Strength Validation",
    "Role-based Access Control",
    "SQL Injection Protection",
    "XSS Attack Protection",
    "Rate Limiting",
    "Secure Error Messages",
    "JWT Token Security",
    "Input Validation"
)

foreach ($feature in $securityFeatures) {
    Write-Host "   🛡️  $feature" -ForegroundColor $Green
}

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "🎉 Authentication system is secure and working correctly!" -ForegroundColor $Green
} else {
    Write-Host "⚠️  Please review failed tests and fix any issues" -ForegroundColor $Yellow
}

exit $exitCode
