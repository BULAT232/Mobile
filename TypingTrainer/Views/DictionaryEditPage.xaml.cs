using TypingTrainer.ViewModels;

namespace TypingTrainer.Views;

public partial class DictionaryEditPage : ContentPage
{
    private readonly DictionaryEditViewModel _viewModel;

    public DictionaryEditPage(DictionaryEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDictionaryCommand.ExecuteAsync(null);
    }
}
