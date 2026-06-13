using CheckersGame.ViewModels;

namespace CheckersGame.Views;

public partial class MainMenuPage : ContentPage
{
    private readonly MainMenuViewModel _viewModel;

    public MainMenuPage(MainMenuViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
