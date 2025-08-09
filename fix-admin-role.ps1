# Fix Admin Role Script
Write-Host "Checking user roles and fixing admin user..." -ForegroundColor Yellow

# Let's create a proper admin user with correct role
$apiUrl = "http://localhost:61376/api/auth/register"

$adminUser = @{
    firstName = "Admin"
    lastName = "User" 
    email = "admin@telehealth.com"
    password = "Admin123!"
    confirmPassword = "Admin123!"
    phoneNumber = "1234567890"
    gender = "Other"
    address = "Admin Address"
    city = "Admin City"
    state = "Admin State"
    zipCode = "12345"
    roleName = "Admin"  # Try roleName instead of role
}

$headers = @{
    "Content-Type" = "application/json"
}

Write-Host "Creating new admin user with correct role..." -ForegroundColor Cyan

try {
    $body = $adminUser | ConvertTo-Json
    $response = Invoke-RestMethod -Uri $apiUrl -Method POST -Headers $headers -Body $body -TimeoutSec 30
    Write-Host "New admin user created!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json)" -ForegroundColor Green
    
    # Test login with new user
    Write-Host ""
    Write-Host "Testing login with new admin user..." -ForegroundColor Yellow
    
    $loginUrl = "http://localhost:61376/api/auth/login"
    $loginData = @{
        email = "admin@telehealth.com"
        password = "Admin123!"
    }
    
    $loginBody = $loginData | ConvertTo-Json
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method POST -Headers $headers -Body $loginBody -TimeoutSec 30
    Write-Host "LOGIN SUCCESS!" -ForegroundColor Green
    Write-Host "User Role: $($loginResponse.data.user.role)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Updated Login Credentials:" -ForegroundColor Yellow
    Write-Host "Email: admin@telehealth.com" -ForegroundColor White
    Write-Host "Password: Admin123!" -ForegroundColor White
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Response: $responseBody" -ForegroundColor Red
            
            if ($responseBody -like "*already exists*") {
                Write-Host ""
                Write-Host "User already exists. Testing login..." -ForegroundColor Yellow
                
                $loginUrl = "http://localhost:61376/api/auth/login"
                $loginData = @{
                    email = "admin@telehealth.com"
                    password = "Admin123!"
                }
                
                try {
                    $loginBody = $loginData | ConvertTo-Json
                    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method POST -Headers $headers -Body $loginBody -TimeoutSec 30
                    Write-Host "LOGIN SUCCESS!" -ForegroundColor Green
                    Write-Host "User Role: $($loginResponse.data.user.role)" -ForegroundColor Cyan
                    Write-Host ""
                    Write-Host "Login Credentials:" -ForegroundColor Yellow
                    Write-Host "Email: admin@telehealth.com" -ForegroundColor White
                    Write-Host "Password: Admin123!" -ForegroundColor White
                } catch {
                    Write-Host "Login failed: $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        } catch {
            Write-Host "Could not read error response" -ForegroundColor Red
        }
    }
}
