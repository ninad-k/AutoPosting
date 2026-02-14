# deploy.ps1
param (
    [string]$AppName
)

if ([string]::IsNullOrWhiteSpace($AppName)) {
    Write-Host "Usage: .\deploy.ps1 -AppName <YourAzureFunctionAppName>" -ForegroundColor Yellow
    exit
}

Write-Host "Starting deployment to Azure Function App: $AppName" -ForegroundColor Cyan

# Check if func tools are installed
if (-not (Get-Command "func" -ErrorAction SilentlyContinue)) {
    Write-Error "Azure Functions Core Tools ('func') not found. Please install them first."
    exit 1
}

# Build and Publish locally first (Release mode)
Write-Host "Building project in Release mode..." -ForegroundColor Green
dotnet publish -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Aborting deployment."
    exit 1
}

# Deploy
Write-Host "Publishing to Azure..." -ForegroundColor Green
# dotnet-isolated functions are published from the output directory of the dotnet publish command, 
# or we can just let 'func' handle it if we are in the root. 
# For dotnet-isolated, 'func azure functionapp publish' usually works fine if run from project root.
func azure functionapp publish $AppName --dotnet-isolated

Write-Host "Deployment completed!" -ForegroundColor Cyan
