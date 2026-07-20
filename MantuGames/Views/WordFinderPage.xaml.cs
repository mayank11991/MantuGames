using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class WordFinderPage : ContentPage
{
    private WordFinderViewModel _vm;
    private int _startLevel = 1;
    private int _cellSize;
    private int _dragStartRow, _dragStartCol;
    private int _lastDragRow, _lastDragCol;
    private bool _isDragging;

    public string Level
    {
        set
        {
            if (int.TryParse(value, out int l)) _startLevel = l;
        }
    }

    public WordFinderPage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AudioService.Instance.StartMusic();
        _vm = new WordFinderViewModel(_startLevel);
        WordFinderTimer.TotalSeconds = ProgressService.GetTimerSeconds(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        this.Loaded += (s, e) => BuildGrid();
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

    private void OnRulesClicked(object sender, EventArgs e)
    {
        RulesPopup.Show(GameRules.GetRules("wordfinder"));
    }

    // ── BACK ────────────────────────────────────────────────────
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

    // ── BUILD LETTER GRID ────────────────────────────────────────
    private void BuildGrid()
    {
        LetterGrid.Children.Clear();
        LetterGrid.RowDefinitions.Clear();
        LetterGrid.ColumnDefinitions.Clear();

        int size = _vm.GridSize;
        var info = DeviceDisplay.MainDisplayInfo;
        double screenW = info.Width / info.Density;
        double screenH = info.Height / info.Density;
        // Reserve space for top bar/title/timer (~160), word list panel (~120), solution row (~56), padding
        double availableH = screenH - 160 - 120 - 56 - 24;
        double availableW = screenW - 24;
        _cellSize = (int)Math.Min(Math.Min(availableW / size, availableH / size), 44);

        for (int i = 0; i < size; i++)
        {
            LetterGrid.RowDefinitions.Add(new RowDefinition { Height = _cellSize });
            LetterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = _cellSize });
        }

        foreach (var cell in _vm.Cells)
        {
            var frame = CreateCellFrame(cell, _cellSize);
            Grid.SetRow(frame, cell.Row);
            Grid.SetColumn(frame, cell.Col);
            LetterGrid.Children.Add(frame);
        }
    }

    private Frame CreateCellFrame(LetterCellViewModel cell, int size)
    {
        var label = new Label
        {
            Text = cell.Letter.ToString(),
            FontSize = size * 0.45,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            TextColor = Color.FromArgb("#1B5E20"),
            InputTransparent = true
        };

        var frame = new Frame
        {
            Padding = 0,
            Margin = new Thickness(1),
            CornerRadius = 8,
            HasShadow = false,
            HeightRequest = size,
            WidthRequest = size,
            BackgroundColor = Color.FromArgb("#F1F8E9"),
            Content = label
        };

        void UpdateColors()
        {
            if (cell.IsFound)
            {
                frame.BackgroundColor = Color.FromArgb("#4CAF50");
                label.TextColor = Colors.White;
            }
            else if (cell.IsHinted)
            {
                frame.BackgroundColor = Color.FromArgb("#EDE7F6");
                label.TextColor = Color.FromArgb("#6A1B9A");
            }
            else if (cell.IsSelected)
            {
                frame.BackgroundColor = Color.FromArgb("#FFE066");
                label.TextColor = Color.FromArgb("#333333");
            }
            else
            {
                frame.BackgroundColor = Color.FromArgb("#F1F8E9");
                label.TextColor = Color.FromArgb("#1B5E20");
            }
        }

        UpdateColors();

        cell.PropertyChanged += (s, e) =>
            MainThread.BeginInvokeOnMainThread(UpdateColors);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            if (!cell.IsFound && !_vm.IsGameOver)
            {
                _vm.TapCellCommand.Execute(cell);
                AudioService.Instance.Play("tap");
                if (cell.IsSelected)
                {
                    await frame.ScaleTo(1.15, 60);
                    await frame.ScaleTo(1.0, 60);
                }
            }
        };
        frame.GestureRecognizers.Add(tap);

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += (s, e) =>
        {
            if (_vm.IsGameOver || cell.IsFound) return;
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _isDragging = true;
                    _dragStartRow = cell.Row;
                    _dragStartCol = cell.Col;
                    _lastDragRow = cell.Row;
                    _lastDragCol = cell.Col;
                    _vm.ClearSelection();
                    _vm.AddToSelection(cell);
                    break;
                case GestureStatus.Running:
                    if (!_isDragging) return;
                    int targetRow = _dragStartRow + (int)Math.Round(e.TotalY / _cellSize);
                    int targetCol = _dragStartCol + (int)Math.Round(e.TotalX / _cellSize);
                    targetRow = Math.Clamp(targetRow, 0, _vm.GridSize - 1);
                    targetCol = Math.Clamp(targetCol, 0, _vm.GridSize - 1);
                    if (targetRow != _lastDragRow || targetCol != _lastDragCol)
                    {
                        int dr = targetRow - _dragStartRow;
                        int dc = targetCol - _dragStartCol;
                        if (dr != 0 && dc != 0)
                        {
                            int steps = Math.Min(Math.Abs(dr), Math.Abs(dc));
                            targetRow = _dragStartRow + Math.Sign(dr) * steps;
                            targetCol = _dragStartCol + Math.Sign(dc) * steps;
                        }
                        targetRow = Math.Clamp(targetRow, 0, _vm.GridSize - 1);
                        targetCol = Math.Clamp(targetCol, 0, _vm.GridSize - 1);
                        if (targetRow != _lastDragRow || targetCol != _lastDragCol)
                        {
                            var targetCell = _vm.Cells.FirstOrDefault(c => c.Row == targetRow && c.Col == targetCol);
                            if (targetCell != null && !targetCell.IsFound)
                            {
                                _lastDragRow = targetRow;
                                _lastDragCol = targetCol;
                                _vm.AddToSelection(targetCell);
                            }
                        }
                    }
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    _isDragging = false;
                    break;
            }
        };
        frame.GestureRecognizers.Add(pan);

        // Bounce animation when word is found
        cell.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == nameof(LetterCellViewModel.IsFound) && cell.IsFound)
            {
                AudioService.Instance.Play("clear");
                await frame.ScaleTo(1.3, 100);
                await frame.ScaleTo(1.0, 100);
            }
        };

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

            await ResultPopup.Show(isWin, _vm.CurrentLevel, elapsed, 210, stars, points, reason, "wordfinder");
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
        _vm = new WordFinderViewModel(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        BuildGrid();
    }

    private void OnRetry(object sender, EventArgs e)
    {
        _vm.GameEnded -= OnGameEnded;
        _vm = new WordFinderViewModel(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        BuildGrid();
    }
}