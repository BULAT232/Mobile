using Newtonsoft.Json;
using TypingTrainer.Models;

namespace TypingTrainer.Services;

/// <summary>
/// Service for managing typing session statistics.
/// </summary>
public class StatisticsService
{
    private const string StatsFileName = "statistics.json";
    private List<SessionResult>? _results;

    private string FilePath =>
        Path.Combine(FileSystem.AppDataDirectory, StatsFileName);

    /// <summary>
    /// Gets all session results.
    /// </summary>
    public async Task<List<SessionResult>> GetAllResultsAsync()
    {
        if (_results == null)
        {
            await LoadResultsAsync();
        }
        return _results!;
    }

    /// <summary>
    /// Adds a new session result.
    /// </summary>
    public async Task AddResultAsync(SessionResult result)
    {
        var results = await GetAllResultsAsync();
        results.Add(result);
        await SaveResultsAsync();
    }

    /// <summary>
    /// Gets the best WPM across all sessions.
    /// </summary>
    public async Task<double> GetBestWPMAsync()
    {
        var results = await GetAllResultsAsync();
        return results.Count > 0 ? results.Max(r => r.WPM) : 0;
    }

    /// <summary>
    /// Gets the average WPM across all sessions.
    /// </summary>
    public async Task<double> GetAverageWPMAsync()
    {
        var results = await GetAllResultsAsync();
        return results.Count > 0 ? results.Average(r => r.WPM) : 0;
    }

    /// <summary>
    /// Gets the average accuracy across all sessions.
    /// </summary>
    public async Task<double> GetAverageAccuracyAsync()
    {
        var results = await GetAllResultsAsync();
        return results.Count > 0 ? results.Average(r => r.Accuracy) : 0;
    }

    /// <summary>
    /// Gets the total number of sessions played.
    /// </summary>
    public async Task<int> GetTotalSessionsAsync()
    {
        var results = await GetAllResultsAsync();
        return results.Count;
    }

    /// <summary>
    /// Gets the total practice time in minutes.
    /// </summary>
    public async Task<double> GetTotalPracticeTimeMinutesAsync()
    {
        var results = await GetAllResultsAsync();
        return results.Sum(r => r.DurationSeconds) / 60.0;
    }

    /// <summary>
    /// Gets the last N session results.
    /// </summary>
    public async Task<List<SessionResult>> GetRecentResultsAsync(int count = 10)
    {
        var results = await GetAllResultsAsync();
        return results.OrderByDescending(r => r.Date).Take(count).ToList();
    }

    /// <summary>
    /// Clears all statistics.
    /// </summary>
    public async Task ClearAllAsync()
    {
        _results = new List<SessionResult>();
        await SaveResultsAsync();
    }

    private async Task LoadResultsAsync()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = await File.ReadAllTextAsync(FilePath);
                _results = JsonConvert.DeserializeObject<List<SessionResult>>(json) ?? new List<SessionResult>();
            }
            else
            {
                _results = new List<SessionResult>();
            }
        }
        catch
        {
            _results = new List<SessionResult>();
        }
    }

    private async Task SaveResultsAsync()
    {
        var json = JsonConvert.SerializeObject(_results, Formatting.Indented);
        await File.WriteAllTextAsync(FilePath, json);
    }
}
