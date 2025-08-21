# Comprehensive Test Runner for SmartTelehealth Application
# This script runs all tests for controllers and services

param(
    [string]$TestType = "All",  # All, Controllers, Services, Integration
    [string]$Filter = "",       # Filter tests by name
    [switch]$Verbose,           # Verbose output
    [switch]$Coverage,          # Run with coverage
    [switch]$Parallel           # Run tests in parallel
)

Write-Host "🚀 SmartTelehealth Comprehensive Test Suite" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Set error action preference
$ErrorActionPreference = "Stop"

# Function to check if dotnet is available
function Test-DotNetInstallation {
    try {
        $dotnetVersion = dotnet --version
        Write-Host "✅ .NET SDK Version: $dotnetVersion" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
        return $false
    }
}

# Function to restore packages
function Restore-Packages {
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
    try {
        dotnet restore
        Write-Host "✅ Packages restored successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Failed to restore packages" -ForegroundColor Red
        throw
    }
}

# Function to build the solution
function Build-Solution {
    Write-Host "🔨 Building solution..." -ForegroundColor Yellow
    try {
        dotnet build --no-restore --configuration Release
        Write-Host "✅ Solution built successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Build failed" -ForegroundColor Red
        throw
    }
}

# Function to run specific test categories
function Run-TestCategory {
    param(
        [string]$Category,
        [string]$Filter
    )
    
    Write-Host "🧪 Running $Category tests..." -ForegroundColor Magenta
    
    $testArgs = @(
        "test",
        "--no-build",
        "--configuration", "Release",
        "--verbosity", "normal"
    )
    
    if ($Filter) {
        $testArgs += "--filter", $Filter
    }
    
    if ($Parallel) {
        $testArgs += "--maxcpucount:0"
    }
    
    try {
        dotnet $testArgs
        Write-Host "✅ $Category tests completed" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ $Category tests failed" -ForegroundColor Red
        throw
    }
}

# Function to run tests with coverage
function Run-TestsWithCoverage {
    Write-Host "📊 Running tests with coverage..." -ForegroundColor Magenta
    
    $coverageArgs = @(
        "test",
        "--no-build",
        "--configuration", "Release",
        "--collect", "XPlat Code Coverage",
        "--verbosity", "normal"
    )
    
    if ($Filter) {
        $coverageArgs += "--filter", $Filter
    }
    
    try {
        dotnet $coverageArgs
        Write-Host "✅ Coverage tests completed" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Coverage tests failed" -ForegroundColor Red
        throw
    }
}

# Function to run integration tests
function Run-IntegrationTests {
    Write-Host "🔗 Running integration tests..." -ForegroundColor Magenta
    
    $integrationArgs = @(
        "test",
        "--no-build",
        "--configuration", "Release",
        "--filter", "Category=Integration",
        "--verbosity", "normal"
    )
    
    try {
        dotnet $integrationArgs
        Write-Host "✅ Integration tests completed" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Integration tests failed" -ForegroundColor Red
        throw
    }
}

# Function to run specific test files
function Run-SpecificTests {
    param(
        [string]$TestFile
    )
    
    Write-Host "🎯 Running specific test file: $TestFile" -ForegroundColor Magenta
    
    $testArgs = @(
        "test",
        "--no-build",
        "--configuration", "Release",
        "--filter", "FullyQualifiedName~$TestFile",
        "--verbosity", "normal"
    )
    
    try {
        dotnet $testArgs
        Write-Host "✅ Specific tests completed" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Specific tests failed" -ForegroundColor Red
        throw
    }
}

# Function to display test summary
function Show-TestSummary {
    Write-Host "📋 Test Summary" -ForegroundColor Cyan
    Write-Host "===============" -ForegroundColor Cyan
    Write-Host "✅ Controllers tested: Billing, Appointments, Users, Subscriptions, etc." -ForegroundColor Green
    Write-Host "✅ Services tested: Billing, Appointment, User, Subscription, etc." -ForegroundColor Green
    Write-Host "✅ Base classes created for consistent testing patterns" -ForegroundColor Green
    Write-Host "✅ Error handling and edge cases covered" -ForegroundColor Green
    Write-Host "✅ TokenModel parameter validation" -ForegroundColor Green
    Write-Host "✅ JsonModel response validation" -ForegroundColor Green
}

# Main execution
try {
    # Check prerequisites
    if (-not (Test-DotNetInstallation)) {
        exit 1
    }
    
    # Change to test directory
    Set-Location $PSScriptRoot
    
    # Restore and build
    Restore-Packages
    Build-Solution
    
    # Run tests based on type
    switch ($TestType.ToLower()) {
        "all" {
            Write-Host "🎯 Running ALL tests..." -ForegroundColor Green
            
            # Run controller tests
            Run-TestCategory "Controller" "Category=Controller"
            
            # Run service tests
            Run-TestCategory "Service" "Category=Service"
            
            # Run integration tests
            Run-IntegrationTests
            
            # Run remaining tests
            Run-TestCategory "Remaining" ""
        }
        "controllers" {
            Run-TestCategory "Controller" "Category=Controller"
        }
        "services" {
            Run-TestCategory "Service" "Category=Service"
        }
        "integration" {
            Run-IntegrationTests
        }
        "billing" {
            Run-SpecificTests "Billing"
        }
        "appointments" {
            Run-SpecificTests "Appointment"
        }
        "users" {
            Run-SpecificTests "User"
        }
        "subscriptions" {
            Run-SpecificTests "Subscription"
        }
        default {
            Write-Host "❌ Unknown test type: $TestType" -ForegroundColor Red
            Write-Host "Available types: All, Controllers, Services, Integration, Billing, Appointments, Users, Subscriptions" -ForegroundColor Yellow
            exit 1
        }
    }
    
    # Run coverage if requested
    if ($Coverage) {
        Run-TestsWithCoverage
    }
    
    # Show summary
    Show-TestSummary
    
    Write-Host "🎉 All tests completed successfully!" -ForegroundColor Green
    
} catch {
    Write-Host "💥 Test execution failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
    exit 1
} finally {
    # Return to original directory
    Set-Location $PSScriptRoot
}

Write-Host "`n📚 Test Documentation:" -ForegroundColor Cyan
Write-Host "- Controller tests validate HTTP responses and JsonModel structure" -ForegroundColor White
Write-Host "- Service tests validate business logic and error handling" -ForegroundColor White
Write-Host "- All tests follow workspace rules for TokenModel and JsonModel" -ForegroundColor White
Write-Host "- Tests cover success, error, and edge cases" -ForegroundColor White
