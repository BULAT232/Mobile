namespace TypingTrainer.Models;

/// <summary>
/// Application settings model.
/// </summary>
public class AppSettings
{
    public int FontSize { get; set; } = 20;
    public int WordCount { get; set; } = 20;
    public string FontFamily { get; set; } = "OpenSansRegular";
    public bool ShowTimer { get; set; } = true;
    public bool ShowWPM { get; set; } = true;
    public bool ShowAccuracy { get; set; } = true;
    public bool SoundEnabled { get; set; } = true;
    public bool HighlightErrors { get; set; } = true;
    public string SelectedDictionaryId { get; set; } = string.Empty;
}
