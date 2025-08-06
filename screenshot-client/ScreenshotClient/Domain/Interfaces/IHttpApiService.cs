using ScreenshotCommonLib.Models;
using System.Threading.Tasks;

namespace ScreenshotClient.Domain.Interfaces
{
    public interface IHttpApiService
    {
        Task<AnalyzeImageResponse> AnalyzeImageAsync(byte[] imageData, string prompt);
    }
}