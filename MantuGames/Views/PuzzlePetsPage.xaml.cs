using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class PuzzlePetsPage : ContentPage
{
    private static readonly string[] AnimalImages =
    {
        "butterfly.png", "cat.png", "elephant.png", "girafe.png",
        "lion.png", "octopas.png", "owl.png"
    };

    private int _level = 1;
    private PuzzlePetsPuzzle _puzzle;
    private int _totalSec;
    private int _remainSec;
    private IDispatcherTimer _gameTimer;
    private DateTime _startTime;
    private bool _gameEnded;
    private readonly List<Border> _gridCells = new();
    private double _pieceSize;
    private int _dragPieceId = -1;

    private static readonly Color[] PiecePalette =
    {
        Color.FromArgb("#E91E63"), Color.FromArgb("#2196F3"),
        Color.FromArgb("#4CAF50"), Color.FromArgb("#FF5722"),
        Color.FromArgb("#9C27B0"), Color.FromArgb("#00BCD4"),
        Color.FromArgb("#FF9800"), Color.FromArgb("#3F51B5"),
        Color.FromArgb("#8BC34A"), Color.FromArgb("#FF4081"),
        Color.FromArgb("#7C4DFF"), Color.FromArgb("#00E676"),
    };

    public string Level
    {
        set
        {
            if (int.TryParse(Uri.UnescapeDataString(value ?? "1"), out int l))
                _level = l;
        }
    }

    public PuzzlePetsPage()
    {
        InitializeComponent();
        this.AddBannerAd();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AudioService.Instance.StartMusic();
        StartLevel();
        this.Opacity = 0;
        this.FadeTo(1, 350);
        PauseOverlay.Resumed += OnResumeGame;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseOverlay.Resumed -= OnResumeGame;
        _gameTimer?.Stop();
    }

    // ── Level init ──────────────────────────────────────────────────────────
    private void StartLevel()
    {
        _puzzle = new PuzzlePetsPuzzle(_level);
        _gameEnded = false;
        _dragPieceId = -1;
        _totalSec = ProgressService.GetTimerSeconds(_level);
        _remainSec = _totalSec;
        _startTime = DateTime.Now;

        LevelLabel.Text = $"Level {_level}";
        StatusLabel.Text = "Drag pieces from the tray to their spots!";

        _gridCells.Clear();

        double screenW = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        double availW = screenW - 64;
        double gap = 4;
        _pieceSize = Math.Floor((availW - gap * (_puzzle.Cols - 1) - 16) / _puzzle.Cols);
        _pieceSize = Math.Max(44, Math.Min(_pieceSize, 76));

        BuildGrid();
        BuildTray();

        UpdateTimerUI();
        ResultPopup.Hide();
        _gameTimer?.Stop();
        StartTimer();
    }

    // ── Build puzzle grid ──────────────────────────────────────────────────
    private void BuildGrid()
    {
        PuzzleGrid.Children.Clear();
        PuzzleGrid.RowDefinitions.Clear();
        PuzzleGrid.ColumnDefinitions.Clear();

        for (int r = 0; r < _puzzle.Rows; r++)
            PuzzleGrid.RowDefinitions.Add(new RowDefinition { Height = _pieceSize });
        for (int c = 0; c < _puzzle.Cols; c++)
            PuzzleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = _pieceSize });

        for (int pos = 0; pos < _puzzle.TotalPieces; pos++)
        {
            int row = pos / _puzzle.Cols;
            int col = pos % _puzzle.Cols;
            Color hintColor = GetPieceColor(pos);

            var iconImage = new Image
            {
                Source = ImageSource.FromFile(AnimalImages[pos % AnimalImages.Length]),
                WidthRequest = _pieceSize * 0.55,
                HeightRequest = _pieceSize * 0.55,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Opacity = 0.55,
                Aspect = Aspect.AspectFit,
                InputTransparent = true,
            };

            var numLabel = new Label
            {
                Text = (pos + 1).ToString(),
                FontSize = _pieceSize * 0.32,
                FontFamily = "BrickSans",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                Opacity = 0.5,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 4, 2),
                InputTransparent = true,
            };

            var content = new Grid
            {
                WidthRequest = _pieceSize,
                HeightRequest = _pieceSize,
                InputTransparent = true,
                Children = { iconImage }
            };

            var cell = new Border
            {
                BackgroundColor = hintColor.WithAlpha(0.30f),
                Stroke = hintColor.WithAlpha(0.7f),
                StrokeThickness = 2.5,
                StrokeDashArray = new DoubleCollection { 6, 4 },
                WidthRequest = _pieceSize,
                HeightRequest = _pieceSize,
                Content = content,
            };
            cell.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 };

            int capturedPos = pos;

            var drop = new DropGestureRecognizer();
            drop.DragOver += (s, e) => OnDragOverCell(e, capturedPos);
            drop.DragLeave += (s, e) => OnDragLeaveCell(capturedPos);
            drop.Drop += (s, e) => OnDropOnCell(e, capturedPos);
            cell.GestureRecognizers.Add(drop);

            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);
            PuzzleGrid.Children.Add(cell);
            _gridCells.Add(cell);
        }
    }

    // ── Build piece tray ───────────────────────────────────────────────────
    private void BuildTray()
    {
        PieceTray.Children.Clear();

        foreach (int pieceId in _puzzle.TrayPieces)
        {
            var piece = CreatePieceView(pieceId);
            PieceTray.Children.Add(piece);
        }
    }

    private Border CreatePieceView(int pieceId)
    {
        Color color = GetPieceColor(pieceId);
        var stack = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        stack.Children.Add(new Image
        {
            Source = ImageSource.FromFile(AnimalImages[pieceId % AnimalImages.Length]),
            WidthRequest = _pieceSize * 0.55,
            HeightRequest = _pieceSize * 0.55,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Aspect = Aspect.AspectFit
        });

        var border = new Border
        {
            BackgroundColor = color.WithAlpha(0.85f),
            StrokeThickness = 2,
            Stroke = Colors.White.WithAlpha(0.6f),
            WidthRequest = _pieceSize,
            HeightRequest = _pieceSize,
            Content = stack,
        };
        border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 };

        int capturedId = pieceId;
        var drag = new DragGestureRecognizer();
        drag.DragStarting += (s, e) =>
        {
            _dragPieceId = capturedId;
            e.Data.Text = capturedId.ToString();
        };
        drag.DropCompleted += (s, e) =>
        {
            _dragPieceId = -1;
        };
        border.GestureRecognizers.Add(drag);

        return border;
    }

    // ── Drag & Drop handlers ──────────────────────────────────────────────
    private void OnDragOverCell(DragEventArgs e, int gridPos)
    {
        if (_gameEnded || _dragPieceId < 0) return;
        if (_puzzle.GridState[gridPos] != -1) return;
        int pieceId = _dragPieceId;

        bool isCorrect = _puzzle.PiecePositions[pieceId] == gridPos;
        var cell = _gridCells[gridPos];

        if (isCorrect)
        {
            cell.BackgroundColor = GetPieceColor(pieceId).WithAlpha(0.3f);
            cell.Stroke = Color.FromArgb("#66BB6A");
            cell.StrokeDashArray = null;
        }
        else
        {
            cell.BackgroundColor = Color.FromArgb("#B71C1C44");
            cell.Stroke = Color.FromArgb("#EF5350");
            cell.StrokeDashArray = new DoubleCollection { 2, 2 };
        }
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnDragLeaveCell(int gridPos)
    {
        if (_gameEnded) return;
        ResetCellAppearance(gridPos);
    }

    private async void OnDropOnCell(DropEventArgs e, int gridPos)
    {
        try
        {
            if (_gameEnded) return;
            ResetCellAppearance(gridPos);

            if (_dragPieceId < 0) return;
            int pieceId = _dragPieceId;
            _dragPieceId = -1;

            bool success = _puzzle.TryPlace(pieceId, gridPos);

            if (success)
            {
                AudioService.Instance.Play("correct");
                await PlacePieceOnGrid(pieceId, gridPos);
                RebuildTray();

                StatusLabel.Text =
                    $"{_puzzle.Moves} / {_puzzle.TotalPieces} placed";

                if (_puzzle.IsSolved)
                    TriggerWin();
            }
            else
            {
                AudioService.Instance.Play("wrong");
                StatusLabel.Text = $"Piece {pieceId + 1} goes in a different spot";
                var cell = _gridCells[gridPos];
                await cell.ScaleTo(1.1, 60, Easing.SpringOut);
                await cell.ScaleTo(1.0, 60, Easing.SpringIn);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnDropOnCell: {ex.Message}");
        }
    }

    // ── Place piece onto grid ──────────────────────────────────────────────
    private async Task PlacePieceOnGrid(int pieceId, int gridPos)
    {
        if (gridPos >= _gridCells.Count) return;
        var cell = _gridCells[gridPos];

        var placed = CreatePieceView(pieceId);
        cell.Content = placed;
        cell.BackgroundColor = Colors.Transparent;
        cell.StrokeDashArray = null;
        cell.Stroke = Colors.Transparent;

        placed.Scale = 0.5;
        placed.Opacity = 0;
        await Task.WhenAll(
            placed.ScaleTo(1.15, 150, Easing.SpringOut),
            placed.FadeTo(1, 120)
        );
        await placed.ScaleTo(1.0, 100, Easing.SpringIn);
    }

    private void RebuildTray()
    {
        PieceTray.Children.Clear();
        foreach (int pieceId in _puzzle.TrayPieces)
        {
            var piece = CreatePieceView(pieceId);
            PieceTray.Children.Add(piece);
        }

        StatusLabel.Text = _puzzle.TrayPieces.Count == 0
            ? "All pieces placed!"
            : $"{_puzzle.TrayPieces.Count} piece{(_puzzle.TrayPieces.Count > 1 ? "s" : "")} left";
    }

    // ── Visual helpers ────────────────────────────────────────────────────
    private void ResetCellAppearance(int gridPos)
    {
        if (gridPos >= _gridCells.Count) return;
        var cell = _gridCells[gridPos];
        if (_puzzle.GridState[gridPos] != -1) return;

        Color hintColor = GetPieceColor(gridPos);
        cell.BackgroundColor = hintColor.WithAlpha(0.25f);
        cell.Stroke = hintColor.WithAlpha(0.7f);
        cell.StrokeThickness = 2.5;
        cell.StrokeDashArray = new DoubleCollection { 6, 4 };
    }

    private Color GetPieceColor(int pieceId) =>
        PiecePalette[pieceId % PiecePalette.Length];

    // ── Pause / Resume ──────────────────────────────────────────────────────
    private void OnPause(object sender, EventArgs e)
    {
        _gameTimer?.Stop();
        PauseOverlay.Show();
    }

    private void OnResumeGame(object sender, EventArgs e)
    {
        _gameTimer?.Start();
    }

    // ── Timer ──────────────────────────────────────────────────────────────
    private void StartTimer()
    {
        _gameTimer = Dispatcher.CreateTimer();
        _gameTimer.Interval = TimeSpan.FromSeconds(1);
        _gameTimer.Tick += OnGameTick;
        _gameTimer.Start();
    }

    private void OnGameTick(object s, EventArgs e)
    {
        if (_gameEnded) return;
        _remainSec = Math.Max(0, _remainSec - 1);
        UpdateTimerUI();
        if (_remainSec <= 0) EndGame(win: false);
    }

    private void UpdateTimerUI()
    {
        PuzzleTimer.TotalSeconds = _totalSec;
        PuzzleTimer.SecondsRemaining = _remainSec;
    }

    // ── Win / Lose ─────────────────────────────────────────────────────────
    private async void TriggerWin()
    {
        try
        {
            if (_gameEnded) return;
            _gameEnded = true;
            _gameTimer?.Stop();

            int elapsed = (int)(DateTime.Now - _startTime).TotalSeconds;
            int stars = ProgressService.CalcStars(elapsed, _totalSec);
            int coins = stars switch { 3 => 5, 2 => 3, 1 => 1, _ => 0 };

            foreach (var cell in _gridCells)
            {
                await cell.ScaleTo(1.08, 60, Easing.SpringOut);
                await cell.ScaleTo(1.0, 60, Easing.SpringIn);
            }

            StatusLabel.Text = "Puzzle Complete!";

            _ = ResultPopup.Show(true, _level, elapsed, _totalSec, stars, coins, gameId: "puzzlepets");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in TriggerWin: {ex.Message}");
        }
    }

    private void EndGame(bool win)
    {
        if (_gameEnded) return;
        _gameEnded = true;
        _gameTimer?.Stop();
        _ = ResultPopup.Show(win, _level, _totalSec, _totalSec);
    }

    // ── Navigation ──────────────────────────────────────────────────────────
    private void OnNextLevel(object sender, EventArgs e)
    {
        _level++;
        StartLevel();
    }

    private void OnRetry(object sender, EventArgs e) => StartLevel();

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave", "Stay");
            if (!leave) return;
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnBackClicked: {ex.Message}");
        }
    }

    private async void OnShowSolution(object sender, EventArgs e)
    {
        try
        {
            if (CoinService.GetCoins("puzzlepets") < CoinService.SolutionCost)
            {
                CoinShopPopup.ShowForGame("puzzlepets");
                return;
            }
            CoinService.SpendCoins("puzzlepets", CoinService.SolutionCost);

            SolutionButton.IsEnabled = false;
            StatusLabel.Text = "Placing pieces...";

            for (int pos = 0; pos < _puzzle.TotalPieces; pos++)
            {
                if (_puzzle.GridState[pos] != -1) continue;

                int pieceId = Array.IndexOf(_puzzle.PiecePositions, pos);
                if (pieceId < 0 || !_puzzle.TrayPieces.Contains(pieceId)) continue;

                _puzzle.TryPlace(pieceId, pos);

                var placed = CreatePieceView(pieceId);
                var cell = _gridCells[pos];
                cell.Content = placed;
                cell.BackgroundColor = Colors.Transparent;
                cell.StrokeDashArray = null;
                cell.Stroke = Colors.Transparent;

                placed.AnchorY = 2;
                placed.Scale = 0.3;
                placed.Opacity = 0;
                placed.TranslationY = 40;
                await Task.WhenAll(
                    placed.TranslateTo(0, 0, 300, Easing.CubicInOut),
                    placed.ScaleTo(1.15, 180, Easing.SpringOut),
                    placed.FadeTo(1, 150)
                );
                await placed.ScaleTo(1.0, 80, Easing.SpringIn);

                RebuildTray();
                await Task.Delay(80);
            }

            if (_puzzle.IsSolved)
            {
                StatusLabel.Text = "All pieces placed!";
                TriggerWin();
            }
            else
            {
                StatusLabel.Text = "Puzzle complete!";
                SolutionButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnShowSolution: {ex.Message}");
        }
    }
}
