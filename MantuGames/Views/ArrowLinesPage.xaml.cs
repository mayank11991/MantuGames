using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class ArrowLinesPage : ContentPage
{
    private ArrowLinesViewModel _vm;
    private int _startLevel = 1;
    private Dictionary<int, Border> _arrowViews = new();
    private HashSet<int> _animating = new();

    public string Level
    {
        set { if (int.TryParse(value, out int l)) _startLevel = l; }
    }

    public ArrowLinesPage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AudioService.Instance.StartMusic();
        InitGame(_startLevel);
        PauseOverlay.Resumed += OnResumeGame;

        int coins = CoinService.GetCoins("arrowlines");
        SolutionCoinLabel.Text = $"* Costs {CoinService.SolutionCost} coins — you have {coins}";

        this.Opacity = 0;
        this.FadeTo(1, 400);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseOverlay.Resumed -= OnResumeGame;
        if (_vm != null)
        {
            _vm.GameEnded -= OnGameEnded;
            _vm.BoardChanged -= OnBoardChanged;
            _vm.MoveAnimated -= OnMoveAnimated;
            _vm.ArrowBlocked -= OnArrowBlocked;
            _vm.Cleanup();
        }
    }

    private void InitGame(int level)
    {
        _vm = new ArrowLinesViewModel(level);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        _vm.BoardChanged += OnBoardChanged;
        _vm.MoveAnimated += OnMoveAnimated;
        _vm.ArrowBlocked += OnArrowBlocked;
        HeartsLabel.SetBinding(Label.TextProperty, new Binding("LivesDisplay"));
        BuildGrid();
    }

    private void BuildGrid()
    {
        ArrowGrid.Children.Clear();
        ArrowGrid.RowDefinitions.Clear();
        ArrowGrid.ColumnDefinitions.Clear();
        _arrowViews.Clear();

        int rows = _vm.GridRows;
        int cols = _vm.GridCols;

        // Only column/row definitions for sizing - no visible cells
        for (int r = 0; r < rows; r++)
            ArrowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        for (int c = 0; c < cols; c++)
            ArrowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        // Place only arrows, no empty cell borders
        foreach (var arrow in _vm.Arrows)
        {
            if (arrow.IsCleared) continue;
            PlaceArrow(arrow);
        }
    }

    private void PlaceArrow(ArrowCell arrow)
    {
        var cell = new Border
        {
            BackgroundColor = Color.FromArgb("#0F172A"),
            StrokeThickness = 2,
            Stroke = GetArrowColor(arrow.Direction),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Padding = 0,
            Opacity = 1
        };

        var label = new Label
        {
            Text = arrow.Symbol.ToString(),
            TextColor = GetArrowColor(arrow.Direction),
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            FontFamily = "MomoTrustDisplay"
        };
        cell.Content = label;

        cell.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = _vm.ArrowTappedCommand,
            CommandParameter = arrow
        });

        ArrowGrid.Add(cell, arrow.Col, arrow.Row);
        _arrowViews[arrow.Index] = cell;
    }

    private void OnBoardChanged()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var toRemove = _arrowViews.Where(kv =>
                !_animating.Contains(kv.Key) &&
                _vm.Arrows.FirstOrDefault(a => a.Index == kv.Key && !a.IsCleared) == null
            ).Select(kv => kv.Key).ToList();

            foreach (var key in toRemove)
            {
                if (_arrowViews.TryGetValue(key, out var view))
                {
                    ArrowGrid.Children.Remove(view);
                    _arrowViews.Remove(key);
                }
            }
        });
    }

    private async void OnMoveAnimated(ArrowCell arrow, List<(int Row, int Col)> path)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!_arrowViews.TryGetValue(arrow.Index, out var view)) return;

            _animating.Add(arrow.Index);
            view.GestureRecognizers.Clear();

            double cellW = (ArrowGrid.Width - (_vm.GridCols - 1) * 4) / _vm.GridCols;
            double cellH = (ArrowGrid.Height - (_vm.GridRows - 1) * 4) / _vm.GridRows;

            foreach (var (r, c) in path)
            {
                double tx = (c - arrow.Col) * (cellW + 4);
                double ty = (r - arrow.Row) * (cellH + 4);
                await view.TranslateTo(tx, ty, 60, Easing.Linear);
            }

            double offX = 0, offY = 0;
            switch (arrow.Direction)
            {
                case ArrowDirection.Up: offY = -300; break;
                case ArrowDirection.Down: offY = 300; break;
                case ArrowDirection.Left: offX = -300; break;
                case ArrowDirection.Right: offX = 300; break;
            }

            await Task.WhenAll(
                view.TranslateTo(view.TranslationX + offX, view.TranslationY + offY, 100, Easing.SpringOut),
                view.FadeTo(0, 100)
            );

            ArrowGrid.Children.Remove(view);
            _arrowViews.Remove(arrow.Index);
            _animating.Remove(arrow.Index);
        });
    }

    private async void OnArrowBlocked(ArrowCell arrow)
    {
        if (!_arrowViews.TryGetValue(arrow.Index, out var view)) return;

        // Shake animation
        await view.TranslateTo(6, 0, 30);
        await view.TranslateTo(-6, 0, 30);
        await view.TranslateTo(4, 0, 20);
        await view.TranslateTo(-4, 0, 20);
        await view.TranslateTo(0, 0, 20);
    }

    private Color GetArrowColor(ArrowDirection dir)
    {
        return dir switch
        {
            ArrowDirection.Up => Color.FromArgb("#22D3EE"),
            ArrowDirection.Down => Color.FromArgb("#F97316"),
            ArrowDirection.Left => Color.FromArgb("#A855F7"),
            ArrowDirection.Right => Color.FromArgb("#34D399"),
            _ => Colors.White
        };
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost.", "Leave", "Stay");
        if (!leave) return;
        _vm?.Cleanup();
        await Shell.Current.GoToAsync("..");
    }

    private async void OnGameEnded(bool isWin)
    {
        if (isWin)
        {
            int elapsed = 0;
            int total = 210;
            int stars = _vm.Moves <= _vm.TotalArrows + 2 ? 3 :
                        _vm.Moves <= _vm.TotalArrows + 5 ? 2 : 1;
            int coins = stars == 3 ? 5 : stars == 2 ? 3 : 1;

            AudioService.Instance.Play("win");
            VibrationHelper.Click();

            await ResultPopup.Show(true, _vm.CurrentLevel, elapsed, total, stars, coins, gameId: "arrowlines");
        }
        else
        {
            AudioService.Instance.Play("lose");
            await ResultPopup.Show(false, _vm.CurrentLevel, 0, 210, reason: "No moves left!", gameId: "arrowlines");
        }
    }

    private void OnNextLevel(object sender, EventArgs e)
    {
        _vm?.Cleanup();
        InitGame(_vm.CurrentLevel + 1);
    }

    private void OnRetry(object sender, EventArgs e)
    {
        _vm?.Cleanup();
        InitGame(_startLevel);
    }

    private void OnPause(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        PauseOverlay.Show();
    }

    private void OnResumeGame(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
    }

    private async void OnShowSolution(object sender, EventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;

        if (CoinService.GetCoins("arrowlines") < CoinService.SolutionCost)
        {
            AudioService.Instance.Play("tap");
            return;
        }
        CoinService.SpendCoins("arrowlines", CoinService.SolutionCost);

        int coins = CoinService.GetCoins("arrowlines");
        SolutionCoinLabel.Text = $"* Costs {CoinService.SolutionCost} coins — you have {coins}";

        SolutionButton.IsEnabled = false;
        var remaining = _vm.Arrows.Where(a => !a.IsCleared).ToList();
        foreach (var arrow in remaining)
        {
            _vm.ArrowTappedCommand.Execute(arrow);
            await Task.Delay(300);
        }
        SolutionButton.IsEnabled = true;
    }

    protected override bool OnBackButtonPressed()
    {
        OnBackClicked(this, EventArgs.Empty);
        return true;
    }
}
