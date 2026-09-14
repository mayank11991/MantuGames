using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class ConnectTheDotsPage : ContentPage
{
    private ConnectTheDotsViewModel _vm;
    private int _startLevel = 1;
    private bool _hasDrawn = false;

    public string Level
    {
        set
        {
            if (int.TryParse(value, out int l)) _startLevel = l;
        }
    }

    public ConnectTheDotsPage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("[CTD] OnAppearing start");
        try
        {
            AudioService.Instance.StartMusic();
            System.Diagnostics.Debug.WriteLine("[CTD] Audio started");
            _vm = new ConnectTheDotsViewModel(_startLevel);
            System.Diagnostics.Debug.WriteLine($"[CTD] VM created, pairs={_vm.Pairs.Count}, size={_vm.GridSize}");
            BindingContext = _vm;
            GameCanvas.Drawable = _vm.Drawable;
            _vm.GameEnded += OnGameEnded;
            _vm.BoardChanged += OnBoardChanged;
            _vm.CellTouched += OnCellTouched;
            _vm.PropertyChanged += OnViewModelPropertyChanged;
            GameCanvas.SizeChanged += OnCanvasSizeChanged;
            this.Opacity = 0;
            this.FadeTo(1, 400);
            PauseOverlay.Resumed += OnResumeGame;

            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(200), () =>
            {
                GameCanvas.Invalidate();
                return false;
            });
            System.Diagnostics.Debug.WriteLine("[CTD] OnAppearing done");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CTD] OnAppearing ERROR: {ex.Message}\n{ex.StackTrace}");
            TestBox.IsVisible = true;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseOverlay.Resumed -= OnResumeGame;
        GameCanvas.SizeChanged -= OnCanvasSizeChanged;
        if (_vm != null)
        {
            _vm.GameEnded -= OnGameEnded;
            _vm.BoardChanged -= OnBoardChanged;
            _vm.CellTouched -= OnCellTouched;
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
            _vm.Cleanup();
        }
    }

    private void OnCanvasSizeChanged(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[CTD] SizeChanged: {GameCanvas.Width}x{GameCanvas.Height}");
        if (GameCanvas.Width > 0 && GameCanvas.Height > 0)
        {
            UpdateLevelBadge();
            UpdateScore();
            GameCanvas.Invalidate();
        }
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectTheDotsViewModel.Score))
            UpdateScore();
        else if (e.PropertyName == nameof(ConnectTheDotsViewModel.IsGameOver))
            UpdateButtonsVisibility();
    }

    private void OnBoardChanged()
    {
        MainThread.BeginInvokeOnMainThread(() => GameCanvas.Invalidate());
    }

    private void OnCanvasTapped(object sender, TappedEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;

        var position = e.GetPosition(GameCanvas);
        if (position == null) return;

        float tapX = (float)position.Value.X;
        float tapY = (float)position.Value.Y;

        // Use the drawable's layout info to convert to grid cell
        var drawable = _vm.Drawable as ConnectTheDotsDrawable;
        if (drawable == null) return;

        int cellIndex = drawable.HitTest(tapX, tapY, _vm.GridSize);
        if (cellIndex >= 0)
            _vm.CellTappedCommand.Execute(cellIndex);
    }

    private void OnCellTouched(int row, int col)
    {
        if (!_hasDrawn)
        {
            _hasDrawn = true;
        }
    }

    private void UpdateLevelBadge()
    {
        if (_vm != null)
            LevelBadge.Text = _vm.LevelDisplay;
    }

    private void UpdateScore()
    {
        if (_vm != null)
            ScoreLabel.Text = $"Score: {_vm.Score}";
    }

    private void UpdateButtonsVisibility()
    {
        OnPropertyChanged(nameof(ConnectTheDotsViewModel.IsGameOver));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave", "Stay");
            if (!leave) return;
            _vm?.Cleanup();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnBackClicked: {ex.Message}");
        }
    }

    private void OnPauseClicked(object sender, EventArgs e)
    {
        try { _vm?.PauseTimer(); } catch { }
        PauseOverlay.Show();
    }

    private void OnResumeGame(object sender, EventArgs e)
    {
        try { _vm?.ResumeTimer(); } catch { }
    }

    private void OnShowSolutionClicked(object sender, EventArgs e)
    {
        if (CoinService.GetCoins("connectthedots") < CoinService.SolutionCost)
        {
            CoinShopPopup.ShowForGame("connectthedots");
            return;
        }
        CoinService.SpendCoins("connectthedots", CoinService.SolutionCost);
        _vm?.ShowSolutionCommand.Execute(null);
    }

    private void OnRestartClicked(object sender, EventArgs e)
    {
        _vm?.Cleanup();
        _vm = new ConnectTheDotsViewModel(_startLevel);
        BindingContext = _vm;
        GameCanvas.Drawable = _vm.Drawable;
        _vm.GameEnded += OnGameEnded;
        _vm.BoardChanged += OnBoardChanged;
        _vm.CellTouched += OnCellTouched;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
        _hasDrawn = false;
        GameCanvas.Invalidate();
        UpdateLevelBadge();
        UpdateScore();
    }

    private async void OnGameEnded(bool win)
    {
        try
        {
            AudioService.Instance.Play(win ? "win" : "lose");
            VibrationHelper.Click();

            int stars = 0;
            int coinsEarned = 0;
            int timeBonus = 0;
            int elapsedSeconds = 0;

            if (win && _vm != null)
            {
                double ratio = (double)_vm.TimeRemainingSec / _vm.PuzzleTimerSeconds;
                if (ratio > 0.6) stars = 3;
                else if (ratio > 0.3) stars = 2;
                else stars = 1;

                coinsEarned = stars switch { 3 => 5, 2 => 3, _ => 1 };
                timeBonus = _vm.TimeRemainingSec * 2;
                elapsedSeconds = _vm.PuzzleTimerSeconds - _vm.TimeRemainingSec;
                _vm.Score += timeBonus;
                UpdateScore();

                ProgressService.Instance.CompleteLevel("connectthedots", _startLevel, stars);
            }
            else
            {
                elapsedSeconds = _vm?.PuzzleTimerSeconds ?? 120;
            }

            StatsService.RecordGame("connectthedots", win, coinsEarned);

            await Task.Delay(400);
            ResultPopup.Show(
                win,
                _startLevel,
                elapsedSeconds,
                _vm?.PuzzleTimerSeconds ?? 120,
                stars,
                coinsEarned,
                _vm?.SolutionWasShown ?? false ? "Solution shown" : null,
                "connectthedots"
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnGameEnded: {ex.Message}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        OnBackClicked(this, EventArgs.Empty);
        return true;
    }
}