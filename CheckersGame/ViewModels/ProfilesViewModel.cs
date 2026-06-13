using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CheckersGame.Models;
using CheckersGame.Services;

namespace CheckersGame.ViewModels;

/// <summary>
/// ViewModel страницы профилей.
/// </summary>
public partial class ProfilesViewModel : ObservableObject
{
    private readonly ProfileService _profileService;

    [ObservableProperty]
    private ObservableCollection<PlayerProfile> _profiles = new();

    [ObservableProperty]
    private bool _isEmpty = true;

    public List<string> AvatarColors { get; } = new()
    {
        "#512BD4", "#F44336", "#4CAF50", "#2196F3",
        "#FF9800", "#9C27B0", "#00BCD4", "#795548"
    };

    public ProfilesViewModel(ProfileService profileService)
    {
        _profileService = profileService;
    }

    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        var profiles = await _profileService.GetProfilesAsync();
        Profiles = new ObservableCollection<PlayerProfile>(profiles);
        IsEmpty = Profiles.Count == 0;
    }

    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        string? name = await Shell.Current.DisplayPromptAsync(
            "Новый профиль", "Введите имя игрока:", "Создать", "Отмена",
            placeholder: "Имя");

        if (string.IsNullOrWhiteSpace(name)) return;

        string color = AvatarColors[Profiles.Count % AvatarColors.Count];
        var profile = await _profileService.CreateProfileAsync(name, color);
        Profiles.Add(profile);
        IsEmpty = false;
    }

    [RelayCommand]
    private async Task EditProfileAsync(PlayerProfile profile)
    {
        string? newName = await Shell.Current.DisplayPromptAsync(
            "Редактировать профиль", "Введите новое имя:",
            "Сохранить", "Отмена",
            initialValue: profile.Name);

        if (string.IsNullOrWhiteSpace(newName)) return;

        profile.Name = newName;
        await _profileService.UpdateProfileAsync(profile);

        // Обновляем список
        await LoadProfilesAsync();
    }

    [RelayCommand]
    private async Task DeleteProfileAsync(PlayerProfile profile)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Удалить профиль",
            $"Удалить профиль \"{profile.Name}\"? Это действие нельзя отменить.",
            "Удалить", "Отмена");

        if (!confirm) return;

        await _profileService.DeleteProfileAsync(profile.Id);
        Profiles.Remove(profile);
        IsEmpty = Profiles.Count == 0;
    }
}
