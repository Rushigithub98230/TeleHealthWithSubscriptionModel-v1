# Test Login Script
$apiUrl = "http://localhost:61376/api/auth/login"

$loginData = @{
    email = "admin@test.com"
    password = "Admin123!"
}

$headers = @{
    "Content-Type" = "application/json"
}

try {
    $body = $loginData | ConvertTo-Json
    $response = Invoke-RestMethod -Uri $apiUrl -Method POST -Headers $headers -Body $body
    Write-Host "Login successful!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json)" -ForegroundColor Green
} catch {
    Write-Host "Error during login: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody" -ForegroundColor Red
    }
}
