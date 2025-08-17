#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Comprehensive Test Runner for SmartTelehealth Subscription Management System

.DESCRIPTION
    This script runs all test suites for the subscription management system including:
    - Comprehensive subscription workflow tests
    - Stripe integration tests
    - Payment processing tests
    - Admin management tests
    - Error handling and edge case tests

.PARAMETER Configuration
    Build configuration (Debug/Release). Default is Debug.

.PARAMETER Verbosity
    Test output verbosity level. Default is normal.

.PARAMETER Filter
    Filter tests by name pattern.

.PARAMETER Coverage
    Enable code coverage collection.

.EXAMPLE
    .\run-comprehensive-tests.ps1

.EXAMPLE
    .\run-comprehensive-tests.ps1 -Configuration Release -Verbosity detailed

.EXAMPLE
    .\run-comprehensive-tests.ps1 -Filter "Stripe" -Coverage
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    
    [ValidateSet("quiet", "minimal", "normal", "detailed", "diagnostic")]
    [string]$Verbosity = "normal",
    
    [string]$Filter = "",
    
    [switch]$Coverage,
    
    [switch]$Help
)

# Show help if requested
if ($Help) {
    Get-Help $MyInvocation.MyCommand.Path -Full
    exit 0
}

# Script configuration
$ScriptName = "Comprehensive Subscription Test Runner"
$ScriptVersion = "1.0.0"
$StartTime = Get-Date

# Colors for output
$Colors = @{
    Header = "Cyan"
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "White"
    SubHeader = "Magenta"
}

# Test project configuration
$TestProjectPath = "SmartTelehealth.Tests"
$TestProjectFile = "$TestProjectPath/SmartTelehealth.Tests.csproj"
$TestResultsPath = "TestResults"
$CoveragePath = "Coverage"

# Ensure we're in the right directory
if (-not (Test-Path $TestProjectFile)) {
    Write-Host "Error: Test project file not found at $TestProjectFile" -ForegroundColor $Colors.Error
    Write-Host "Please run this script from the project root directory." -ForegroundColor $Colors.Error
    exit 1
}

# Function to write formatted output
function Write-FormattedOutput {
    param(
        [string]$Message,
        [string]$Level = "Info",
        [string]$Color = $Colors.Info
    )
    
    $timestamp = Get-Date -Format "HH:mm:ss"
    $levelIndicator = switch ($Level) {
        "Header" { "=== " }
        "SubHeader" { "--- " }
        "Success" { "✓ " }
        "Warning" { "⚠ " }
        "Error" { "✗ " }
        default { "" }
    }
    
    Write-Host "[$timestamp] $levelIndicator$Message" -ForegroundColor $Color
}

# Function to check prerequisites
function Test-Prerequisites {
    Write-FormattedOutput "Checking prerequisites..." "SubHeader" $Colors.SubHeader
    
    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-FormattedOutput ".NET SDK version: $dotnetVersion" "Info" $Colors.Success
    }
    catch {
        Write-FormattedOutput "Error: .NET SDK not found. Please install .NET 8.0 SDK." "Error" $Colors.Error
        return $false
    }
    
    # Check test project
    if (-not (Test-Path $TestProjectFile)) {
        Write-FormattedOutput "Error: Test project file not found." "Error" $Colors.Error
        return $false
    }
    
    # Check if test project builds
    Write-FormattedOutput "Building test project..." "Info" $Colors.Info
    $buildResult = dotnet build $TestProjectFile --configuration $Configuration --no-restore
    
    if ($LASTEXITCODE -ne 0) {
        Write-FormattedOutput "Error: Test project build failed." "Error" $Colors.Error
        return $false
    }
    
    Write-FormattedOutput "Prerequisites check completed successfully." "Success" $Colors.Success
    return $true
}

# Function to run tests
function Invoke-TestExecution {
    Write-FormattedOutput "Starting test execution..." "SubHeader" $Colors.SubHeader
    
    # Create test results directory
    if (-not (Test-Path $TestResultsPath)) {
        New-Item -ItemType Directory -Path $TestResultsPath -Force | Out-Null
    }
    
    # Build test arguments
    $testArgs = @(
        "test",
        $TestProjectFile,
        "--configuration", $Configuration,
        "--verbosity", $Verbosity,
        "--logger", "trx;LogFileName=TestResults.trx",
        "--results-directory", $TestResultsPath
    )
    
    # Add filter if specified
    if ($Filter) {
        $testArgs += "--filter", $Filter
        Write-FormattedOutput "Applying test filter: $Filter" "Info" $Colors.Info
    }
    
    # Add coverage if requested
    if ($Coverage) {
        $testArgs += "--collect", "XPlat Code Coverage"
        Write-FormattedOutput "Code coverage collection enabled" "Info" $Colors.Info
    }
    
    # Run tests
    Write-FormattedOutput "Executing tests with command: dotnet $($testArgs -join ' ')" "Info" $Colors.Info
    
    $testResult = dotnet $testArgs
    
    $exitCode = $LASTEXITCODE
    
    # Parse test results
    $testOutput = $testResult -join "`n"
    
    # Extract test summary
    if ($testOutput -match "Test Run Successful\.\s*Total:\s*(\d+),\s*Passed:\s*(\d+),\s*Failed:\s*(\d+),\s*Skipped:\s*(\d+)") {
        $totalTests = $matches[1]
        $passedTests = $matches[2]
        $failedTests = $matches[3]
        $skippedTests = $matches[4]
        
        Write-FormattedOutput "Test Results Summary:" "SubHeader" $Colors.SubHeader
        Write-FormattedOutput "Total Tests: $totalTests" "Info" $Colors.Info
        Write-FormattedOutput "Passed: $passedTests" "Success" $Colors.Success
        Write-FormattedOutput "Failed: $failedTests" $(if ($failedTests -gt 0) { "Error" } else { "Info" }) $(if ($failedTests -gt 0) { $Colors.Error } else { $Colors.Info })
        Write-FormattedOutput "Skipped: $skippedTests" "Info" $Colors.Info
    }
    
    return @{
        ExitCode = $exitCode
        Output = $testOutput
        Success = $exitCode -eq 0
    }
}

