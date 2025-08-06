using System.Threading.Tasks;
using ScreenshotCommonLib.Enums;

namespace ScreenshotClient.Domain.Interfaces
{
    public interface IScreenshotService
    {
        Task<byte[]> CaptureAreaAsync(int x, int y, int width, int height);
        Task SaveScreenshotAsync(byte[] imageData, string filePath, ImageFormat format);
    }
}