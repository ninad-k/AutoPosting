import schedule
import time
from . import config
import datetime
from .screenshot import capture_screenshot
from .mailer import send_email_with_attachment
import os

def daily_job():
    print(f"\n🚀 Starting daily job at {datetime.datetime.now()}")
    
    screenshots = []
    
    for target in config.TARGET_URLS:
        if not target.strip():
            continue
            
        parts = target.split("|")
        url = parts[0]
        selector = parts[1] if len(parts) > 1 else None
        
        # Create a unique filename based on time
        timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"screenshot_{timestamp}_{len(screenshots)}.png"
        
        path = capture_screenshot(url, selector, filename)
        if path:
            screenshots.append(path)
    
    if screenshots:
        print("📧 Sending email with screenshots...")
        # Currently the mailer function sends one attachment, let's just send the first one or loop
        # For simplicity in this v1, if there are multiple, we execute send_email for each or modify mailer.
        # Let's send one email per screenshot for now to keep it simple, or just the first one if we assume single task.
        
        # Better approach: Modify mailer to accept list, OR just helper loop here.
        # User asked for "similar screenshot", likely one main dashboard.
        
        for path in screenshots:
             send_email_with_attachment(
                 f"Daily Update: {os.path.basename(path)}",
                 "Here is your scheduled screenshot.",
                 path
             )
             
        # Cleanup
        # for path in screenshots:
        #    os.remove(path)
            
    print("🏁 Job finished.\n")

def run_scheduler():
    print(f"⏰ Scheduler started. Will run daily at {config.SCHEDULE_TIME}")
    print("Press Ctrl+C to exit.")
    
    schedule.every().day.at(config.SCHEDULE_TIME).do(daily_job)
    
    # Also allow running immediately for testing checks? 
    # Uncomment next line to run once on startup
    # daily_job()
    
    while True:
        schedule.run_pending()
        time.sleep(60)

if __name__ == "__main__":
    if not config.TARGET_URLS:
        print("⚠️ No targets configured. Please edit .env file.")
        
    run_scheduler()
