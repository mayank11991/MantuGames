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
    private ArrowLinesDrawable _drawable;
    private bool _isAnimating;

    public string Level
    {
        set { if (int.TryParse(value, out int l)) _startLevel = l; }
    }

    public ArrowLinesPage()
    {
        InitializeComponent();
        this.AddBannerAd();

        _drawable = new ArrowLinesDrawable();
        ArrowGraphics.Drawable = _drawable;
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnGraphicsTapped;
        ArrowGraphics.GestureRecognizers.Add(tapGesture);
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
        RefreshDrawable();
    }

    private void RefreshDrawable()
    {
        _drawable.Arrows = _vm.Arrows;
        _drawable.Rows = _vm.GridRows;
        _drawable.Cols = _vm.GridCols;
        _drawable.HighlightedArrow = null;
        _drawable.SlideArrow = null;
        _drawable.SlideProgress = 0;
        ArrowGraphics.Invalidate();
    }

    private void OnGraphicsTapped(object sender, TappedEventArgs e)
    {
        if (_isAnimating || _vm.IsGameOver) return;

        var point = e.GetPosition(ArrowGraphics);
        if (point == null) return;

        float startX = (float)(ArrowGraphics.Width - _vm.GridCols * _drawable.CellSize) / 2;
        float startY = (float)(ArrowGraphics.Height - _vm.GridRows * _drawable.CellSize) / 2;

        float relX = (float)point.Value.X - startX;
        float relY = (float)point.Value.Y - startY;

        int col = (int)(relX / _drawable.CellSize);
        int row = (int)(relY / _drawable.CellSize);

        if (row < 0 || row >= _vm.GridRows || col < 0 || col >= _vm.GridCols) return;

        var arrow = _vm.Arrows.FirstOrDefault(a => a.Row == row && a.Col == col && !a.IsCleared);
        if (arrow == null) return;

        _vm.ArrowTappedCommand.Execute(arrow);
    }

    private async void OnMoveAnimated(ArrowCell arrow, List<(int Row, int Col)> path)
    {
        _isAnimating = true;

        // Just slide the arrow off in its direction, no line
        _drawable.SlideArrow = arrow;
        _drawable.SlideProgress = 0;
        ArrowGraphics.Invalidate();

        // Animate slide out
        for (int i = 0; i <= 10; i++)
        {
            _drawable.SlideProgress = i / 10f;
            ArrowGraphics.Invalidate();
            await Task.Delay(25);
        }

        // Clear and refresh
        _drawable.SlideArrow = null;
        _drawable.SlideProgress = 0;
        _isAnimating = false;

        RefreshDrawable();
    }

    private async void OnArrowBlocked(ArrowCell arrow)
    {
        // Flash the arrow briefly
        _drawable.HighlightedArrow = arrow;
        ArrowGraphics.Invalidate();
        await Task.Delay(200);
        _drawable.HighlightedArrow = null;
        ArrowGraphics.Invalidate();
    }

    private void OnBoardChanged()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_isAnimating)
                RefreshDrawable();
        });
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
        if (_vm == null || _vm.IsGameOver || _isAnimating) return;

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
            if (_vm.IsGameOver) break;
            _vm.ArrowTappedCommand.Execute(arrow);
            await Task.Delay(350);
        }
        SolutionButton.IsEnabled = true;
    }

    protected override bool OnBackButtonPressed()
    {
        OnBackClicked(this, EventArgs.Empty);
        return true;
    }
}
