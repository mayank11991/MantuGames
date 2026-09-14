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
    private bool _isDragging = false;

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
        try
        {
            AudioService.Instance.StartMusic();
            _vm = new ConnectTheDotsViewModel(_startLevel);
            BindingContext = _vm;
            GameCanvas.Drawable = _vm.Drawable;
            _vm.GameEnded += OnGameEnded;
            _vm.BoardChanged += OnBoardChanged;
            _vm.CellTouched += OnCellTouched;
            _vm.PropertyChanged += OnViewModelPropertyChanged;
            GameCanvas.SizeChanged += OnCanvasSizeChanged;

            UpdateLevelInfo();
            UpdateCoins();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CTD] OnAppearing ERROR: {ex.Message}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
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
        if (GameCanvas.Width > 0 && GameCanvas.Height > 0)
        {
            UpdateLevelInfo();
            GameCanvas.Invalidate();
        }
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectTheDotsViewModel.IsGameOver))
        {
            // Update UI if needed
        }
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

        var drawable = _vm.Drawable as ConnectTheDotsDrawable;
        if (drawable == null) return;

        int cellIndex = drawable.HitTest(tapX, tapY, _vm.GridSize);
        if (cellIndex >= 0)
            _vm.CellTappedCommand.Execute(cellIndex);
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isDragging = true;
                break;

            case GestureStatus.Running:
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isDragging = false;
                break;
        }
    }

    private void OnCellTouched(int row, int col)
    {
        // Cell touched - handled by ViewModel
    }

    private void UpdateLevelInfo()
    {
        if (_vm == null) return;

        LevelLabel.Text = _vm.LevelDisplay;

        int level = _vm.CurrentLevel;
        string difficulty = level switch
        {
            <= 3 => "EASY",
            <= 10 => "MEDIUM",
            <= 20 => "HARD",
            _ => "EXPERT"
        };
        DifficultyLabel.Text = difficulty;
    }

    private void UpdateCoins()
    {
        int coins = CoinService.GetCoins("connectthedots");
        CoinsLabel.Text = $"💰 {coins}";
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

    private void OnRestartClicked(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        _vm?.Cleanup();
        _vm = new ConnectTheDotsViewModel(_startLevel);
        BindingContext = _vm;
        GameCanvas.Drawable = _vm.Drawable;
        _vm.GameEnded += OnGameEnded;
        _vm.BoardChanged += OnBoardChanged;
        _vm.CellTouched += OnCellTouched;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
        GameCanvas.Invalidate();
        UpdateLevelInfo();
        UpdateCoins();
    }

    private void OnRulesClicked(object sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        RulesPopup.Show(GameRules.GetRules("connectthedots"));
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

                ProgressService.Instance.CompleteLevel("connectthedots", _startLevel, stars);
                CoinService.AddCoins("connectthedots", coinsEarned);
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
                null,
                "connectthedots"
            );

            UpdateCoins();
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
