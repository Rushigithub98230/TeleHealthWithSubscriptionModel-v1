# Test Backend Connection Script
$apiUrl = "http://localhost:61376/api/auth/register"

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

Write-Host "Testing backend at: $apiUrl" -ForegroundColor Yellow

try {
    $body = $adminUser | ConvertTo-Json
    Write-Host "Sending request..." -ForegroundColor Cyan
    
    $response = Invoke-RestMethod -Uri $apiUrl -Method POST -Headers $headers -Body $body -TimeoutSec 30
    Write-Host "SUCCESS! Admin user created!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Login Credentials:" -ForegroundColor Yellow
    Write-Host "Email: systemadmin@test.com" -ForegroundColor White
    Write-Host "Password: Admin123!" -ForegroundColor White
    
    # Test login
    Write-Host ""
    Write-Host "Testing login..." -ForegroundColor Yellow
    
    $loginUrl = "http://localhost:61376/api/auth/login"
    $loginData = @{
        email = "systemadmin@test.com"
        password = "Admin123!"
    }
    
    $loginBody = $loginData | ConvertTo-Json
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method POST -Headers $headers -Body $loginBody -TimeoutSec 30
    Write-Host "LOGIN SUCCESS!" -ForegroundColor Green
    Write-Host "Token: $($loginResponse.token.Substring(0, 20))..." -ForegroundColor Green
    Write-Host "User: $($loginResponse.user.firstName) $($loginResponse.user.lastName) ($($loginResponse.user.role))" -ForegroundColor Green
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Response Body: $responseBody" -ForegroundColor Red
            
            # Check if user already exists
            if ($responseBody -like "*already exists*") {
                Write-Host ""
                Write-Host "User already exists. Testing login..." -ForegroundColor Yellow
                
                $loginUrl = "http://localhost:61376/api/auth/login"
                $loginData = @{
                    email = "systemadmin@test.com"
                    password = "Admin123!"
                }
                
                try {
                    $loginBody = $loginData | ConvertTo-Json
                    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method POST -Headers $headers -Body $loginBody -TimeoutSec 30
                    Write-Host "LOGIN SUCCESS!" -ForegroundColor Green
                    Write-Host "Token: $($loginResponse.token.Substring(0, 20))..." -ForegroundColor Green
                    Write-Host "User: $($loginResponse.user.firstName) $($loginResponse.user.lastName) ($($loginResponse.user.role))" -ForegroundColor Green
                } catch {
                    Write-Host "Login failed: $($_.Exception.Message)" -ForegroundColor Red
                    if ($_.Exception.Response) {
                        $loginReader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                        $loginResponseBody = $loginReader.ReadToEnd()
                        Write-Host "Login error response: $loginResponseBody" -ForegroundColor Red
                    }
                }
            }
        } catch {
            Write-Host "Could not read error response" -ForegroundColor Red
        }
    }
}
