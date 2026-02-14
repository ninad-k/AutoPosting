# ==========================================
# 🚀 Production Dockerfile for AWS Lambda (.NET 8 + Playwright)
# ==========================================

# 1. Base Stage: Uses Microsoft's Playwright image (includes correct Chromium + Deps)
# We use the 'jammy' tag to match AWS Lambda's underlying OS (Ubuntu 22.04) logic.
FROM mcr.microsoft.com/playwright/dotnet:v1.58.0-jammy AS base
WORKDIR /app
EXPOSE 80 443 8080

# Environment optimization for Lambda
ENV DOTNET_EnableWriteXorExecute=0

# 2. Build Stage: Uses .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ScreenshotLambdaV2.csproj", "./"]
# Restore dependencies (Layers cached here if csproj doesn't change)
RUN dotnet restore "ScreenshotLambdaV2.csproj"

COPY . .
WORKDIR "/src/."
# Build for Release
RUN dotnet build "ScreenshotLambdaV2.csproj" -c Release -o /app/build

# 3. Publish Stage: Publishes the self-contained executable
FROM build AS publish
RUN dotnet publish "ScreenshotLambdaV2.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final Stage: Runtime
FROM base AS final
WORKDIR /var/task

# Copy the published app
COPY --from=publish /app/publish .

# ⚠️ Setup Entrypoint for AWS Lambda Custom Runtime (via RuntimeSupport)
# Since we are using a custom base image (not public.ecr.aws/lambda/dotnet:8),
# we must invoke the dotnet executable directly.
# The 'Amazon.Lambda.RuntimeSupport' library inside our app handles the Lambda API loop.
ENTRYPOINT ["/usr/bin/dotnet", "ScreenshotLambdaV2.dll"]
