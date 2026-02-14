using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AutoPosting.ScreenshotFunction.Services;
using AutoPosting.ScreenshotFunction;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IScreenshotService, PlaywrightScreenshotService>();
        services.AddSingleton<IWhatsAppService>(sp => 
        {
            var config = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<WebhookWhatsAppService>>();
            var url = config["WhatsAppWebhookUrl"];
            return new WebhookWhatsAppService(url, logger);
        });
    })
    .Build();

host.Run();
