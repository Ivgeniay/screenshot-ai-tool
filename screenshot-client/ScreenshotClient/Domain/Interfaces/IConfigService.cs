using ScreenshotClient.Models;
using System.Threading.Tasks;

namespace ScreenshotClient.Interfaces
{
    public interface IConfigService
    {
        AppConfig GetConfig();
        Task<AppConfig> GetConfigAsync();
        void SaveConfig(AppConfig config);
        Task SaveConfigAsync(AppConfig config);
    }
}