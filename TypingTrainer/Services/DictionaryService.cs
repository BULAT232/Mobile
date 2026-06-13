using Newtonsoft.Json;
using TypingTrainer.Models;

namespace TypingTrainer.Services;

/// <summary>
/// Service for managing typing dictionaries (word lists).
/// Handles loading, saving, creating, editing, and deleting dictionaries.
/// </summary>
public class DictionaryService
{
    private const string DictionariesFileName = "dictionaries.json";
    private List<TypingDictionary>? _dictionaries;

    private string FilePath =>
        Path.Combine(FileSystem.AppDataDirectory, DictionariesFileName);

    /// <summary>
    /// Gets all available dictionaries.
    /// </summary>
    public async Task<List<TypingDictionary>> GetAllDictionariesAsync()
    {
        if (_dictionaries == null)
        {
            await LoadDictionariesAsync();
        }
        return _dictionaries!;
    }

    /// <summary>
    /// Gets a dictionary by its ID.
    /// </summary>
    public async Task<TypingDictionary?> GetDictionaryByIdAsync(string id)
    {
        var dictionaries = await GetAllDictionariesAsync();
        return dictionaries.FirstOrDefault(d => d.Id == id);
    }

    /// <summary>
    /// Saves or updates a dictionary.
    /// </summary>
    public async Task SaveDictionaryAsync(TypingDictionary dictionary)
    {
        var dictionaries = await GetAllDictionariesAsync();
        var existing = dictionaries.FirstOrDefault(d => d.Id == dictionary.Id);

        if (existing != null)
        {
            dictionaries.Remove(existing);
        }

        dictionary.UpdatedAt = DateTime.Now;
        dictionaries.Add(dictionary);
        await SaveDictionariesAsync();
    }

    /// <summary>
    /// Deletes a dictionary by ID.
    /// </summary>
    public async Task DeleteDictionaryAsync(string id)
    {
        var dictionaries = await GetAllDictionariesAsync();
        var dictionary = dictionaries.FirstOrDefault(d => d.Id == id);

        if (dictionary != null && !dictionary.IsBuiltIn)
        {
            dictionaries.Remove(dictionary);
            await SaveDictionariesAsync();
        }
    }

