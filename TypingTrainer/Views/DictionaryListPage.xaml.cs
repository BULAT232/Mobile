using TypingTrainer.ViewModels;

namespace TypingTrainer.Views;

public partial class DictionaryListPage : ContentPage
{
    private readonly DictionaryListViewModel _viewModel;

    public DictionaryListPage(DictionaryListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDictionariesCommand.ExecuteAsync(null);
    }
}
