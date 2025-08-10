using ScreenshotClient.Models;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia;
using System;
using System.Linq;
using Avalonia.Media;

namespace ScreenshotClient.Views.Toasts
{
    public partial class NotificationToast : Window
    {
        private readonly int _displayTime;

        public NotificationToast(string title, string message, ToastType type, int displayTimeMs, ToastPosition position)
        {
            InitializeComponent();
            
            _displayTime = displayTimeMs;

            SetupToast(title, message, type);
            SetupPosition(position);
            SetupAnimations();
        }

        private void SetupToast(string title, string message, ToastType type)
        {
            ToastTitle.Text = title;
            ToastMessage.Text = message;

            var typeClass = type.ToString().ToLower();
            ToastContainer.Classes.Add(typeClass);
            ToastIcon.Classes.Add(typeClass);

            //ToastIcon.Data = GetIconPath(type);
        }

        private void SetupPosition(ToastPosition position)
        {
            var screens = Screens;
            var screen = screens?.Primary ?? screens?.All.FirstOrDefault();
            if (screen == null) return;

            var workingArea = screen.WorkingArea;
            const int margin = 20;

            var x = position switch
            {
                ToastPosition.TopLeft or ToastPosition.BottomLeft => workingArea.X + margin,
                ToastPosition.TopCenter or ToastPosition.BottomCenter => workingArea.X + (workingArea.Width - Width) / 2,
                ToastPosition.TopRight or ToastPosition.BottomRight => workingArea.X + workingArea.Width - Width - margin,
                _ => workingArea.X + workingArea.Width - Width - margin
            };

            var y = position switch
            {
                ToastPosition.TopLeft or ToastPosition.TopCenter or ToastPosition.TopRight => workingArea.Y + margin,
                ToastPosition.BottomLeft or ToastPosition.BottomCenter or ToastPosition.BottomRight => workingArea.Y + workingArea.Height - Height - margin,
                _ => workingArea.Y + margin
            };

            Position = new PixelPoint((int)x, (int)y);
        }

        private void SetupAnimations()
        {
            Opacity = 0;
            RenderTransform = new Avalonia.Media.TranslateTransform(0, -20);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            
            _ = Task.Run(async () =>
            {
                await ShowAnimation();
                await Task.Delay(_displayTime);
                await HideAnimation();
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Close();
                });
            });
        }

        private async Task ShowAnimation()
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                // Создаем композитную анимацию для окна
                var showAnimation = new Animation
                {
                    Duration = TimeSpan.FromMilliseconds(300),
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(0.0),
                            Setters = { 
                                new Setter(OpacityProperty, 0.0),
                                new Setter(TranslateTransform.YProperty, -20.0)
                            }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(1.0),
                            Setters = { 
                                new Setter(OpacityProperty, 1.0),
                                new Setter(TranslateTransform.YProperty, 0.0)
                            }
                        }
                    }
                };

                await showAnimation.RunAsync(this);
            });
        }

        private async Task HideAnimation()
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var hideAnimation = new Animation
                {
                    Duration = TimeSpan.FromMilliseconds(200),
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(0.0),
                            Setters = { 
                                new Setter(OpacityProperty, 1.0),
                                new Setter(TranslateTransform.YProperty, 0.0)
                            }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(1.0),
                            Setters = { 
                                new Setter(OpacityProperty, 0.0),
                                new Setter(TranslateTransform.YProperty, -10.0)
                            }
                        }
                    }
                };

                await hideAnimation.RunAsync(this);
            });
        }

        private void OnCloseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private static string GetIconPath(ToastType type)
        {
            return type switch
            {
                ToastType.Success => "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M11,16.5L18,9.5L16.59,8.09L11,13.67L7.91,10.59L6.5,12L11,16.5Z",
                ToastType.Warning => "M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z",
                ToastType.Error => "M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z",
                _ => "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,17A1.5,1.5 0 0,0 13.5,15.5A1.5,1.5 0 0,0 12,14A1.5,1.5 0 0,0 10.5,15.5A1.5,1.5 0 0,0 12,17M12,7A1,1 0 0,0 11,8V12A1,1 0 0,0 12,13A1,1 0 0,0 13,12V8A1,1 0 0,0 12,7Z"
            };
        }
    }

}