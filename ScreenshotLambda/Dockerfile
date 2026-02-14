FROM mcr.microsoft.com/playwright/dotnet:v1.41.0-jammy AS base
WORKDIR /app

# Install AWS Lambda RIC (Runtime Interface Client) dependencies if needed
# The base image is Ubuntu 22.04 (jammy)
RUN apt-get update && apt-get install -y \
    zip \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ScreenshotLambda.csproj", "./"]
RUN dotnet restore "ScreenshotLambda.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ScreenshotLambda.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ScreenshotLambda.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .

# Install the AWS Lambda Runtime Interface Client
# (Since we are using a custom image, we need to ensure the entrypoint is correct)
# But Wait! The base image 'mcr.microsoft.com/playwright/dotnet' does NOT have the AWS RIC pre-installed.
# We need to add it, or use the AWS base image and install Playwright (which is harder).
# EASIER PATH: Use Microsoft's Playwright image and adding the dotnet-lambda-ric.
# However, for .NET 8, the executable is the entrypoint.

ENTRYPOINT ["/usr/bin/dotnet", "ScreenshotLambda.dll"]
