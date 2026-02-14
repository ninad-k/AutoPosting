# deploy-aws.ps1
param (
    [string]$AwsAccountId,
    [string]$Region = "us-east-1",
    [string]$RepoName = "screenshot-lambda",
    [string]$FunctionName = "AutoPosting-Screenshot"
)

if ([string]::IsNullOrWhiteSpace($AwsAccountId)) {
    Write-Host "Usage: .\deploy-aws.ps1 -AwsAccountId <123456789012> [-Region <us-east-1>]" -ForegroundColor Yellow
    exit
}

$EcrUri = "$AwsAccountId.dkr.ecr.$Region.amazonaws.com"
$ImageTag = "$EcrUri/$RepoName`:latest"

# 1. Login to ECR
Write-Host "Logging into AWS ECR..." -ForegroundColor Green
aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin $EcrUri

if ($LASTEXITCODE -ne 0) { Write-Error "ECR Login failed."; exit 1 }

# 2. Build & Tag
Write-Host "Building and Tagging Image..." -ForegroundColor Green
docker build -t $RepoName .
docker tag "$RepoName`:latest" $ImageTag

# 3. Push
Write-Host "Pushing to ECR..." -ForegroundColor Green
docker push $ImageTag

# 4. Update Lambda
Write-Host "Updating Lambda Function Code..." -ForegroundColor Green
aws lambda update-function-code --function-name $FunctionName --image-uri $ImageTag --publish

Write-Host "Deployment Completed!" -ForegroundColor Cyan
