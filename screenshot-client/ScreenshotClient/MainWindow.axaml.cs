using Avalonia.Controls.ApplicationLifetimes;
using ScreenshotClient.Domain.Interfaces;
using ScreenshotClient.Interfaces;
using ScreenshotClient.Models;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia;
using System;

namespace ScreenshotClient
{
    public partial class MainWindow : Window
    {
        private readonly IConfigService _configService;
        private readonly IHotkeyService _hotkeyService;
        private readonly IToastService _toastService;
        private TrayIcon? _trayIcon;

        public MainWindow(
            IConfigService configService,
            IHotkeyService hotkeyService,
            IToastService toastService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
            _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));

            InitializeComponent();
            InitializeTrayIcon();
            InitializeHotkeys();

            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
            this.IsVisible = false;
        }

        private void InitializeTrayIcon()
        {
            _trayIcon = new TrayIcon();
            _trayIcon.Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://ScreenshotClient/Assets/icon.ico")));
            _trayIcon.ToolTipText = "Screenshot Client";
            _trayIcon.IsVisible = true;

            var menu = new NativeMenu();
            
            var showItem = new NativeMenuItem("Показать");
            showItem.Click += (_, _) => ShowWindow();
            
            var exitItem = new NativeMenuItem("Выход");
            exitItem.Click += (_, _) => ExitApplication();

            menu.Add(showItem);
            menu.Add(new NativeMenuItemSeparator());
            menu.Add(exitItem);

            _trayIcon.Menu = menu;
            _trayIcon.Clicked += (_, _) => ShowWindow();
        }

        private void InitializeHotkeys()
        {
            var config = _configService.GetConfig();
            _hotkeyService.RegisterHotkey(config.HotKey, OnScreenshotHotkey);
        }

        private async void OnScreenshotHotkey()
        {
            await _toastService.ShowToastAsync("Функция скриншота пока не реализована", ToastType.Info);
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            this.ShowInTaskbar = true;
        }

        private void HideWindow()
        {
            this.Hide();
            this.ShowInTaskbar = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            HideWindow();
            base.OnClosed(e);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            e.Cancel = true;
            HideWindow();
        }

        private void ExitApplication()
        {
            _hotkeyService.UnregisterAllHotkeys();
            _trayIcon?.Dispose();

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}