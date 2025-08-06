using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia;
using Application = Avalonia.Application;
using ScreenshotClient.Interfaces;
using ScreenshotClient.AppServices;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Data.Core.Plugins;
using ScreenshotClient.Domain.Interfaces;

namespace ScreenshotClient;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);

        var collection = new ServiceCollection();
        collection.InjectServices();

        var serviceProvider = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = serviceProvider.GetService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

public static class ServiceCollectionExtensions
{
    public static void InjectServices(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddSingleton<IScreenshotService, ScreenshotService>();
        services.AddSingleton<IToastService, ToastService>();
        services.AddTransient<IHttpApiService, HttpApiService>();
        services.AddTransient<MainWindow>();
    }
}