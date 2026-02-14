using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AutoPosting.ScreenshotFunction.Services
{
    public class PlaywrightScreenshotService : IScreenshotService
    {
        public async Task<byte[]> CaptureScreenshotAsync(string url)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync(new BrowserNewPageOptions { ViewportSize = new ViewportSize { Width = 1920, Height = 1080 } });

            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            
            // Basic consent handling (optional, similar to Python script)
            try {
                await page.GetByText("Accept").ClickAsync(new LocatorClickOptions { Timeout = 2000 });
            } catch {}

            // Wait a bit for dynamic content
            await page.WaitForTimeoutAsync(5000);

            return await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true });
        }
    }
}
