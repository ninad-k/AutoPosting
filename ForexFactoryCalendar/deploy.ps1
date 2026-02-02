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

# Install production dependencies (pruning dev deps)
Write-Host "Installing/Pruning dependencies..." -ForegroundColor Green
# npm prune --production # Optional if we want to be strict, but npm install is safer for now if not run yet
call npm install --omit=dev

# Deploy
Write-Host "Publishing..." -ForegroundColor Green
func azure functionapp publish $AppName

Write-Host "Deployment completed!" -ForegroundColor Cyan
