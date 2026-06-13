using Newtonsoft.Json;
using CheckersGame.Models;

namespace CheckersGame.Services;

/// <summary>
/// Сервис управления настройками приложения.
/// </summary>
public class SettingsService
{
    private const string SettingsFileName = "settings.json";
    private AppSettings? _settings;

    private string FilePath =>
        Path.Combine(FileSystem.AppDataDirectory, SettingsFileName);

    /// <summary>
    /// Получает текущие настройки.
    /// </summary>
    public async Task<AppSettings> GetSettingsAsync()
    {
        if (_settings == null)
            await LoadSettingsAsync();
        return _settings!;
    }

    /// <summary>
    /// Сохраняет настройки.
    /// </summary>
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _settings = settings;
        var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
        await File.WriteAllTextAsync(FilePath, json);
    }

    /// <summary>
    /// Сбрасывает настройки по умолчанию.
    /// </summary>
    public async Task ResetToDefaultsAsync()
    {
        _settings = new AppSettings();
        await SaveSettingsAsync(_settings);
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = await File.ReadAllTextAsync(FilePath);
                _settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            else
            {
                _settings = new AppSettings();
            }
        }
        catch
        {
            _settings = new AppSettings();
        }
    }
}
