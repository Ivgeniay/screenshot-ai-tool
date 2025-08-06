using ScreenshotClient.Models;
using System.Threading.Tasks;

namespace ScreenshotClient.Domain.Interfaces
{
    public interface IToastService
    {
        Task ShowToastAsync(string message, ToastType type = ToastType.Info);
    }
}