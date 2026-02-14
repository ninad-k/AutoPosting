# build-docker.ps1
param (
    [string]$ImageName = "screenshot-lambda"
)

Write-Host "Building Docker image: $ImageName..." -ForegroundColor Green

# Ensure we are in the script's directory
Set-Location $PSScriptRoot

# Build the image
docker build -t $ImageName .

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful!" -ForegroundColor Cyan
    Write-Host "You can test it locally with:"
    Write-Host "docker run -p 9000:8080 $ImageName"
}
else {
    Write-Error "Build failed."
    exit 1
}
