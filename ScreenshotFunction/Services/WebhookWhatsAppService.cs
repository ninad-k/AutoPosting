using RestSharp;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AutoPosting.ScreenshotFunction.Services
{
    public class WebhookWhatsAppService : IWhatsAppService
    {
        private readonly string _webhookUrl;
        private readonly ILogger<WebhookWhatsAppService> _logger;

        public WebhookWhatsAppService(string webhookUrl, ILogger<WebhookWhatsAppService> logger)
        {
            _webhookUrl = webhookUrl;
            _logger = logger;
        }

        public async Task SendImageAsync(byte[] imageBytes, string filename)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                _logger.LogWarning("WhatsApp Webhook URL is not configured. Skipping send.");
                return;
            }

            var client = new RestClient(_webhookUrl);
            var request = new RestRequest("/", Method.Post);
            request.AddFile("file", imageBytes, filename);

            try
            {
                var response = await client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    _logger.LogError($"Failed to send image to webhook. Status: {response.StatusCode}, content: {response.Content}");
                }
                else
                {
                    _logger.LogInformation("Successfully sent image to webhook.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending image to webhook.");
            }
        }
    }
}
