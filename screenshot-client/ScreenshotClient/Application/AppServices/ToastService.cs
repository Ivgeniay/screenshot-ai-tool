using ScreenshotClient.Domain.Interfaces;
using ScreenshotClient.Views.Toasts;
using ScreenshotClient.Interfaces;
using System.Collections.Generic;
using ScreenshotClient.Models;
using System.Threading.Tasks;
using Avalonia.Threading;
using System;
using System.Linq;

namespace ScreenshotClient.AppServices
{
    public class ToastService : IToastService
    {
        private readonly IConfigService _configService;
        private readonly List<NotificationToast> _activeToasts = new();

        public ToastService(IConfigService configService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        public async Task ShowToastAsync(string message, ToastType type = ToastType.Info)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var config = _configService.GetConfig();
                var title = GetNotificationTitle(type);

                var toast = new NotificationToast(
                    title: title,
                    message: message,
                    type: type,
                    displayTimeMs: config.ToastDisplayTime,
                    position: config.ToastPosition
                );

                toast.Closed += OnToastClosed;
                
                _activeToasts.Add(toast);
                AdjustToastPositions();
                
                toast.Show();
            });
        }

        private void OnToastClosed(object? sender, EventArgs e)
        {
            if (sender is NotificationToast toast)
            {
                toast.Closed -= OnToastClosed;
                _activeToasts.Remove(toast);
                AdjustToastPositions();
            }
        }

        private void AdjustToastPositions()
        {
            if (_activeToasts.Count <= 1)
                return;

            if (_activeToasts.Count == 0)
                return;

            var config = _configService.GetConfig();
            var firstToast = _activeToasts[0];
            var screens = firstToast.Screens;
            var screen = screens?.Primary ?? screens?.All.FirstOrDefault();
            
            if (screen == null)
                return;
                
            var workingArea = screen.WorkingArea;
            const int margin = 20;
            const int toastSpacing = 10;

            for (int i = 0; i < _activeToasts.Count; i++)
            {
                var toast = _activeToasts[i];
                var offset = i * (toast.Height + toastSpacing);

                var x = config.ToastPosition switch
                {
                    ToastPosition.TopLeft or ToastPosition.BottomLeft => workingArea.X + margin,
                    ToastPosition.TopCenter or ToastPosition.BottomCenter => workingArea.X + (workingArea.Width - toast.Width) / 2,
                    ToastPosition.TopRight or ToastPosition.BottomRight => workingArea.X + workingArea.Width - toast.Width - margin,
                    _ => workingArea.X + workingArea.Width - toast.Width - margin
                };

                var y = config.ToastPosition switch
                {
                    ToastPosition.TopLeft or ToastPosition.TopCenter or ToastPosition.TopRight => 
                        workingArea.Y + margin + offset,
                    ToastPosition.BottomLeft or ToastPosition.BottomCenter or ToastPosition.BottomRight => 
                        workingArea.Y + workingArea.Height - toast.Height - margin - offset,
                    _ => workingArea.Y + margin + offset
                };

                toast.Position = new Avalonia.PixelPoint((int)x, (int)y);
            }
        }
        
        private static string GetNotificationTitle(ToastType type)
        {
            return type switch
            {
                ToastType.Success => "Успех",
                ToastType.Warning => "Предупреждение",
                ToastType.Error => "Ошибка",
                _ => "Информация"
            };
        }

        public void CloseAllToasts()
        {
            var toastsToClose = new List<NotificationToast>(_activeToasts);
            
            foreach (var toast in toastsToClose)
            {
                toast.Close();
            }
            
            _activeToasts.Clear();
        }

        public int GetActiveToastCount()
        {
            return _activeToasts.Count;
        }
    }
}