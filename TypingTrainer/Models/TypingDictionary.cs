namespace TypingTrainer.Models;

/// <summary>
/// Represents a dictionary (word list) for typing practice.
/// </summary>
public class TypingDictionary
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Language { get; set; } = "ru";
    public List<string> Words { get; set; } = new();
    public bool IsBuiltIn { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets a random subset of words from the dictionary.
    /// </summary>
    public List<string> GetRandomWords(int count)
    {
        var random = new Random();
        return Words.OrderBy(_ => random.Next()).Take(Math.Min(count, Words.Count)).ToList();
    }

    /// <summary>
    /// Gets the text for a typing session by joining random words.
    /// </summary>
    public string GenerateText(int wordCount = 20)
    {
        var words = GetRandomWords(wordCount);
        return string.Join(" ", words);
    }
}
