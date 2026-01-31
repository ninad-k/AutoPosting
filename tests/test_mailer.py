import sys
import os
import pytest
from unittest.mock import MagicMock, patch

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from src import mailer

@patch('src.mailer.smtplib.SMTP')
def test_send_email_success(mock_smtp):
    """Test that email is sent with correct parameters"""
    # Setup mock
    mock_server = MagicMock()
    mock_smtp.return_value.__enter__.return_value = mock_server
    
    # Mock config to ensure we have credentials
    with patch('src.config.SMTP_USER', 'user@test.com'), \
         patch('src.config.SMTP_PASSWORD', 'pass'), \
         patch('src.config.SENDER_EMAIL', 'sender@test.com'), \
         patch('src.config.RECIPIENT_EMAILS', ['recipient@test.com']):
         
        mailer.send_email_with_attachment("Subject", "Body", None)
        
        # Assertions
        mock_server.starttls.assert_called_once()
        mock_server.login.assert_called_with('user@test.com', 'pass')
        mock_server.send_message.assert_called_once()

def test_send_email_no_creds():
    """Test correct exit if no credentials"""
    with patch('src.config.SMTP_USER', None):
         with patch('builtins.print') as mock_print:
             mailer.send_email_with_attachment("Subject", "Body", None)
             mock_print.assert_called_with("❌ Email credentials not set. Skipping email.")
