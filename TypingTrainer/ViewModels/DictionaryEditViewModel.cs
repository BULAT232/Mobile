using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TypingTrainer.Models;
using TypingTrainer.Services;

namespace TypingTrainer.ViewModels;

/// <summary>
/// ViewModel for creating and editing dictionaries.
/// </summary>
[QueryProperty(nameof(DictionaryId), "dictionaryId")]
public partial class DictionaryEditViewModel : ObservableObject
{
    private readonly DictionaryService _dictionaryService;
    private bool _isNewDictionary;

    [ObservableProperty]
    private string _dictionaryId = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _language = "ru";

    [ObservableProperty]
    private string _wordsText = string.Empty;

    [ObservableProperty]
    private int _wordCount;

    [ObservableProperty]
    private bool _isBuiltIn;

    [ObservableProperty]
    private string _pageTitle = "Новый словарь";

    public DictionaryEditViewModel(DictionaryService dictionaryService)
    {
        _dictionaryService = dictionaryService;
    }

    [RelayCommand]
    private async Task LoadDictionaryAsync()
    {
        if (DictionaryId == "new")
        {
            _isNewDictionary = true;
            PageTitle = "Новый словарь";
            Name = string.Empty;
            Description = string.Empty;
            Language = "ru";
            WordsText = string.Empty;
            WordCount = 0;
            IsBuiltIn = false;
            return;
        }

        _isNewDictionary = false;
        var dictionary = await _dictionaryService.GetDictionaryByIdAsync(DictionaryId);
        if (dictionary == null)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Словарь не найден", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        PageTitle = $"Редактирование: {dictionary.Name}";
        Name = dictionary.Name;
        Description = dictionary.Description;
        Language = dictionary.Language;
        WordsText = string.Join("\n", dictionary.Words);
        WordCount = dictionary.Words.Count;
        IsBuiltIn = dictionary.IsBuiltIn;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Ошибка", "Введите название словаря", "OK");
            return;
        }

        var words = WordsText
            .Split(new[] { '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim())
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Distinct()
            .ToList();

        if (words.Count < 5)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Словарь должен содержать минимум 5 слов", "OK");
            return;
        }

        var dictionary = new TypingDictionary
        {
            Id = _isNewDictionary ? Guid.NewGuid().ToString() : DictionaryId,
            Name = Name.Trim(),
            Description = Description?.Trim() ?? string.Empty,
            Language = Language,
            Words = words,
            IsBuiltIn = false,
            CreatedAt = _isNewDictionary ? DateTime.Now : DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _dictionaryService.SaveDictionaryAsync(dictionary);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    partial void OnWordsTextChanged(string value)
    {
        var words = value
            .Split(new[] { '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim())
            .Where(w => !string.IsNullOrWhiteSpace(w));
        WordCount = words.Count();
    }
}
