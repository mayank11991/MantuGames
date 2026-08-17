using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class CardMemoryPage : ContentPage
{
    private CardMemoryViewModel _vm;
    private int _startLevel = 1;

    public string Level
    {
        set { if (int.TryParse(value, out int l)) _startLevel = l; }
    }

    public CardMemoryPage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AudioService.Instance.StartMusic();
        _vm = new CardMemoryViewModel(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        this.Loaded += (s, e) => BuildCards();
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
            _vm.Cleanup();
        }
    }

    // ── Pause / Resume ──────────────────────────────────────────────
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
        if (_vm == null || _vm.IsGameOver) return;

        if (CoinService.GetCoins("cardmemory") < CoinService.SolutionCost)
        {
            CoinShopPopup.ShowForGame("cardmemory");
            return;
        }
        CoinService.SpendCoins("cardmemory", CoinService.SolutionCost);

        SolutionButton.IsEnabled = false;
        _vm.RevealAll();
        _ = RevealThenEndAsync();
    }

    private async Task RevealThenEndAsync()
    {
        try
        {
            await Task.Delay(2500);
            if (_vm != null && !_vm.IsGameOver)
                _vm.ForceEnd(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in RevealThenEndAsync: {ex.Message}");
        }
    }

    // ── BACK ────────────────────────────────────────────────────────
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

    // ── BUILD CARD GRID ─────────────────────────────────────────────
    private void BuildCards()
    {
        CardGrid.Children.Clear();
        CardGrid.RowDefinitions.Clear();
        CardGrid.ColumnDefinitions.Clear();

        int cols = _vm.Columns;
        int total = _vm.Cards.Count;
        int rows = (int)Math.Ceiling((double)total / cols);

        var info = DeviceDisplay.MainDisplayInfo;
        double screenW = info.Width / info.Density;
        double screenH = info.Height / info.Density;
        double availableH = screenH - 220;
        double paddingH = 10;
        double spacingW = 8;
        double cellW = (screenW - paddingH - spacingW * (cols - 1)) / cols;
        double cellSize = Math.Min(cellW, Math.Min(availableH / rows, 72));
        cellSize = Math.Max(cellSize, 40);

        for (int r = 0; r < rows; r++)
            CardGrid.RowDefinitions.Add(new RowDefinition { Height = cellSize });
        for (int c = 0; c < cols; c++)
            CardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        for (int i = 0; i < _vm.Cards.Count; i++)
        {
            var card = _vm.Cards[i];
            int row = i / cols;
            int col = i % cols;

            var border = CreateCardBorder(card, (int)cellSize);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            CardGrid.Children.Add(border);
        }
    }

    private Border CreateCardBorder(CardViewModel card, int size)
    {
        var cardImage = new Image
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = size * 0.55,
            HeightRequest = size * 0.55,
            InputTransparent = true,
            Aspect = Aspect.AspectFit
        };

        float screwSize = Math.Max(6, size * 0.07f);
        float screwMgn = Math.Max(4, size * 0.06f);
        
        Border MakeScrew() => new Border
        {
            BackgroundColor = Color.FromArgb("#ffffff"),
            StrokeThickness = 0,
            WidthRequest = screwSize,
            HeightRequest = screwSize,
            InputTransparent = true
        };

        var screwTL = MakeScrew();
        screwTL.HorizontalOptions = LayoutOptions.Start;
        screwTL.VerticalOptions = LayoutOptions.Start;
        screwTL.Margin = new Thickness(screwMgn);
        screwTL.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = screwSize / 2 };

        var screwTR = MakeScrew();
        screwTR.HorizontalOptions = LayoutOptions.End;
        screwTR.VerticalOptions = LayoutOptions.Start;
        screwTR.Margin = new Thickness(screwMgn);
        screwTR.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = screwSize / 2 };

        var screwBL = MakeScrew();
        screwBL.HorizontalOptions = LayoutOptions.Start;
        screwBL.VerticalOptions = LayoutOptions.End;
        screwBL.Margin = new Thickness(screwMgn);
        screwBL.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = screwSize / 2 };

        var screwBR = MakeScrew();
        screwBR.HorizontalOptions = LayoutOptions.End;
        screwBR.VerticalOptions = LayoutOptions.End;
        screwBR.Margin = new Thickness(screwMgn);
        screwBR.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = screwSize / 2 };

        var questionMark = new Label
        {
            Text = "?",
            FontSize = size * 0.45,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#ffffff"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = true
        };

        var contentGrid = new Grid { InputTransparent = true };
        contentGrid.Children.Add(cardImage);
        contentGrid.Children.Add(questionMark);
        contentGrid.Children.Add(screwTL);
        contentGrid.Children.Add(screwTR);
        contentGrid.Children.Add(screwBL);
        contentGrid.Children.Add(screwBR);

        var border = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#3949AB"),
            HeightRequest = size,
            WidthRequest = size,
            Content = contentGrid
        };
        border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 };

        void SetScrewsVisible(bool visible)
        {
            screwTL.IsVisible = visible;
            screwTR.IsVisible = visible;
            screwBL.IsVisible = visible;
            screwBR.IsVisible = visible;
        }

        void ApplyState()
        {
            if (card.IsMatched)
            {
                border.BackgroundColor = Color.FromArgb("#4CAF50");
                cardImage.Source = ImageSource.FromFile(card.Image);
                questionMark.IsVisible = false;
                SetScrewsVisible(false);
            }
            else if (card.IsFlipped)
            {
                border.BackgroundColor = Color.FromArgb("#7C4DFF");
                cardImage.Source = ImageSource.FromFile(card.Image);
                questionMark.IsVisible = false;
                SetScrewsVisible(false);
            }
            else
            {
                border.BackgroundColor = Color.FromArgb("#3949AB");
                cardImage.Source = null;
                questionMark.IsVisible = true;
                SetScrewsVisible(true);
            }
        }

        ApplyState();

        card.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == nameof(CardViewModel.IsMatched) && card.IsMatched)
                AudioService.Instance.Play("correct");

            if (e.PropertyName == nameof(CardViewModel.IsFlipped) ||
                e.PropertyName == nameof(CardViewModel.IsMatched))
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await border.ScaleXTo(0, 100, Easing.Linear);
                    ApplyState();
                    await border.ScaleXTo(1, 100, Easing.Linear);
                });
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) =>
        {
            if (!_vm.IsGameOver)
            {
                _vm.FlipCommand.Execute(card);
                AudioService.Instance.Play("pop");
            }
        };
        border.GestureRecognizers.Add(tap);

        return border;
    }

    // ── GAME EVENTS ─────────────────────────────────────────────────
    private async void OnGameEnded(bool isWin)
    {
        try
        {
            int total   = _vm.TotalSecondsForLevel;
            int elapsed = total - _vm.TimeRemainingSec;

            int stars  = isWin ? ProgressService.CalcStars(elapsed, total) : 0;
            int points = isWin ? ProgressService.CalcPoints(stars, elapsed, total) : 0;

            await ResultPopup.Show(isWin, _vm.CurrentLevel, elapsed, total, stars, points, gameId: "cardmemory");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnGameEnded: {ex.Message}");
        }
    }

    private void OnNextLevel(object sender, EventArgs e)
    {
        _startLevel = _vm.CurrentLevel + 1;
        _vm.GameEnded -= OnGameEnded;
        _vm = new CardMemoryViewModel(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        SolutionButton.IsEnabled = true;
        BuildCards();
    }

    private void OnRetry(object sender, EventArgs e)
    {
        _vm.GameEnded -= OnGameEnded;
        _vm = new CardMemoryViewModel(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        SolutionButton.IsEnabled = true;
        BuildCards();
    }
}
