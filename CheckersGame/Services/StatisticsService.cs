using Newtonsoft.Json;
using CheckersGame.Models;

namespace CheckersGame.Services;

/// <summary>
/// Сервис статистики и истории игр.
/// </summary>
public class StatisticsService
{
    private const string RecordsFileName = "game_records.json";
    private List<GameRecord>? _records;

    private string FilePath =>
        Path.Combine(FileSystem.AppDataDirectory, RecordsFileName);

    /// <summary>
    /// Получает все записи игр.
    /// </summary>
    public async Task<List<GameRecord>> GetAllRecordsAsync()
    {
        if (_records == null)
            await LoadRecordsAsync();
        return _records!;
    }

    /// <summary>
    /// Добавляет запись об игре.
    /// </summary>
    public async Task AddRecordAsync(GameRecord record)
    {
        var records = await GetAllRecordsAsync();
        records.Add(record);
        await SaveRecordsAsync();
    }

    /// <summary>
    /// Получает записи для конкретного игрока.
    /// </summary>
    public async Task<List<GameRecord>> GetRecordsForPlayerAsync(string playerId)
    {
        var records = await GetAllRecordsAsync();
        return records.Where(r => r.Player1Id == playerId || r.Player2Id == playerId)
                      .OrderByDescending(r => r.Date)
                      .ToList();
    }

    /// <summary>
    /// Получает записи по типу игры.
    /// </summary>
    public async Task<List<GameRecord>> GetRecordsByGameTypeAsync(GameType gameType)
    {
        var records = await GetAllRecordsAsync();
        return records.Where(r => r.GameType == gameType)
                      .OrderByDescending(r => r.Date)
                      .ToList();
    }

    /// <summary>
    /// Получает общую статистику.
    /// </summary>
    public async Task<OverallStats> GetOverallStatsAsync()
    {
        var records = await GetAllRecordsAsync();
        return new OverallStats
        {
            TotalGames = records.Count,
            TotalDuration = TimeSpan.FromTicks(records.Sum(r => r.Duration.Ticks)),
            AverageDuration = records.Count > 0
                ? TimeSpan.FromTicks((long)records.Average(r => r.Duration.Ticks))
                : TimeSpan.Zero,
            TotalMoves = records.Sum(r => r.MoveCount),
            GamesByType = records.GroupBy(r => r.GameType)
                                 .ToDictionary(g => g.Key, g => g.Count()),
            DrawCount = records.Count(r => r.Status == GameStatus.Draw)
        };
    }

    /// <summary>
    /// Получает статистику для игрока.
    /// </summary>
    public async Task<PlayerStats> GetPlayerStatsAsync(string playerId)
    {
        var records = await GetRecordsForPlayerAsync(playerId);
        var wins = records.Count(r => r.WinnerId == playerId);
        var losses = records.Count(r => r.WinnerId != null && r.WinnerId != playerId && r.Status != GameStatus.Draw);
        var draws = records.Count(r => r.Status == GameStatus.Draw);

        return new PlayerStats
        {
            TotalGames = records.Count,
            Wins = wins,
            Losses = losses,
            Draws = draws,
            TotalCaptured = records.Sum(r =>
                r.Player1Id == playerId ? r.Player1Captured : r.Player2Captured),
            AverageDuration = records.Count > 0
                ? TimeSpan.FromTicks((long)records.Average(r => r.Duration.Ticks))
                : TimeSpan.Zero,
            GamesByType = records.GroupBy(r => r.GameType)
                                 .ToDictionary(g => g.Key, g => g.Count()),
            RecentGames = records.Take(10).ToList()
        };
    }

    /// <summary>
    /// Удаляет все записи.
    /// </summary>
    public async Task ClearAllRecordsAsync()
    {
        _records = new List<GameRecord>();
        await SaveRecordsAsync();
    }

    private async Task SaveRecordsAsync()
    {
        var json = JsonConvert.SerializeObject(_records, Formatting.Indented);
        await File.WriteAllTextAsync(FilePath, json);
    }

    private async Task LoadRecordsAsync()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = await File.ReadAllTextAsync(FilePath);
                _records = JsonConvert.DeserializeObject<List<GameRecord>>(json) ?? new List<GameRecord>();
            }
            else
            {
                _records = new List<GameRecord>();
            }
        }
        catch
        {
            _records = new List<GameRecord>();
        }
    }
}

/// <summary>
/// Общая статистика.
/// </summary>
public class OverallStats
{
    public int TotalGames { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public int TotalMoves { get; set; }
    public Dictionary<GameType, int> GamesByType { get; set; } = new();
    public int DrawCount { get; set; }
}

/// <summary>
/// Статистика игрока.
/// </summary>
public class PlayerStats
{
    public int TotalGames { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int TotalCaptured { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public Dictionary<GameType, int> GamesByType { get; set; } = new();
    public List<GameRecord> RecentGames { get; set; } = new();
    public double WinRate => TotalGames > 0 ? (double)Wins / TotalGames * 100 : 0;
}
