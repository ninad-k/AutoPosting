using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AutoPosting.ServerMonitor
{
    public class NotificationService
    {
        private readonly AppConfig _config;

        public NotificationService(AppConfig config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, byte[] attachmentData)
        {
            if (string.IsNullOrEmpty(_config.SmtpHost) || string.IsNullOrEmpty(_config.SmtpUser))
            {
                throw new InvalidOperationException("SMTP settings are not configured.");
            }

            using (var client = new SmtpClient(_config.SmtpHost, _config.SmtpPort))
            {
                client.Credentials = new NetworkCredential(_config.SmtpUser, _config.SmtpPass);
                client.EnableSsl = true;

                var mail = new MailMessage(_config.SmtpUser, to, subject, "Please see the attached screenshot.");
                if (attachmentData != null && attachmentData.Length > 0)
                {
                    mail.Attachments.Add(new Attachment(new MemoryStream(attachmentData), $"screenshot_{DateTime.Now:yyyyMMddHHmmss}.png", "image/png"));
                }

                await client.SendMailAsync(mail);
            }
        }

        public async Task SendWhatsAppAsync(string phone, byte[] attachmentData)
        {
             if (string.IsNullOrEmpty(_config.WhatsAppApiUrl))
            {
                throw new InvalidOperationException("WhatsApp API URL is not configured.");
            }

            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    if (attachmentData != null && attachmentData.Length > 0)
                    {
                        var imageContent = new ByteArrayContent(attachmentData);
                        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                        content.Add(imageContent, "file", $"screenshot_{DateTime.Now:yyyyMMddHHmmss}.png");
                    }
                    
                    // Add other parameters if needed, e.g. phone number or message
                    // Some APIs take 'phone', 'apikey', 'text' as query params or form data.
                    // Assuming generic post for now, appending API key to URL if present.
                    
                    // Specific handling for CallMeBot:
                    // https://api.callmebot.com/whatsapp.php?phone=[phone]&text=[text]&apikey=[apikey]
                    // But that's for text. For images, it depends on the API.
                    // The user said "send to selected method".
                    // Let's assume the API URL accepts the file.
                    
                    string url = _config.WhatsAppApiUrl;
                    if (!string.IsNullOrEmpty(_config.WhatsAppApiKey))
                    {
                         url += url.Contains("?") ? $"&apikey={_config.WhatsAppApiKey}" : $"?apikey={_config.WhatsAppApiKey}";
                    }
                    // Often need to pass phone number too
                    if (!string.IsNullOrEmpty(phone))
                    {
                        url += url.Contains("?") ? $"&phone={phone}" : $"?phone={phone}";
                    }

                    var response = await client.PostAsync(url, content);
                    if (!response.IsSuccessStatusCode)
                    {
                         throw new HttpRequestException($"WhatsApp API failed: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                    }
                }
            }
        }
    }
}
