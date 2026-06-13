using CheckersGame.ViewModels;

namespace CheckersGame.Views;

public partial class GameSetupPage : ContentPage
{
    private readonly GameSetupViewModel _viewModel;

    public GameSetupPage(GameSetupViewModel viewModel)
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