    /// <summary>
    /// Loads dictionaries from file, creating defaults if none exist.
    /// </summary>
    private async Task LoadDictionariesAsync()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = await File.ReadAllTextAsync(FilePath);
                _dictionaries = JsonConvert.DeserializeObject<List<TypingDictionary>>(json) ?? new List<TypingDictionary>();
            }
            else
            {
                _dictionaries = CreateDefaultDictionaries();
                await SaveDictionariesAsync();
            }
        }
        catch
        {
            _dictionaries = CreateDefaultDictionaries();
            await SaveDictionariesAsync();
        }
    }

    /// <summary>
    /// Saves all dictionaries to file.
    /// </summary>
    private async Task SaveDictionariesAsync()
    {
        var json = JsonConvert.SerializeObject(_dictionaries, Formatting.Indented);
        await File.WriteAllTextAsync(FilePath, json);
    }

    /// <summary>
    /// Creates default built-in dictionaries.
    /// </summary>
    private List<TypingDictionary> CreateDefaultDictionaries()
    {
        return new List<TypingDictionary>
        {
            new TypingDictionary
            {
                Id = "ru-basic",
                Name = "Русский (базовый)",
                Description = "Основные русские слова для начинающих",
                Language = "ru",
                IsBuiltIn = true,
                Words = new List<string>
                {
                    "привет", "мир", "дом", "кот", "собака", "книга", "стол", "окно",
                    "дверь", "вода", "огонь", "земля", "небо", "солнце", "луна", "звезда",
                    "дерево", "цветок", "река", "море", "гора", "лес", "поле", "город",
                    "улица", "школа", "работа", "друг", "семья", "время", "день", "ночь",
                    "утро", "вечер", "год", "месяц", "неделя", "час", "минута", "секунда",
                    "человек", "ребёнок", "мужчина", "женщина", "мальчик", "девочка",
                    "хлеб", "молоко", "мясо", "рыба", "овощи", "фрукты", "сахар", "соль",
                    "чай", "кофе", "сок", "масло", "яйцо", "сыр", "каша", "суп",
                    "красный", "синий", "зелёный", "жёлтый", "белый", "чёрный",
                    "большой", "маленький", "новый", "старый", "хороший", "плохой",
                    "быстро", "медленно", "далеко", "близко", "высоко", "низко"
                }
            },
            new TypingDictionary
            {
                Id = "ru-advanced",
                Name = "Русский (продвинутый)",
                Description = "Сложные русские слова для продвинутых",
                Language = "ru",
                IsBuiltIn = true,
                Words = new List<string>
                {
                    "программирование", "компьютер", "клавиатура", "монитор", "процессор",
                    "алгоритм", "переменная", "функция", "библиотека", "интерфейс",
                    "архитектура", "производительность", "оптимизация", "документация",
                    "тестирование", "развёртывание", "конфигурация", "аутентификация",
                    "авторизация", "шифрование", "безопасность", "масштабируемость",
                    "совместимость", "доступность", "надёжность", "эффективность",
                    "инфраструктура", "виртуализация", "контейнеризация", "микросервис",
                    "приложение", "разработка", "проектирование", "моделирование",
                    "визуализация", "автоматизация", "интеграция", "миграция",
                    "сериализация", "десериализация", "валидация", "верификация",
                    "аналитика", "статистика", "диагностика", "мониторинг",
                    "уведомление", "синхронизация", "резервирование", "восстановление"
                }
            },
            new TypingDictionary
            {
                Id = "en-basic",
                Name = "English (Basic)",
                Description = "Common English words for beginners",
                Language = "en",
                IsBuiltIn = true,
                Words = new List<string>
                {
                    "the", "be", "to", "of", "and", "a", "in", "that", "have", "it",
                    "for", "not", "on", "with", "he", "as", "you", "do", "at", "this",
                    "but", "his", "by", "from", "they", "we", "say", "her", "she", "or",
                    "an", "will", "my", "one", "all", "would", "there", "their", "what",
                    "so", "up", "out", "if", "about", "who", "get", "which", "go", "me",
                    "when", "make", "can", "like", "time", "no", "just", "him", "know",
                    "take", "people", "into", "year", "your", "good", "some", "could",
                    "them", "see", "other", "than", "then", "now", "look", "only", "come",
                    "its", "over", "think", "also", "back", "after", "use", "two", "how",
                    "our", "work", "first", "well", "way", "even", "new", "want", "because",
                    "any", "these", "give", "day", "most", "find", "here", "thing", "many"
                }
            },
            new TypingDictionary
            {
                Id = "en-programming",
                Name = "Programming Terms",
                Description = "Common programming terms and keywords",
                Language = "en",
                IsBuiltIn = true,
                Words = new List<string>
                {
                    "function", "variable", "class", "object", "method", "property",
                    "interface", "abstract", "static", "public", "private", "protected",
                    "return", "void", "string", "integer", "boolean", "array", "list",
                    "dictionary", "exception", "try", "catch", "finally", "throw",
                    "async", "await", "task", "thread", "lock", "event", "delegate",
                    "lambda", "expression", "statement", "condition", "loop", "while",
                    "foreach", "switch", "case", "break", "continue", "default",
                    "namespace", "import", "export", "module", "package", "library",
                    "framework", "database", "query", "table", "column", "index",
                    "server", "client", "request", "response", "endpoint", "protocol",
                    "algorithm", "structure", "pattern", "design", "architecture",
                    "testing", "debug", "deploy", "build", "compile", "runtime"
                }
            }
        };
    }
}
