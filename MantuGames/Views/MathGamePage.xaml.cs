using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class MathGamePage : ContentPage
{
    private MathViewModel _vm;
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
        MathTimer.TotalSeconds = ProgressService.GetTimerSeconds(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        _vm.QuestionChanged += BuildChoices;
        this.Loaded += (s, e) =>
        {
            BuildChoices();
            UpdateProgressBar();
        };
        _vm.PropertyChanged += OnViewModelPropertyChanged;
        this.Opacity = 0;
        this.FadeTo(1, 400);
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

    // ── Pause / Resume ──────────────────────────────────────────
    private void OnPause(object sender, EventArgs e)
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
        if (CoinService.GetCoins("mathchallenge") < CoinService.SolutionCost)
        {
            CoinShopPopup.ShowForGame("mathchallenge");
            return;
        }
        CoinService.SpendCoins("mathchallenge", CoinService.SolutionCost);
        _vm?.ShowSolutionCommand.Execute(null);
    }

    // ── PROGRESS BAR ────────────────────────────────────────────
    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MathViewModel.ProgressValue))
            UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        double containerW = ProgressBar.Parent is Frame f ? f.Width : 300;
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

    private Frame CreateChoiceButton(ChoiceViewModel choice)
    {
        var label = new Label
        {
            Text = choice.Value.ToString(),
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = true
        };

        var frame = new Frame
        {
            CornerRadius = 18,
            Padding = new Thickness(0),
            HasShadow = true,
            HeightRequest = 70,
            BackgroundColor = Color.FromArgb(choice.BgColor),
            Content = label
        };

        void UpdateColor()
        {
            frame.BackgroundColor = Color.FromArgb(choice.BgColor);
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
            await frame.ScaleTo(0.92, 80);
            await frame.ScaleTo(1.0, 80);
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
            int elapsed = 210 - _vm.TimeRemainingSec;
            string reason = _vm.SolutionWasShown ? "Solution Shown" : null;

            int stars  = isWin && reason == null ? ProgressService.CalcStars(elapsed, 210) : 0;
            int points = isWin && reason == null ? ProgressService.CalcPoints(stars, elapsed, 210) : 0;

            await ResultPopup.Show(isWin, _vm.CurrentLevel, elapsed, 210, stars, points, reason, "mathchallenge");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnGameEnded: {ex.Message}");
        }
    }

    private void OnNextLevel(object sender, EventArgs e) => ResetWith(_startLevel = _vm.CurrentLevel + 1);
    private void OnRetry(object sender, EventArgs e) => ResetWith(_startLevel);

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