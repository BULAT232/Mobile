using CheckersGame.ViewModels;

namespace CheckersGame.Views;

public partial class GameHistoryPage : ContentPage
{
    private readonly GameHistoryViewModel _viewModel;

    public GameHistoryPage(GameHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadHistoryCommand.ExecuteAsync(null);
    }
}
