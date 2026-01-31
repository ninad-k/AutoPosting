# Daily Screenshot Bot

A Python automated bot that visits websites, takes screenshots of specific elements (like charts or tables), and emails them to you daily.

## Features
- 📸 **High-Definition Screenshots**: Uses Playwright for perfect rendering of complex JS-heavy sites (TradingView, ForexFactory, etc.).
- 🎯 **element Targeting**: Can screenshot just a specific chart or table, or the entire page.
- ⏰ **Scheduler**: Runs automatically at a specific time every day.
- 📧 **Email Notifications**: clear visual reports delivered to your inbox.
- ✅ **Unit Tested**: Includes mocks for safe testing.

## Setup

### 1. Prerequisites
- Python 3.8+
- Chrome/Chromium installed (handled by script)

### 2. Installation
```bash
# Create and activate virtual environment
python3 -m venv venv
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt

# Install browser binaries
playwright install chromium
```

### 3. Configuration
1. Copy `.env.example` to `.env`:
   ```bash
   cp .env.example .env
   ```
2. Edit `.env` with your details:
   - **SMTP Settings**: Use an App Password if using Gmail (Security > 2-Step Verification > App Passwords).
   - **TARGET_URLS**: Format is `URL|CSS_SELECTOR`.
     - Example: `https://site.com/chart|.main-chart-div`
     - If you want full page, just put the URL: `https://site.com/chart`
   - **SCHEDULE_TIME**: 24-hour format (e.g., `08:00`).

## Usage

Run the bot:
```bash
python -m src.main
```
Keep the terminal open (or run in background/screen) for the scheduler to work.

## Testing

Run unit tests (safe, does not send emails):
```bash
pytest
```

## Project Structure
- `src/`: Source code
  - `main.py`: Scheduler and entry point
  - `screenshot.py`: Browser automation
  - `mailer.py`: Email logic
- `tests/`: Unit tests
- `requirements.txt`: Dependencies