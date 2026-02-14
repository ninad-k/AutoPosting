using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AutoPosting.ScreenshotFunction
{
    public class ScreenshotFunction
    {
        private readonly ILogger _logger;
        private readonly IScreenshotService _screenshotService;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IConfiguration _configuration;

        public ScreenshotFunction(ILoggerFactory loggerFactory, IScreenshotService screenshotService, IWhatsAppService whatsAppService, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<ScreenshotFunction>();
            _screenshotService = screenshotService;
            _whatsAppService = whatsAppService;
            _configuration = configuration;
        }

        [Function("ScreenshotFunction")]
        public async Task Run([TimerTrigger("%ScheduleCron%")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string targetUrl = _configuration["TargetUrl"];
            if (string.IsNullOrEmpty(targetUrl))
            {
                _logger.LogError("TargetUrl is not configured.");
                return;
            }

            try
            {
                _logger.LogInformation($"Taking screenshot of {targetUrl}...");
                byte[] screenshotBytes = await _screenshotService.CaptureScreenshotAsync(targetUrl);

                if (screenshotBytes != null && screenshotBytes.Length > 0)
                {
                    _logger.LogInformation("Screenshot captured successfully. Sending to WhatsApp...");
                    string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    await _whatsAppService.SendImageAsync(screenshotBytes, filename);
                }
                else
                {
                    _logger.LogWarning("Screenshot capture returned no data.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the screenshot process.");
            }
        }
    }
}
