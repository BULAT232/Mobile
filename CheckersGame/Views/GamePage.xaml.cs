using CheckersGame.Models;
using CheckersGame.ViewModels;

namespace CheckersGame.Views;

public partial class GamePage : ContentPage
{
    private readonly GameViewModel _viewModel;
    private int _currentBoardSize;

    public GamePage(GameViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeGameCommand.ExecuteAsync(null);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameViewModel.Cells) ||
            e.PropertyName == nameof(GameViewModel.BoardSize))
        {
            MainThread.BeginInvokeOnMainThread(RenderBoard);
        }
    }

    private void RenderBoard()
    {
        var boardSize = _viewModel.BoardSize;
        var cells = _viewModel.Cells;

        if (cells == null || cells.Count == 0) return;

        // Вычисляем размер клетки на основе ширины экрана
        double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        double cellSize = Math.Min(Math.Floor((screenWidth - 30) / boardSize), 45);

        // Пересоздаём сетку, если размер доски изменился
        if (_currentBoardSize != boardSize)
        {
            _currentBoardSize = boardSize;
            BoardGrid.RowDefinitions.Clear();
            BoardGrid.ColumnDefinitions.Clear();
            BoardGrid.Children.Clear();

            for (int i = 0; i < boardSize; i++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition(new GridLength(cellSize)));
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(cellSize)));
            }
        }
        else
        {
            BoardGrid.Children.Clear();
        }

        for (int i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            var cellView = CreateCellView(cell, cellSize);
            Grid.SetRow(cellView, cell.Row);
            Grid.SetColumn(cellView, cell.Col);
            BoardGrid.Children.Add(cellView);
        }
    }

    private View CreateCellView(CellModel cell, double cellSize)
    {
        Color bgColor = GetCellBackgroundColor(cell);

        var grid = new Grid
        {
            WidthRequest = cellSize,
            HeightRequest = cellSize,
            BackgroundColor = bgColor
        };

        // Шашка
        if (cell.Piece != null)
        {
            string pieceText = GetPieceText(cell.Piece);
            Color pieceColor = cell.Piece.Color == PieceColor.White
                ? Color.FromArgb("#FAFAFA")
                : Color.FromArgb("#212121");

            var label = new Label
            {
                Text = pieceText,
                FontSize = cellSize * 0.65,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = pieceColor
            };

            // Добавляем обводку для белых шашек (чтобы были видны на светлом фоне)
            if (cell.Piece.Color == PieceColor.White)
            {
                var shadow = new Label
                {
                    Text = pieceText,
                    FontSize = cellSize * 0.65,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#333333")
                };
                // Используем основной label поверх
            }

            grid.Children.Add(label);
        }

        // Индикатор допустимого хода
        if (cell.IsValidTarget && cell.Piece == null)
        {
            var dot = new BoxView
            {
                WidthRequest = cellSize * 0.25,
                HeightRequest = cellSize * 0.25,
                CornerRadius = cellSize * 0.125,
                Color = Color.FromArgb("#66000000"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            grid.Children.Add(dot);
        }

        // Обработка нажатия
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            _viewModel.CellTappedCommand.Execute(cell);
            // Перерисовываем доску после нажатия
            MainThread.BeginInvokeOnMainThread(RenderBoard);
        };
        grid.GestureRecognizers.Add(tapGesture);

        return grid;
    }

    private Color GetCellBackgroundColor(CellModel cell)
    {
        if (cell.IsSelected)
            return Color.FromArgb("#FFEB3B");

        if (cell.IsValidTarget)
            return Color.FromArgb("#A5D6A7");

        if (cell.IsLastMove)
            return Color.FromArgb("#90CAF9");

        if (!cell.IsPlayable)
            return Color.FromArgb("#EBECD0");

        return Color.FromArgb("#739552");
    }

    private static string GetPieceText(PieceModel piece)
    {
        return (piece.Color, piece.Type) switch
        {
            (PieceColor.White, PieceType.Man) => "⛀",
            (PieceColor.White, PieceType.King) => "⛁",
            (PieceColor.Black, PieceType.Man) => "⛂",
            (PieceColor.Black, PieceType.King) => "⛃",
            _ => ""
        };
    }
}
