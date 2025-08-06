using ScreenshotClient.Domain.Interfaces;
using Avalonia.Controls.Notifications;
using ScreenshotClient.Interfaces;
using ScreenshotClient.Models;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Controls;
using System;

namespace ScreenshotClient.AppServices
{
    public class ToastService : IToastService
    {
        private readonly IConfigService _configService;
        private WindowNotificationManager? _notificationManager;

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
                EnsureNotificationManagerInitialized();
                
                var config = _configService.GetConfig();
                var notificationType = ConvertToNotificationType(type);
                var displayTime = TimeSpan.FromMilliseconds(config.ToastDisplayTime);

                _notificationManager?.Show(new Notification(
                    title: GetNotificationTitle(type),
                    message: message,
                    type: notificationType,
                    expiration: displayTime));
            });
        }

        private void EnsureNotificationManagerInitialized()
        {
            if (_notificationManager != null)
                return;

            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var config = _configService.GetConfig();
                var position = ConvertToNotificationPosition(config.ToastPosition);
                
                _notificationManager = new WindowNotificationManager(mainWindow)
                {
                    Position = position,
                    MaxItems = 3
                };
            }
        }

        private static Window? GetMainWindow()
        {
            return Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow 
                : null;
        }

        private static NotificationType ConvertToNotificationType(ToastType type)
        {
            return type switch
            {
                ToastType.Success => NotificationType.Success,
                ToastType.Warning => NotificationType.Warning,
                ToastType.Error => NotificationType.Error,
                _ => NotificationType.Information
            };
        }

        private static NotificationPosition ConvertToNotificationPosition(ToastPosition position)
        {
            return position switch
            {
                ToastPosition.TopLeft => NotificationPosition.TopLeft,
                ToastPosition.TopRight => NotificationPosition.TopRight,
                ToastPosition.TopCenter => NotificationPosition.TopCenter,
                ToastPosition.BottomLeft => NotificationPosition.BottomLeft,
                ToastPosition.BottomRight => NotificationPosition.BottomRight,
                ToastPosition.BottomCenter => NotificationPosition.BottomCenter,
                _ => NotificationPosition.TopRight
            };
        }

        private static string GetNotificationTitle(ToastType type)
        {
            return type switch
            {
                ToastType.Success => "Success",
                ToastType.Warning => "Warning",
                ToastType.Error => "Error",
                _ => "Information"
            };
        }
    }
}