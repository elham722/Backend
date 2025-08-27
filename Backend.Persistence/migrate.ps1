# PowerShell script to run Entity Framework migrations for Backend.Persistence

Write-Host "Starting migration process..." -ForegroundColor Green

# Set the working directory to the Persistence project
Set-Location $PSScriptRoot

# Check if EF tools are installed
try {
    dotnet ef --version
} catch {
    Write-Host "Entity Framework tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

# Add initial migration if it doesn't exist
Write-Host "Adding initial migration..." -ForegroundColor Blue
dotnet ef migrations add InitialCreate --startup-project ..\Backend.Api\Backend.Api.csproj

# Update database
Write-Host "Updating database..." -ForegroundColor Blue
dotnet ef database update --startup-project ..\Backend.Api\Backend.Api.csproj

Write-Host "Migration completed successfully!" -ForegroundColor Green 