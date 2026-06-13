namespace TypingTrainer.Models;

/// <summary>
/// Represents a single typing game session with real-time tracking.
/// </summary>
public class GameSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DictionaryId { get; set; } = string.Empty;
    public string DictionaryName { get; set; } = string.Empty;
    public string TargetText { get; set; } = string.Empty;
    public string TypedText { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalCharacters => TargetText.Length;
    public int TypedCharacters => TypedText.Length;
    public int CorrectCharacters { get; set; }
    public int ErrorCount { get; set; }
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Elapsed time in seconds.
    /// </summary>
    public double ElapsedSeconds
    {
        get
        {
            if (EndTime.HasValue)
                return (EndTime.Value - StartTime).TotalSeconds;
            return (DateTime.Now - StartTime).TotalSeconds;
        }
    }

    /// <summary>
    /// Characters per minute.
    /// </summary>
    public double CPM
    {
        get
        {
            var seconds = ElapsedSeconds;
            if (seconds <= 0) return 0;
            return CorrectCharacters / seconds * 60;
        }
    }

    /// <summary>
    /// Words per minute (assuming average word length of 5 characters).
    /// </summary>
    public double WPM => CPM / 5.0;

    /// <summary>
    /// Accuracy percentage.
    /// </summary>
    public double Accuracy
    {
        get
        {
            if (TypedCharacters == 0) return 100;
            return (double)CorrectCharacters / TypedCharacters * 100;
        }
    }
}
