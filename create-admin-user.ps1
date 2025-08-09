# Create Admin User Script
# Try different possible API URLs
$possibleUrls = @(
    "http://localhost:61376/api/auth/register",
    "http://localhost:58677/api/auth/register",
    "https://localhost:58676/api/auth/register"
)

$apiUrl = $null
foreach ($url in $possibleUrls) {
    try {
        $testUrl = $url -replace "/api/auth/register", "/api/health"
        Invoke-RestMethod -Uri $testUrl -Method GET -TimeoutSec 5 | Out-Null
        $apiUrl = $url
        Write-Host "Found working API at: $url" -ForegroundColor Green
        break
    } catch {
        Write-Host "Trying $url - not available" -ForegroundColor Gray
    }
}

if (-not $apiUrl) {
    Write-Host "No working API found. Make sure the backend is running." -ForegroundColor Red
    exit 1
}

# Try creating a fresh admin user
$adminUser = @{
    firstName = "System"
    lastName = "Admin"
    email = "systemadmin@test.com"
    password = "Admin123!"
    confirmPassword = "Admin123!"
    phoneNumber = "1234567890"
    gender = "Other"
    address = "System Admin Address"
    city = "Admin City"
    state = "Admin State"
    zipCode = "12345"
    role = "Admin"
}

$headers = @{
    "Content-Type" = "application/json"
}

Write-Host "Creating admin user..." -ForegroundColor Yellow

try {
    $body = $adminUser | ConvertTo-Json
    Write-Host "Sending request to: $apiUrl" -ForegroundColor Cyan
    Write-Host "Request body: $body" -ForegroundColor Gray
    
    $response = Invoke-RestMethod -Uri $apiUrl -Method POST -Headers $headers -Body $body -TimeoutSec 30
    Write-Host "Admin user created successfully!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Login Credentials:" -ForegroundColor Yellow
    Write-Host "Email: systemadmin@test.com" -ForegroundColor White
    Write-Host "Password: Admin123!" -ForegroundColor White
    
} catch {
    Write-Host "Error creating admin user: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Response: $responseBody" -ForegroundColor Red
        } catch {
            Write-Host "Could not read error response body" -ForegroundColor Red
        }
    }
}

# Test the login immediately
Write-Host ""
Write-Host "Testing login..." -ForegroundColor Yellow

$loginUrl = $apiUrl -replace "/api/auth/register", "/api/auth/login"
$loginData = @{
    email = "systemadmin@test.com"
    password = "Admin123!"
}

try {
    $loginBody = $loginData | ConvertTo-Json
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method POST -Headers $headers -Body $loginBody -TimeoutSec 30
    Write-Host "Login test successful!" -ForegroundColor Green
    Write-Host "Token received: $($loginResponse.token.Substring(0, 20))..." -ForegroundColor Green
    Write-Host "User: $($loginResponse.user.firstName) $($loginResponse.user.lastName) ($($loginResponse.user.role))" -ForegroundColor Green
} catch {
    Write-Host "Login test failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Login error response: $responseBody" -ForegroundColor Red
        } catch {
            Write-Host "Could not read login error response" -ForegroundColor Red
        }
    }
}
