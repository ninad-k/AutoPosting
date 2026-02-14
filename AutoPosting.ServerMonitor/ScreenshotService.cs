using Microsoft.Playwright;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AutoPosting.ServerMonitor
{
    public class ScreenshotService
    {
        public async Task<byte[]> CaptureAsync(string url)
        {
            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
                { 
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-gpu" } 
                });
                
                var page = await browser.NewPageAsync(new BrowserNewPageOptions 
                { 
                    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 } 
                });
                
                await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });
                
                // transform into a more robust wait
                try { await page.WaitForLoadStateAsync(LoadState.NetworkIdle); } catch {}
                
                // Attempt to dismiss cookie banners
                try { await page.GetByText("Accept").ClickAsync(new LocatorClickOptions { Timeout = 2000 }); } catch {}
                try { await page.GetByText("Agree").ClickAsync(new LocatorClickOptions { Timeout = 2000 }); } catch {}
                try { await page.GetByText("Consent").ClickAsync(new LocatorClickOptions { Timeout = 2000 }); } catch {}

                // Slight delay for rendering
                await page.WaitForTimeoutAsync(2000);
                
                return await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error capturing screenshot: {ex.Message}");
                // Return a small error image or rethrow? 
                // For now, rethrow to handle in UI
                throw;
            }
        }
    }
}
