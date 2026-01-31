import os
from dotenv import load_dotenv

load_dotenv()

# Email Configuration
SMTP_SERVER = os.getenv("SMTP_SERVER", "smtp.gmail.com")
SMTP_PORT = int(os.getenv("SMTP_PORT", 587))
SMTP_USER = os.getenv("SMTP_USER")
SMTP_PASSWORD = os.getenv("SMTP_PASSWORD")
SENDER_EMAIL = os.getenv("SENDER_EMAIL", SMTP_USER)
RECIPIENT_EMAILS = os.getenv("RECIPIENT_EMAILS", "").split(",")

# Screenshot Configuration
# Format: "URL|SELECTOR" (Selector is optional)
TARGET_URLS = os.getenv("TARGET_URLS", "").split(";")

# Schedule Configuration
SCHEDULE_TIME = os.getenv("SCHEDULE_TIME", "08:00")
