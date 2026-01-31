from playwright.sync_api import sync_playwright
import time
import os

def capture_screenshot(url, selector=None, output_path="screenshot.png"):
    """
    Captures a screenshot of a specific URL.
    If a selector is provided, captures only that element.
    Otherwise, captures the full page.
    """
    print(f"📸 Taking screenshot of {url}...")
    
    with sync_playwright() as p:
        # Launch browser (headless=True for background execution)
        browser = p.chromium.launch(headless=True)
        context = browser.new_context(viewport={"width": 1920, "height": 1080})
        page = context.new_page()
        
        try:
            page.goto(url, wait_until="networkidle", timeout=60000)
            
            # Basic cookie consent handling (generic attempt)
            try:
                # Common consent button text
                page.get_by_text("Accept", exact=True).click(timeout=2000)
            except:
                pass

            # Wait a bit for dynamic content / charts to render
            time.sleep(5) 
            
            if selector:
                print(f"   Searching for element: {selector}")
                try:
                    element = page.locator(selector).first
                    element.screenshot(path=output_path)
                except Exception as e:
                    print(f"   ⚠️ Could not find element '{selector}'. Taking full page screenshot instead.")
                    page.screenshot(path=output_path, full_page=True)
            else:
                page.screenshot(path=output_path, full_page=True)
                
            print(f"✅ Screenshot saved to {output_path}")
            return output_path
            
        except Exception as e:
            print(f"❌ Error capturing screenshot: {e}")
            return None
        finally:
            browser.close()

if __name__ == "__main__":
    # Test run
    capture_screenshot("https://news.ycombinator.com", ".hnmain")
