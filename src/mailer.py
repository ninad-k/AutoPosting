import smtplib
from email.message import EmailMessage
from . import config
import mimetypes
import os

def send_email_with_attachment(subject, body, attachment_path):
    if not config.SMTP_USER or not config.SMTP_PASSWORD:
        print("❌ Email credentials not set. Skipping email.")
        return

    msg = EmailMessage()
    msg['Subject'] = subject
    msg['From'] = config.SENDER_EMAIL
    msg['To'] = ", ".join(config.RECIPIENT_EMAILS)
    msg.set_content(body)

    if attachment_path and os.path.exists(attachment_path):
        ctype, encoding = mimetypes.guess_type(attachment_path)
        if ctype is None or encoding is not None:
             # No guess could be made, or the file is encoded (compressed), so
             # use a generic bag-of-bits type.
             ctype = 'application/octet-stream'
        
        maintype, subtype = ctype.split('/', 1)
        
        with open(attachment_path, 'rb') as f:
            file_data = f.read()
            msg.add_attachment(
                file_data,
                maintype=maintype,
                subtype=subtype,
                filename=os.path.basename(attachment_path)
            )

    try:
        with smtplib.SMTP(config.SMTP_SERVER, config.SMTP_PORT) as server:
            server.starttls()
            server.login(config.SMTP_USER, config.SMTP_PASSWORD)
            server.send_message(msg)
            print("✅ Email sent successfully!")
            
    except Exception as e:
        print(f"❌ Failed to send email: {e}")

if __name__ == "__main__":
    # Test run
    # Create a dummy file to send if it doesn't exist
    if not os.path.exists("test_image.txt"):
        with open("test_image.txt", "w") as f: f.write("dummy image content")
        
    send_email_with_attachment("Test Daily Screenshot", "Here is your test screenshot.", "test_image.txt")
