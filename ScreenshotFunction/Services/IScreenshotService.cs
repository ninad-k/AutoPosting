using System.Threading.Tasks;

namespace AutoPosting.ScreenshotFunction
{
    public interface IScreenshotService
    {
        Task<byte[]> CaptureScreenshotAsync(string url);
    }
}