# Function to generate test report
function Generate-TestReport {
    param(
        [hashtable]$TestResult
    )
    
    Write-FormattedOutput "Generating test report..." "SubHeader" $Colors.SubHeader
    
    $reportPath = "$TestResultsPath/TestReport.html"
    
    # Create HTML report
    $htmlReport = @"
<!DOCTYPE html>
<html>
<head>
    <title>SmartTelehealth Subscription Management Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; }
        .summary { margin: 20px 0; }
        .success { color: green; }
        .error { color: red; }
        .warning { color: orange; }
        .info { color: blue; }
        .timestamp { color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class="header">
        <h1>SmartTelehealth Subscription Management Test Report</h1>
        <p class="timestamp">Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")</p>
    </div>
    
    <div class="summary">
        <h2>Test Execution Summary</h2>
        <p><strong>Status:</strong> <span class="$(if ($TestResult.Success) { 'success' } else { 'error' })">$(if ($TestResult.Success) { 'PASSED' } else { 'FAILED' })</span></p>
        <p><strong>Exit Code:</strong> $($TestResult.ExitCode)</p>
        <p><strong>Execution Time:</strong> $((Get-Date) - $StartTime)</p>
    </div>
    
    <div class="details">
        <h2>Test Details</h2>
        <pre>$($TestResult.Output)</pre>
    </div>
</body>
</html>
"@
    
    $htmlReport | Out-File -FilePath $reportPath -Encoding UTF8
    
    Write-FormattedOutput "Test report generated: $reportPath" "Success" $Colors.Success
    
    # Open report in default browser
    try {
        Start-Process $reportPath
    }
    catch {
        Write-FormattedOutput "Could not open report automatically. Please open: $reportPath" "Warning" $Colors.Warning
    }
}

# Function to handle coverage reports
function Handle-CoverageReport {
    if (-not $Coverage) {
        return
    }
    
    Write-FormattedOutput "Processing coverage report..." "SubHeader" $Colors.SubHeader
    
    # Look for coverage files
    $coverageFiles = Get-ChildItem -Path $TestResultsPath -Filter "*.coverage" -Recurse
    
    if ($coverageFiles) {
        Write-FormattedOutput "Coverage files found:" "Info" $Colors.Info
        foreach ($file in $coverageFiles) {
            Write-FormattedOutput "  - $($file.FullName)" "Info" $Colors.Info
        }
        
        # Generate coverage report using ReportGenerator if available
        try {
            $reportGenerator = Get-Command reportgenerator -ErrorAction SilentlyContinue
            if ($reportGenerator) {
                Write-FormattedOutput "Generating coverage report..." "Info" $Colors.Info
                
                if (-not (Test-Path $CoveragePath)) {
                    New-Item -ItemType Directory -Path $CoveragePath -Force | Out-Null
                }
                
                $coverageFile = $coverageFiles[0].FullName
                reportgenerator -reports:$coverageFile -targetdir:$CoveragePath
                
                Write-FormattedOutput "Coverage report generated in: $CoveragePath" "Success" $Colors.Success
            }
            else {
                Write-FormattedOutput "ReportGenerator not found. Install with: dotnet tool install -g dotnet-reportgenerator-globaltool" "Warning" $Colors.Warning
            }
        }
        catch {
            Write-FormattedOutput "Error generating coverage report: $($_.Exception.Message)" "Error" $Colors.Error
        }
    }
    else {
        Write-FormattedOutput "No coverage files found." "Warning" $Colors.Warning
    }
}

# Main execution
function Main {
    Write-FormattedOutput "Starting $ScriptName v$ScriptVersion" "Header" $Colors.Header
    Write-FormattedOutput "Configuration: $Configuration" "Info" $Colors.Info
    Write-FormattedOutput "Verbosity: $Verbosity" "Info" $Colors.Info
    Write-FormattedOutput "Coverage: $($Coverage.IsPresent)" "Info" $Colors.Info
    if ($Filter) {
        Write-FormattedOutput "Filter: $Filter" "Info" $Colors.Info
    }
    Write-Host ""
    
    # Check prerequisites
    if (-not (Test-Prerequisites)) {
        Write-FormattedOutput "Prerequisites check failed. Exiting." "Error" $Colors.Error
        exit 1
    }
    
    # Execute tests
    $testResult = Invoke-TestExecution
    
    # Generate report
    Generate-TestReport -TestResult $testResult
    
    # Handle coverage if enabled
    Handle-CoverageReport
    
    # Final summary
    Write-Host ""
    Write-FormattedOutput "Test execution completed." "SubHeader" $Colors.SubHeader
    Write-FormattedOutput "Total execution time: $((Get-Date) - $StartTime)" "Info" $Colors.Info
    
    if ($testResult.Success) {
        Write-FormattedOutput "All tests passed successfully!" "Success" $Colors.Success
    }
    else {
        Write-FormattedOutput "Some tests failed. Please review the test report." "Error" $Colors.Error
    }
    
    # Exit with test result code
    exit $testResult.ExitCode
}

# Execute main function
Main
