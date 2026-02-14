# deploy.ps1
param (
    [string]$AwsAccountId,
    [string]$Region = "us-east-1",
    [string]$RepoName = "screenshot-lambda-v2",
    [string]$FunctionName = "AutoPosting-ForexFactory-Screenshot"
)

if ([string]::IsNullOrWhiteSpace($AwsAccountId)) {
    Write-Host "Usage: .\deploy.ps1 -AwsAccountId <123456789012> [-Region <us-east-1>]" -ForegroundColor Yellow
    exit
}

$EcrUri = "$AwsAccountId.dkr.ecr.$Region.amazonaws.com"
$ImageTag = "$EcrUri/$RepoName`:latest"

# Ensure script directory
Set-Location $PSScriptRoot

# 1. Login
Write-Host "Logging into AWS ECR..." -ForegroundColor Green
aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin $EcrUri
if ($LASTEXITCODE -ne 0) { Write-Error "ECR Login failed."; exit 1 }

# 2. Build & Tag
Write-Host "Building Docker Image..." -ForegroundColor Green
docker build --platform linux/amd64 -t $RepoName .
docker tag "$RepoName`:latest" $ImageTag

# 3. Push
Write-Host "Pushing to ECR..." -ForegroundColor Green
docker push $ImageTag

# 4. Update Lambda
Write-Host "Updating Lambda Function..." -ForegroundColor Green
# Try to create function if it implies it might not exist, but 'update-function-code' requires existence.
# For a "simple" script, we assume user created the function placeholder or we can try to create it here (complex).
# Let's stick to update, but warn if it fails.

aws lambda update-function-code --function-name $FunctionName --image-uri $ImageTag --publish

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Update failed. Does the function '$FunctionName' exist? If not, create it in AWS Console first (choose Container Image)."
}
else {
    Write-Host "Deployment Completed!" -ForegroundColor Cyan
}
