using TypingTrainer.ViewModels;

namespace TypingTrainer.Views;

public partial class GamePage : ContentPage
{
    private readonly GameViewModel _viewModel;

    public GamePage(GameViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Subscribe to character states changes to update colored text
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(GameViewModel.CharacterStates))
            {
                UpdateColoredText();
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeGameCommand.ExecuteAsync(null);
        UpdateColoredText();
        InputEntry.Focus();

        InputEntry.TextChanged += OnInputTextChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        InputEntry.TextChanged -= OnInputTextChanged;
    }

    private void OnInputTextChanged(object? sender, TextChangedEventArgs e)
    {
        _viewModel.ProcessInputCommand.Execute(e.NewTextValue ?? string.Empty);
    }

    /// <summary>
    /// Updates the target text label with colored characters based on typing state.
    /// Green = correct, Red = error, Yellow background = current, Gray = pending.
    /// </summary>
    private void UpdateColoredText()
    {
        var states = _viewModel.CharacterStates;
        if (states == null || states.Count == 0)
        {
            TargetTextLabel.FormattedText = new FormattedString();
            return;
        }

        var formattedString = new FormattedString();

        foreach (var charState in states)
        {
            var span = new Span
            {
                Text = charState.Character.ToString(),
                FontSize = _viewModel.FontSize,
                FontFamily = "monospace"
            };

            switch (charState.State)
            {
                case CharState.Correct:
                    span.TextColor = Color.FromArgb("#4CAF50"); // Green
                    break;
                case CharState.Error:
                    span.TextColor = Color.FromArgb("#F44336"); // Red
                    span.TextDecorations = TextDecorations.Underline;
                    break;
                case CharState.Current:
                    span.TextColor = Color.FromArgb("#000000"); // Black
                    span.BackgroundColor = Color.FromArgb("#FFC107"); // Yellow highlight
                    break;
                case CharState.Pending:
                    span.TextColor = Color.FromArgb("#9E9E9E"); // Gray
                    break;
            }

            formattedString.Spans.Add(span);
        }

        TargetTextLabel.FormattedText = formattedString;
    }
}
