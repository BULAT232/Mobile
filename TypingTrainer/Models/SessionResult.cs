namespace TypingTrainer.Models;

/// <summary>
/// Stores the result of a completed typing session for statistics.
/// </summary>
public class SessionResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DictionaryId { get; set; } = string.Empty;
    public string DictionaryName { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public double WPM { get; set; }
    public double CPM { get; set; }
    public double Accuracy { get; set; }
    public int TotalCharacters { get; set; }
    public int CorrectCharacters { get; set; }
    public int ErrorCount { get; set; }
    public double DurationSeconds { get; set; }
}
