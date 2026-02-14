using System.Threading.Tasks;

namespace AutoPosting.ScreenshotFunction
{
    public interface IWhatsAppService
    {
        Task SendImageAsync(byte[] imageBytes, string filename);
    }
}
