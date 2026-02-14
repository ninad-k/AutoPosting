# Screenshot to WhatsApp Function

This Azure Function (.NET 8 Isolated) captures a screenshot of a webpage and posts it to a webhook (e.g., for WhatsApp).

## 📂 Project Structure

- `ScreenshotFunction.cs`: The main function (TimerTrigger).
- `Services/PlaywrightScreenshotService.cs`: Handles browser automation.
- `Services/WebhookWhatsAppService.cs`: Handles sending the image.

## 🚀 Deployment Guide

### Prerequisites
1.  **Azure Function App**: Create a new Function App in Azure Portal.
    -   **Runtime stack**: .NET
    -   **Version**: 8 (LTS) Isolated Worker
    -   **OS**: Linux (Recommended for Playwright) or Windows.
2.  **Azure Functions Core Tools**: Ensure you have `func` CLI installed.

### Configuration
In your Azure Function App **Environment Variables** (Settings -> Configuration), add:

| Name | Value | Example |
|------|-------|---------|
| `TargetUrl` | URL to screenshot | `https://news.ycombinator.com/` |
| `WhatsAppWebhookUrl` | Your Webhook URL | `https://hook.eu1.make.com/...` |
| `ScheduleCron` | Cron expression | `0 0 9 * * *` (Daily at 9 AM) |

### How to Deploy

1.  Open PowerShell in this directory (`AutoPosting\ScreenshotFunction`).
2.  Run the deployment script:
    ```powershell
    .\deploy.ps1 -AppName "Your-Azure-Function-Name"
    ```

**Note on Linux:** If deploying to Linux, Playwright might require additional dependencies. The build process should handle the binaries, but if you see errors, ensure the Linux environment has the required libraries (or use the Docker container approach).

## 🛠️ Local Development

1.  **Install Browsers**:
    ```powershell
    powershell bin\Debug\net8.0\playwright.ps1 install
    ```
2.  **Configure**: Update `local.settings.json`.
3.  **Run**:
    ```powershell
    func start
    ```

## ☁️ AWS Lambda (Docker)

For AWS deployment, check the `ScreenshotLambda` folder in the project root.

1.  **Prerequisites**: Docker Desktop, AWS CLI (`aws configure`).
2.  **Build**:
    ```powershell
    cd ..\ScreenshotLambda
    .\build-docker.ps1
    ```
3.  **Deploy**:
    ```powershell
    .\deploy-aws.ps1 -AwsAccountId 123456789012 -Region us-east-1
    ```

## ⚡ Simple V2 (ForexFactory Only)

If you just want the ForexFactory screenshot without configuration:

1.  Go to `ScreenshotLambdaV2` folder.
2.  Deploy:
    ```powershell
    .\deploy.ps1 -AwsAccountId YOUR_AWS_ID
    ```
    *(This creates/updates a function named `AutoPosting-ForexFactory-Screenshot`)*

3.  **Configure**:
    In AWS Console -> Lambda -> Configuration -> Environment variables:
    -   `WA_API_URL`: Your WhatsApp API Endpoint.
    -   `WA_API_KEY`: (Optional) Your API Key.
    -   `WA_CHAT_ID`: Your Group ID (see below).
    -   `SMTP_HOST`: (Optional) E.g., `smtp.gmail.com`.
    -   `SMTP_USER`: (Optional) Your email.
    -   `SMTP_PASS`: (Optional) App Password.
    -   `EMAIL_TO`: (Optional) Comma-separated list (e.g., `a@a.com, b@b.com`).

### 🔎 How to find your Group ID (ChatID)

**Using CallMeBot (Free)**:
1.  Add `+34 644 67 23 26` (CallMeBot) to your phone contacts.
2.  Create a WhatsApp Group.
3.  Add the bot to the group.
4.  Send this message in the group: `@CallMeBot` (mention the bot).
5.  The bot will reply with the Group ID (e.g., `123456789-group@g.us`).
6.  Use this ID for `WA_CHAT_ID`.

**Using Paid APIs (GreenAPI, Whapi)**:
Check their specific documentation or API playground for a "Get Groups" endpoint. usually `GET /groups`.
