# Test Login Response Script
$loginUrl = "http://localhost:61376/api/auth/login"
$loginData = @{
    email = "systemadmin@test.com"
    password = "Admin123!"
}

$headers = @{
    "Content-Type" = "application/json"
}

Write-Host "Testing login response structure..." -ForegroundColor Yellow

try {
    $body = $loginData | ConvertTo-Json
    $response = Invoke-RestMethod -Uri $loginUrl -Method POST -Headers $headers -Body $body -TimeoutSec 30
    Write-Host "LOGIN SUCCESS!" -ForegroundColor Green
    Write-Host "Full Response Structure:" -ForegroundColor Cyan
    Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor White
    
} catch {
    Write-Host "Login failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Error response: $responseBody" -ForegroundColor Red
        } catch {
            Write-Host "Could not read error response" -ForegroundColor Red
        }
    }
}
