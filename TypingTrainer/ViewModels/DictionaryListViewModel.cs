using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TypingTrainer.Models;
using TypingTrainer.Services;
using TypingTrainer.Views;

namespace TypingTrainer.ViewModels;

/// <summary>
/// ViewModel for the dictionary list page.
/// </summary>
public partial class DictionaryListViewModel : ObservableObject
{
    private readonly DictionaryService _dictionaryService;

    [ObservableProperty]
    private ObservableCollection<TypingDictionary> _dictionaries = new();

    [ObservableProperty]
    private bool _isLoading;

    public DictionaryListViewModel(DictionaryService dictionaryService)
    {
        _dictionaryService = dictionaryService;
    }

    [RelayCommand]
    private async Task LoadDictionariesAsync()
    {
        IsLoading = true;
        try
        {
            var dicts = await _dictionaryService.GetAllDictionariesAsync();
            Dictionaries = new ObservableCollection<TypingDictionary>(dicts);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddDictionaryAsync()
    {
        await Shell.Current.GoToAsync($"{nameof(DictionaryEditPage)}?dictionaryId=new");
    }

    [RelayCommand]
    private async Task EditDictionaryAsync(TypingDictionary dictionary)
    {
        if (dictionary == null) return;
        await Shell.Current.GoToAsync($"{nameof(DictionaryEditPage)}?dictionaryId={dictionary.Id}");
    }

    [RelayCommand]
    private async Task DeleteDictionaryAsync(TypingDictionary dictionary)
    {
        if (dictionary == null) return;

        if (dictionary.IsBuiltIn)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Нельзя удалить встроенный словарь", "OK");
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert(
            "Удаление",
            $"Вы уверены, что хотите удалить словарь \"{dictionary.Name}\"?",
            "Удалить",
            "Отмена");

        if (confirm)
        {
            await _dictionaryService.DeleteDictionaryAsync(dictionary.Id);
            await LoadDictionariesAsync();
        }
    }
}
