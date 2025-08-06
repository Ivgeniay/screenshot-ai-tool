using ScreenshotClient.Interfaces;
using ScreenshotClient.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System;

namespace ScreenshotClient.AppServices
{
    public class ConfigService : IConfigService
{
    private readonly string _configFilePath;
    private AppConfig? _cachedConfig;
    private readonly object _lock = new object();

    public ConfigService()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _configFilePath = Path.Combine(appDirectory, "config.json");
    }

    public AppConfig GetConfig()
    {
        lock (_lock)
        {
            if (_cachedConfig != null)
                return _cachedConfig;

            try
            {
                if (!File.Exists(_configFilePath))
                {
                    _cachedConfig = CreateDefaultConfig();
                    SaveConfigInternal(_cachedConfig);
                    return _cachedConfig;
                }

                var json = File.ReadAllText(_configFilePath);
                _cachedConfig = JsonConvert.DeserializeObject<AppConfig>(json) ?? CreateDefaultConfig();
                return _cachedConfig;
            }
            catch (Exception)
            {
                _cachedConfig = CreateDefaultConfig();
                return _cachedConfig;
            }
        }
    }

    public async Task<AppConfig> GetConfigAsync()
    {
        return await Task.Run(() => GetConfig());
    }

    public void SaveConfig(AppConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        lock (_lock)
        {
            SaveConfigInternal(config);
            _cachedConfig = config;
        }
    }

    public async Task SaveConfigAsync(AppConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        await Task.Run(() => SaveConfig(config));
    }

    private void SaveConfigInternal(AppConfig config)
    {
        try
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Не удалось сохранить конфигурацию: {ex.Message}", ex);
        }
    }

    private static AppConfig CreateDefaultConfig()
    {
        return new AppConfig();
    }
}
}