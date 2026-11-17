using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class MathGamePage : ContentPage
{
    private MathViewModel _vm = null!;
    private int _startLevel = 1;

    public string Level
    {
        set
        {
            if (int.TryParse(value, out int l)) _startLevel = l;
        }
    }

    public MathGamePage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AudioService.Instance.StartMusic();
        _vm = new MathViewModel(_startLevel);
        MathTimer.TotalSeconds = ProgressService.ClassicTimerSeconds;
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        _vm.QuestionChanged += BuildChoices;

        int coins = CoinService.Balance;
        SolutionCoinLabel.Text = $" Show Solution \n* Costs {CoinService.SolutionCost} coins — you have {coins}";
        this.Loaded += (s, e) =>
        {
            BuildChoices();
            UpdateProgressBar();
        };
        _vm.PropertyChanged += OnViewModelPropertyChanged;
        this.Opacity = 0;
        this.FadeToAsync(1, 400);
        PauseOverlay.Resumed += OnResumeGame;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseOverlay.Resumed -= OnResumeGame;
        if (_vm != null)
        {
            _vm.GameEnded -= OnGameEnded;
            _vm.QuestionChanged -= BuildChoices;
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
            _vm.Cleanup();
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        try
        {
            bool leave = ResultPopup.IsVisible || await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave", "Stay");
            if (!leave) return;
            _vm?.Cleanup();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnBackClicked: {ex.Message}");
        }
    }

    // ── Pause / Resume ──────────────────────────────────────────
    private void OnPause(object? sender, EventArgs e)
    {
        try { _vm?.PauseTimer(); } catch { }
        PauseOverlay.Show();
    }

    private void OnResumeGame(object? sender, EventArgs e)
    {
        try { _vm?.ResumeTimer(); } catch { }
    }

    private void OnShowSolutionClicked(object? sender, EventArgs e)
    {
        if (CoinService.Balance < CoinService.SolutionCost)
        {
            CoinShopPopup.ShowForGame("mathchallenge");
            return;
        }
        CoinService.SpendForSolution("mathchallenge");
        int coins = CoinService.Balance;
        SolutionCoinLabel.Text = $"* Costs {CoinService.SolutionCost} coins — you have {coins}";
        _vm?.ShowSolutionCommand.Execute(null);
    }

    // ── PROGRESS BAR ────────────────────────────────────────────
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MathViewModel.ProgressValue))
            UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        double containerW = ProgressBar.Parent is Border f ? f.Width : 300;
        if (containerW <= 0) containerW = 300;
        ProgressBar.WidthRequest = Math.Max(0, _vm.ProgressValue * containerW);
    }

    // ── BUILD CHOICE BUTTONS ─────────────────────────────────────
    private void BuildChoices()
    {
        ChoicesGrid.Children.Clear();

        int i = 0;
        foreach (var choice in _vm.Choices)
        {
            int row = i / 2;
            int col = i % 2;

            var btn = CreateChoiceButton(choice);
            Grid.SetRow(btn, row);
            Grid.SetColumn(btn, col);
            ChoicesGrid.Children.Add(btn);
            i++;
        }
    }

    private Border CreateChoiceButton(ChoiceViewModel choice)
    {
        var label = new Label
        {
            Text = choice.Value.ToString(),
            FontSize = 30,
            FontFamily = SkinService.TitleFont,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = true
        };

        var frame = new Border
        {
            Stroke = Colors.White,
            StrokeThickness = 3,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 },
            Padding = new Thickness(0),
            HeightRequest = 70,
            BackgroundColor = Color.FromArgb(choice.BgColor),
            Shadow = new Shadow { Brush = Color.FromArgb(choice.LipColor), Offset = new Point(0, 6), Radius = 0, Opacity = 1 },
            Content = label
        };

        void UpdateColor()
        {
            frame.BackgroundColor = Color.FromArgb(choice.BgColor);
            frame.Shadow = new Shadow { Brush = Color.FromArgb(choice.LipColor), Offset = new Point(0, 6), Radius = 0, Opacity = 1 };
        }

        choice.PropertyChanged += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(UpdateColor);
            if (e.PropertyName == nameof(ChoiceViewModel.State))
            {
                if (choice.State == ChoiceState.Correct)
                    AudioService.Instance.Play("correct");
                else if (choice.State == ChoiceState.Wrong)
                    AudioService.Instance.Play("wrong");
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            if (_vm.IsGameOver) return;
            await frame.ScaleToAsync(0.92, 80);
            await frame.ScaleToAsync(1.0, 80);
            _vm.AnswerCommand.Execute(choice);
        };
        frame.GestureRecognizers.Add(tap);

        return frame;
    }

    // ── GAME EVENTS ──────────────────────────────────────────────
    private async void OnGameEnded(bool isWin)
    {
        try
        {
            int elapsed = ProgressService.ClassicTimerSeconds - _vm.TimeRemainingSec;
            string? reason = _vm.SolutionWasShown ? "Solution Shown" : null;

            int stars  = isWin && reason == null ? ProgressService.CalcStars(elapsed, ProgressService.ClassicTimerSeconds) : 0;
            int coins = isWin && reason == null ? stars switch { 3 => 5, 2 => 3, 1 => 1, _ => 0 } : 0;

            await ResultPopup.Show(isWin, _vm.CurrentLevel, elapsed, ProgressService.ClassicTimerSeconds, stars, coins, reason, "mathchallenge");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnGameEnded: {ex.Message}");
        }
    }

    private void OnNextLevel(object? sender, EventArgs e) => ResetWith(_startLevel = _vm.CurrentLevel + 1);
    private void OnRetry(object? sender, EventArgs e) => ResetWith(_startLevel);

    private void ResetWith(int level)
    {
        _vm.GameEnded -= OnGameEnded;
        _vm.QuestionChanged -= BuildChoices;
        _vm = new MathViewModel(level);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        _vm.QuestionChanged += BuildChoices;
        BuildChoices();
        UpdateProgressBar();
    }
}