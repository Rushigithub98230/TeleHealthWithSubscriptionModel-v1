# Smart Telehealth Platform Setup Script
Write-Host "Setting up Smart Telehealth Platform..." -ForegroundColor Green

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build the solution
Write-Host "Building the solution..." -ForegroundColor Yellow
dotnet build

# Create database and run migrations
Write-Host "Setting up database..." -ForegroundColor Yellow
dotnet ef database update --project src/SmartTelehealth.Infrastructure --startup-project src/SmartTelehealth.API

# Install required NuGet packages if not already installed
Write-Host "Installing required NuGet packages..." -ForegroundColor Yellow

# API project packages
dotnet add src/SmartTelehealth.API/SmartTelehealth.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/SmartTelehealth.API/SmartTelehealth.API.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/SmartTelehealth.API/SmartTelehealth.API.csproj package Swashbuckle.AspNetCore

# Infrastructure project packages
dotnet add src/SmartTelehealth.Infrastructure/SmartTelehealth.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/SmartTelehealth.Infrastructure/SmartTelehealth.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/SmartTelehealth.Infrastructure/SmartTelehealth.Infrastructure.csproj package Stripe.net
dotnet add src/SmartTelehealth.Infrastructure/SmartTelehealth.Infrastructure.csproj package SendGrid
dotnet add src/SmartTelehealth.Infrastructure/SmartTelehealth.Infrastructure.csproj package Azure.Storage.Blobs

# Application project packages
dotnet add src/SmartTelehealth.Application/SmartTelehealth.Application.csproj package AutoMapper
dotnet add src/SmartTelehealth.Application/SmartTelehealth.Application.csproj package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add src/SmartTelehealth.Application/SmartTelehealth.Application.csproj package FluentValidation
dotnet add src/SmartTelehealth.Application/SmartTelehealth.Application.csproj package FluentValidation.AspNetCore
dotnet add src/SmartTelehealth.Application/SmartTelehealth.Application.csproj package MediatR

Write-Host "Setup completed successfully!" -ForegroundColor Green
Write-Host "You can now run the application with: dotnet run --project src/SmartTelehealth.API" -ForegroundColor Cyan
Write-Host "The API will be available at: https://localhost:7001" -ForegroundColor Cyan
Write-Host "Swagger documentation will be available at: https://localhost:7001/swagger" -ForegroundColor Cyan 