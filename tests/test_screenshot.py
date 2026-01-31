import sys
import os
import pytest
from unittest.mock import MagicMock, patch

# Ensure src is in path for imports
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from src import screenshot

@patch('src.screenshot.sync_playwright')
def test_capture_screenshot_success(mock_playwright):
    """Test that capture_screenshot calls playwright correctly"""
    # Setup mocks
    mock_browser = MagicMock()
    mock_context = MagicMock()
    mock_page = MagicMock()
    
    mock_p = mock_playwright.return_value.__enter__.return_value
    mock_p.chromium.launch.return_value = mock_browser
    mock_browser.new_context.return_value = mock_context
    mock_context.new_page.return_value = mock_page
    
    # Run function
    url = "https://example.com"
    selector = ".test-class"
    output = "test.png"
    
    result = screenshot.capture_screenshot(url, selector, output)
    
    # Assertions
    assert result == output
    mock_page.goto.assert_called_with(url, wait_until="networkidle", timeout=60000)
    mock_page.locator.assert_called_with(selector)
    mock_page.locator(selector).first.screenshot.assert_called_with(path=output)
    mock_browser.close.assert_called_once()

@patch('src.screenshot.sync_playwright')
def test_capture_screenshot_full_page_fallback(mock_playwright):
    """Test fallback to full page if selector fails"""
    # Setup mocks
    mock_browser = MagicMock()
    mock_page = MagicMock()
    
    mock_p = mock_playwright.return_value.__enter__.return_value
    mock_p.chromium.launch.return_value = mock_browser
    mock_browser.new_context.return_value.new_page.return_value = mock_page
    
    # Simulate selector failure
    mock_page.locator.side_effect = Exception("Element not found")
    
    # Run function
    result = screenshot.capture_screenshot("https://example.com", ".bad-selector", "test.png")
    
    # Assertions
    assert result == "test.png"
    mock_page.screenshot.assert_called_with(path="test.png", full_page=True)
