using MimeKit;
using MailKit.Net.Smtp;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Playwright;
using RestSharp;


namespace AutoPosting.ScreenshotLambdaV2;

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
        context.Logger.LogInformation($"Function started. Triggered by: {input}");

        // Configuration
        string targetUrl = "https://www.forexfactory.com/calendar?day=today";
        string waApiUrl = Environment.GetEnvironmentVariable("WA_API_URL");
        string waApiKey = Environment.GetEnvironmentVariable("WA_API_KEY");
        // Email Config
        string smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
        string smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
        string smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");
        string emailTo = Environment.GetEnvironmentVariable("EMAIL_TO"); // Comma separated

        context.Logger.LogInformation($"Navigating to {targetUrl}...");
        byte[] imageBytes;

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-gpu", "--single-process" }
            });

            var page = await browser.NewPageAsync(new BrowserNewPageOptions { ViewportSize = new ViewportSize { Width = 1200, Height = 1600 } });
            
            // Go to page
            await page.GotoAsync(targetUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });

            try { 
                await page.GetByRole(AriaRole.Button, new() { Name = "Agree" }).Or(page.GetByText("Agree")).First.ClickAsync(new LocatorClickOptions { Timeout = 3000 });
            } catch {}

            try {
                await page.WaitForSelectorAsync(".calendar__table", new PageWaitForSelectorOptions { Timeout = 10000 });
            } catch {
                context.Logger.LogWarning("Calendar table specific selector not found, taking full page.");
            }

            imageBytes = await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true });
            context.Logger.LogInformation($"Screenshot captured. Size: {imageBytes.Length} bytes.");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Screenshot failed: {ex}");
            throw;
        }

        var tasks = new List<Task<string>>();

        // Task A: WhatsApp
        if (!string.IsNullOrEmpty(waApiUrl))
        {
            tasks.Add(SendWhatsApp(imageBytes, waApiUrl, waApiKey, context));
        }

        // Task B: Email
        if (!string.IsNullOrEmpty(smtpHost) && !string.IsNullOrEmpty(emailTo))
        {
            tasks.Add(SendEmail(imageBytes, smtpHost, smtpUser, smtpPass, emailTo, context));
        }

        await Task.WhenAll(tasks);
        return "Done";
    }

    private static async Task<string> SendWhatsApp(byte[] imageBytes, string waApiUrl, string waApiKey, ILambdaContext context)
    {
        context.Logger.LogInformation($"Sending to WhatsApp API...");
        try
        {
            var client = new RestClient(waApiUrl);
            var request = new RestRequest("", Method.Post);

            if (!string.IsNullOrEmpty(waApiKey)) request.AddQueryParameter("apikey", waApiKey);
            
            string waChatId = Environment.GetEnvironmentVariable("WA_CHAT_ID");
            if (!string.IsNullOrEmpty(waChatId)) 
            {
                request.AddQueryParameter("chatId", waChatId);
                request.AddQueryParameter("phone", waChatId);
                request.AddQueryParameter("to", waChatId);
            }

            request.AddFile("file", imageBytes, $"forex_{DateTime.UtcNow:yyyyMMdd}.png");

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                context.Logger.LogInformation("WhatsApp Success!");
                return "WhatsApp: Success";
            }
            else
            {
                context.Logger.LogError($"WhatsApp failed: {response.StatusCode} {response.Content}");
                return $"WhatsApp: Failed";
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"WhatsApp error: {ex.Message}");
            return "WhatsApp: Error";
        }
    }

    private static async Task<string> SendEmail(byte[] imageBytes, string host, string user, string pass, string to, ILambdaContext context)
    {
        context.Logger.LogInformation($"Sending Email to {to}...");
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Screenshot Bot", user ?? "bot@example.com")); // Use sender email or generic
            
            foreach(var email in to.Split(','))
            {
                var trimmed = email.Trim();
                if(!string.IsNullOrEmpty(trimmed)) message.To.Add(new MailboxAddress("", trimmed));
            }

            message.Subject = $"Forex Factory Calendar - {DateTime.UtcNow:yyyy-MM-dd}";

            var builder = new BodyBuilder { TextBody = "Here is the latest Forex Factory Calendar screenshot." };
            builder.Attachments.Add($"forex_{DateTime.UtcNow:yyyyMMdd}.png", imageBytes);
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, 587, MailKit.Security.SecureSocketOptions.StartTls);
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                await client.AuthenticateAsync(user, pass);
            }
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            context.Logger.LogInformation("Email Sent Successfully!");
            return "Email: Success";
        }
        catch (Exception ex)
        {
             context.Logger.LogError($"Email failed: {ex.Message}");
             return "Email: Failed";
        }
    }
}
