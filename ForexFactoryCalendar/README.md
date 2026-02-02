# ForexFactory Calendar Auto-Emailer

Automated system to fetch ForexFactory economic calendar events, filter for Today's USD events, and email them.

## Setup

1.  **Prerequisites**:
    *   Node.js (v18+)
    *   Azure Functions Core Tools
    *   An SMTP account (e.g., Gmail App Password)

2.  **Install Dependencies**:
    ```bash
    cd ForexFactoryCalendar
    npm install
    ```
    *Note: Playwright might need browser binaries. If running locally, it installs them automatically. On Azure, you might need to use a specific Docker image or configure the path.*

3.  **Configuration**:
    *   Copy `.env.example` to `.env` (for local run, use `local.settings.json` logic or manually set env vars in Azure Portal).
    *   **IMPORTANT**: For local `func start`, Azure Functions uses `local.settings.json`.
    
    Create `local.settings.json`:
    ```json
    {
      "IsEncrypted": false,
      "Values": {
        "AzureWebJobsStorage": "",
        "FUNCTIONS_WORKER_RUNTIME": "node",
        "TARGET_URL": "https://www.forexfactory.com/calendar/apply-settings/1?navigation=0",
        "TIMEZONE": "Asia/Kolkata",
        "EMAIL_FROM": "...",
        "EMAIL_TO": "...",
        "SMTP_HOST": "smtp.gmail.com",
        "SMTP_PORT": "587",
        "SMTP_USERNAME": "...",
        "SMTP_PASSWORD": "..."
      }
    }
    ```

4.  **Run Locally**:
    ```bash
    func start
    ```
    The timer triggers at 2:00 PM IST (8:30 UTC). To test immediately, you can trigger it via HTTP (if you add an HTTP trigger) or use the Azure Admin API, or just modify the CRON to run soon.
    
    *Or temporary modification: Change `runOnStartup: true` in `function.json` for testing.*

## Deployment to Azure

1.  Create a Function App (Node.js stack) in Azure Portal.
2.  Go to **Configuration** (Environment Variables) and add all keys from `.env.example`.
3.  Deploy:
    ```bash
    func azure functionapp publish <APP_NAME>
    ```

## Notes
- **Timezone**: The scripts use `moment-timezone` to handle IST explicitly.
- **Resilience**: The scraper sets a standard User-Agent.
- **Fallback**: Includes `screenshot.js` (requires Playwright) which renders the generated HTML table to an image if needed (currently optional/manual integration in orchestrator).

