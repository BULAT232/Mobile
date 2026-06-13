using Newtonsoft.Json;
using CheckersGame.Models;

namespace CheckersGame.Services;

/// <summary>
/// Сервис управления профилями игроков.
/// </summary>
public class ProfileService
{
    private const string ProfilesFileName = "profiles.json";
    private List<PlayerProfile>? _profiles;

    private string FilePath =>
        Path.Combine(FileSystem.AppDataDirectory, ProfilesFileName);

    /// <summary>
    /// Получает все профили.
    /// </summary>
    public async Task<List<PlayerProfile>> GetProfilesAsync()
    {
        if (_profiles == null)
            await LoadProfilesAsync();
        return _profiles!;
    }

    /// <summary>
    /// Получает профиль по ID.
    /// </summary>
    public async Task<PlayerProfile?> GetProfileByIdAsync(string id)
    {
        var profiles = await GetProfilesAsync();
        return profiles.FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    /// Создаёт новый профиль.
    /// </summary>
    public async Task<PlayerProfile> CreateProfileAsync(string name, string avatarColor = "#512BD4")
    {
        var profiles = await GetProfilesAsync();
        var profile = new PlayerProfile
        {
            Name = name,
            AvatarColor = avatarColor
        };
        profiles.Add(profile);
        await SaveProfilesAsync();
        return profile;
    }

    /// <summary>
    /// Обновляет профиль.
    /// </summary>
    public async Task UpdateProfileAsync(PlayerProfile profile)
    {
        var profiles = await GetProfilesAsync();
        var index = profiles.FindIndex(p => p.Id == profile.Id);
        if (index >= 0)
        {
            profiles[index] = profile;
            await SaveProfilesAsync();
        }
    }

    /// <summary>
    /// Удаляет профиль.
    /// </summary>
    public async Task DeleteProfileAsync(string id)
    {
        var profiles = await GetProfilesAsync();
        profiles.RemoveAll(p => p.Id == id);
        await SaveProfilesAsync();
    }

    /// <summary>
    /// Обновляет рейтинг после игры (ELO-подобная система).
    /// </summary>
    public async Task UpdateRatingsAsync(string winnerId, string loserId, bool isDraw = false)
    {
        var winner = await GetProfileByIdAsync(winnerId);
        var loser = await GetProfileByIdAsync(loserId);

        if (winner == null || loser == null) return;

        const int K = 16;

        double expectedWinner = 1.0 / (1.0 + Math.Pow(10, (loser.Rating - winner.Rating) / 400.0));
        double expectedLoser = 1.0 / (1.0 + Math.Pow(10, (winner.Rating - loser.Rating) / 400.0));

        if (isDraw)
        {
            winner.Rating += (int)Math.Round(K * (0.5 - expectedWinner));
            loser.Rating += (int)Math.Round(K * (0.5 - expectedLoser));
            winner.Draws++;
            loser.Draws++;
            winner.CurrentStreak = 0;
            loser.CurrentStreak = 0;
        }
        else
        {
            winner.Rating += (int)Math.Round(K * (1.0 - expectedWinner));
            loser.Rating += (int)Math.Round(K * (0.0 - expectedLoser));
            winner.Wins++;
            loser.Losses++;
            winner.CurrentStreak++;
            loser.CurrentStreak = 0;

            if (winner.CurrentStreak > winner.BestStreak)
                winner.BestStreak = winner.CurrentStreak;
        }

        winner.GamesPlayed++;
        loser.GamesPlayed++;

        // Рейтинг не может быть ниже 100
        winner.Rating = Math.Max(100, winner.Rating);
        loser.Rating = Math.Max(100, loser.Rating);

        await UpdateProfileAsync(winner);
        await UpdateProfileAsync(loser);
    }

    /// <summary>
    /// Обновляет количество захваченных шашек.
    /// </summary>
    public async Task UpdateCapturedCountAsync(string playerId, int captured)
    {
        var profile = await GetProfileByIdAsync(playerId);
        if (profile != null)
        {
            profile.TotalCaptured += captured;
            await UpdateProfileAsync(profile);
        }
    }

    private async Task SaveProfilesAsync()
    {
        var json = JsonConvert.SerializeObject(_profiles, Formatting.Indented);
        await File.WriteAllTextAsync(FilePath, json);
    }

    private async Task LoadProfilesAsync()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = await File.ReadAllTextAsync(FilePath);
                _profiles = JsonConvert.DeserializeObject<List<PlayerProfile>>(json) ?? new List<PlayerProfile>();
            }
            else
            {
                _profiles = new List<PlayerProfile>();
            }
        }
        catch
        {
            _profiles = new List<PlayerProfile>();
        }
    }
}
