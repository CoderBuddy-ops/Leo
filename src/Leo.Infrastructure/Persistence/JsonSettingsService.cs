using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Leo.Application.Interfaces;

namespace Leo.Infrastructure.Persistence;

public class JsonSettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private AppSettings _currentSettings = new();

    public AppSettings CurrentSettings => _currentSettings;

    public JsonSettingsService()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, "Leo");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        _settingsFilePath = Path.Combine(folder, "settings.json");
    }

    public void LoadSettings()
    {
        if (File.Exists(_settingsFilePath))
        {
            try
            {
                string json = File.ReadAllText(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    _currentSettings = loaded;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings.json: {ex.Message}");
            }
        }
    }

    public async Task LoadSettingsAsync()
    {
        LoadSettings();
        await Task.CompletedTask;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _currentSettings = settings;
        try
        {
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings.json: {ex.Message}");
        }
    }
}
