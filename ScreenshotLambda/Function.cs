using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Playwright;
using RestSharp;
using System.Text.Json;

namespace AutoPosting.ScreenshotLambda;

public class Function
{
    private static async Task Main()
    {
        Func<string, ILambdaContext, Task<string>> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
            .Build()
            .RunAsync();
    }

    public static async Task<string> FunctionHandler(string input, ILambdaContext context)
    {
        context.Logger.LogInformation($"Function started. Input: {input}");

        // Configuration (Environment Variables)
        string targetUrl = Environment.GetEnvironmentVariable("TARGET_URL") ?? "https://news.ycombinator.com/";
        string waApiUrl = Environment.GetEnvironmentVariable("WA_API_URL");
        string waApiKey = Environment.GetEnvironmentVariable("WA_API_KEY");
        
        // 1. Capture Screenshot
        context.Logger.LogInformation($"Taking screenshot of {targetUrl}...");
        byte[] imageBytes;
        
        try 
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
            { 
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-gpu", "--single-process" } // Crucial for Lambda
            });
            
            var page = await browser.NewPageAsync(new BrowserNewPageOptions { ViewportSize = new ViewportSize { Width = 1920, Height = 1080 } });
            await page.GotoAsync(targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            
            // Basic consent handling
            try { await page.GetByText("Accept").ClickAsync(new LocatorClickOptions { Timeout = 2000 }); } catch {}
            
            await page.WaitForTimeoutAsync(3000);
            imageBytes = await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true });
            
            context.Logger.LogInformation($"Screenshot captured. Size: {imageBytes.Length} bytes.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Screenshot failed: {ex}");
            throw;
        }

        // 2. Send to WhatsApp API
        if (string.IsNullOrEmpty(waApiUrl)) 
        {
            context.Logger.LogWarning("WA_API_URL not configured. Skipping upload.");
            return "Screenshot taken but not sent (No API URL).";
        }

        context.Logger.LogInformation($"Sending to WhatsApp API: {waApiUrl}");
        
        try 
        {
            var client = new RestClient(waApiUrl);
            var request = new RestRequest("", Method.Post);
            
            // Add API Key header if exists
            if (!string.IsNullOrEmpty(waApiKey))
            {
                request.AddQueryParameter("apikey", waApiKey); // Common for CallMeBot
            }
            
            // CallMeBot style: /sendPhoto?phone=...&apikey=...&image=...
            // But strict file upload usually requires multipart.
            // Let's assume a generic POST with file for now, or adapt to a specific provider if user clarifies.
            // For CallMeBot specifically, it's easier to send the IMAGE URL if we hosted it, but since we have the bytes,
            // we might need a widely compatible method.
            
            // GENERIC POST (Multipart) - Works for Make, Zapier, Custom Endpoints
            request.AddFile("file", imageBytes, $"screenshot_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
            
            var response = await client.ExecuteAsync(request);
            
            if (response.IsSuccessful)
            {
                context.Logger.LogInformation("WhatsApp API response: Success");
                return "Success";
            }
            else
            {
                context.Logger.LogError($"WhatsApp API failed: {response.StatusCode} - {response.Content}");
                return $"Failed: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
             context.Logger.LogError($"Upload failed: {ex}");
             return "Upload Failed";
        }
    }
}
