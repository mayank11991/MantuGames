using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

[QueryProperty(nameof(Level), "level")]
public partial class AnimalCrushPage : ContentPage
{
    private AnimalCrushViewModel _vm;
    private int _startLevel = 1;
    private Border _selectedPowerUpBorder;
    private AnimalCrushPuzzle.PowerUpKind? _activePowerUp;
    private readonly Dictionary<(int r, int c), Border> _cellBorders = new();

    public string Level
    {
        set { if (int.TryParse(value, out int l)) _startLevel = l; }
    }

    public AnimalCrushPage()
    {
        InitializeComponent();
        this.AddBannerAd();
        System.Diagnostics.Debug.WriteLine("[AnimalCrush] Constructor");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AudioService.Instance.StartMusic();
        _vm = new AnimalCrushViewModel(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        this.Loaded += (s, e) => BuildBoard();
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

    // ── BOARD ───────────────────────────────────────────────────────
    private void BuildBoard()
    {
        if (_vm?.Puzzle == null) return;
        _cellBorders.Clear();
        BoardGrid.Children.Clear();
        BoardGrid.RowDefinitions.Clear();
        BoardGrid.ColumnDefinitions.Clear();

        var info = DeviceDisplay.MainDisplayInfo;
        double screenW = info.Width / info.Density;
        double screenH = info.Height / info.Density;
        double availableH = screenH - 280;
        double cellSize = Math.Min((screenW - 16) / 8, Math.Min(availableH / 8, 72));
        cellSize = Math.Max(cellSize, 36);

        for (int r = 0; r < 8; r++)
            BoardGrid.RowDefinitions.Add(new RowDefinition { Height = cellSize });
        for (int c = 0; c < 8; c++)
            BoardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cellSize });

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                int type = _vm.Puzzle.Board[r, c];
                var cell = CreateCell(r, c, type, cellSize);
                Grid.SetRow(cell, r);
                Grid.SetColumn(cell, c);
                BoardGrid.Children.Add(cell);
                _cellBorders[(r, c)] = cell;
            }
        }
    }

    private Border CreateCell(int r, int c, int type, double size)
    {
        var img = new Image
        {
            Source = type >= 0 && type < AnimalCrushPuzzle.Animals.Length
                ? AnimalCrushPuzzle.Animals[type]
                : null,
            WidthRequest = size * 0.72,
            HeightRequest = size * 0.72,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Aspect = Aspect.AspectFit,
            InputTransparent = true,
        };

        var border = new Border
        {
            BackgroundColor = Color.FromArgb("#182136"),
            StrokeThickness = 0,
            WidthRequest = size,
            HeightRequest = size,
            Content = img,
        };
        border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 };

        // Tap for power-up targeting
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => OnCellTapped(r, c, border);
        border.GestureRecognizers.Add(tap);

        return border;
    }

    // ── Pan gesture on board for swipes (Candy-Crush style) ──────────────
    private (int r, int c) _dragStartCell = (-1, -1);
    private bool _isDragging;
    private Point _panStartPoint;

    private void OnBoardPan(object sender, PanUpdatedEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver || _vm.IsBusy || _activePowerUp != null) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartPoint = new Point(e.TotalX, e.TotalY);
                var startCell = GetCellAtPointFromGrid(_panStartPoint);
                if (startCell != null)
                {
                    _dragStartCell = startCell.Value;
                    _isDragging = true;
                    HighlightCell(startCell.Value.r, startCell.Value.c, true);
                }
                break;

            case GestureStatus.Running:
                if (_isDragging)
                {
                    var currentPoint = new Point(e.TotalX, e.TotalY);
                    var cell = GetCellAtPointFromGrid(currentPoint);
                    if (cell != null && (cell.Value.r != _dragStartCell.r || cell.Value.c != _dragStartCell.c))
                    {
                        int dr = cell.Value.r - _dragStartCell.r;
                        int dc = cell.Value.c - _dragStartCell.c;
                        if (Math.Abs(dr) + Math.Abs(dc) == 1)
                        {
                            ClearHighlights();
                            HighlightCell(cell.Value.r, cell.Value.c, true);
                        }
                    }
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_isDragging)
                {
                    var endPoint = new Point(e.TotalX, e.TotalY);
                    var endCell = GetCellAtPointFromGrid(endPoint);
                    
                    if (endCell != null && (endCell.Value.r != _dragStartCell.r || endCell.Value.c != _dragStartCell.c))
                    {
                        int dr = endCell.Value.r - _dragStartCell.r;
                        int dc = endCell.Value.c - _dragStartCell.c;
                        if (Math.Abs(dr) + Math.Abs(dc) == 1)
                        {
                            // Valid adjacent swipe - normalize to unit direction
                            dr = Math.Sign(dr);
                            dc = Math.Sign(dc);
                            _ = TrySwapWithAnimation(_dragStartCell.r, _dragStartCell.c, dr, dc);
                        }
                    }
                    ClearHighlights();
                }
                _isDragging = false;
                _dragStartCell = (-1, -1);
                break;
        }
    }

    private (int r, int c)? GetCellAtPoint(Point pt)
    {
        // Use the BoardGrid's coordinate system
        foreach (var kvp in _cellBorders)
        {
            var border = kvp.Value;
            var rect = new Rect(border.X, border.Y, border.Width, border.Height);
            if (rect.Contains(pt))
                return kvp.Key;
        }
        return null;
    }

    // Alternative: use row/col from Grid position
    private (int r, int c)? GetCellAtPointFromGrid(Point pt)
    {
        // Convert point to grid row/col
        double cellWidth = BoardGrid.Width / 8;
        double cellHeight = BoardGrid.Height / 8;
        
        int c = (int)(pt.X / cellWidth);
        int r = (int)(pt.Y / cellHeight);
        
        if (r >= 0 && r < 8 && c >= 0 && c < 8)
            return (r, c);
        return null;
    }

    private void HighlightCell(int r, int c, bool on)
    {
        if (_cellBorders.TryGetValue((r, c), out var border))
        {
            border.StrokeThickness = on ? 3 : 0;
            border.Stroke = on ? new SolidColorBrush(Color.FromArgb("#FF9F1C")) : null;
        }
    }

    private void ClearHighlights()
    {
        foreach (var border in _cellBorders.Values)
        {
            border.StrokeThickness = 0;
            border.Stroke = null;
        }
    }

    private async Task<bool> TrySwapWithAnimation(int r, int c, int dr, int dc)
    {
        if (_vm == null) return false;
        
        // First, animate the swap visually
        await AnimateSwap(r, c, r + dr, c + dc);
        
        // Then execute the swap in the model
        bool ok = await _vm.TrySwipeAsync(r, c, dr, dc);
        if (ok)
        {
            AudioService.Instance.Play("pop");
            if (_vm.Score > 0) AudioService.Instance.Play("correct");
            
            // Animate cascades (falls + new blocks)
            await AnimateCascades();
        }
        else
        {
            AudioService.Instance.Play("wrong");
            // Animate failed swap (wobble back)
            await AnimateFailedSwap(r, c, r + dr, c + dc);
        }
        return ok;
    }

    private async void OnCellPan(int r, int c, PanUpdatedEventArgs e)
    {
        if (_vm == null || _vm.IsGameOver || _vm.IsBusy || _activePowerUp != null) return;

        if (e.StatusType == GestureStatus.Completed)
        {
            double dx = e.TotalX, dy = e.TotalY;
            if (Math.Abs(dx) < 24 && Math.Abs(dy) < 24) return;

            int dr = 0, dc = 0;
            if (Math.Abs(dx) > Math.Abs(dy))
                dc = dx > 0 ? 1 : -1;
            else
                dr = dy > 0 ? 1 : -1;

            bool ok = await _vm.TrySwipeAsync(r, c, dr, dc);
            if (ok)
            {
                AudioService.Instance.Play("pop");
                if (_vm.Score > 0) AudioService.Instance.Play("correct");
                BuildBoard();
            }
            else
            {
                AudioService.Instance.Play("wrong");
            }
        }
    }

    private async void OnCellTapped(int r, int c, Border border)
    {
        if (_vm == null || _vm.IsGameOver || _vm.IsBusy) return;

        if (_activePowerUp != null)
        {
            // Apply selected power-up on this tile
            var kind = _activePowerUp.Value;
            if (_vm.CountOf(kind) <= 0)
            {
                // Offer to buy
                int cost = kind switch
                {
                    AnimalCrushPuzzle.PowerUpKind.Hammer => AnimalCrushViewModel.HammerCost,
                    AnimalCrushPuzzle.PowerUpKind.Rocket => AnimalCrushViewModel.RocketCost,
                    _ => AnimalCrushViewModel.BombCost,
                };
                bool buy = await ConfirmPopup.Show($"Buy {kind} for {cost} coins?",
                    $"You have {CoinService.GetCoins("animalcrush")} coins.", "Buy", "Cancel");
                if (buy && _vm.BuyPowerUp(kind))
                {
                    await ApplyPowerUp(kind, r, c, border);
                }
                else
                {
                    DeselectPowerUp();
                }
                return;
            }

            await ApplyPowerUp(kind, r, c, border);
            DeselectPowerUp();
            return;
        }

        // No power-up active: tap does nothing (swipe is used for swaps)
    }

    // ── Power-up selection ──────────────────────────────────────────────
    private void OnHammerTapped(object sender, EventArgs e) => TogglePowerUp(AnimalCrushPuzzle.PowerUpKind.Hammer, HammerButton);
    private void OnRocketTapped(object sender, EventArgs e) => TogglePowerUp(AnimalCrushPuzzle.PowerUpKind.Rocket, RocketButton);
    private void OnBombTapped(object sender, EventArgs e) => TogglePowerUp(AnimalCrushPuzzle.PowerUpKind.Bomb, BombButton);

    private void TogglePowerUp(AnimalCrushPuzzle.PowerUpKind kind, Border button)
    {
        if (_vm == null || _vm.IsGameOver) return;

        if (_activePowerUp == kind)
        {
            DeselectPowerUp();
            return;
        }

        if (_vm.CountOf(kind) <= 0)
        {
            // Count is 0 - will prompt buy on cell tap; still allow selection
        }

        SelectPowerUp(kind, button);
    }

    private void SelectPowerUp(AnimalCrushPuzzle.PowerUpKind kind, Border button)
    {
        DeselectPowerUp();
        _activePowerUp = kind;
        _selectedPowerUpBorder = button;
        button.StrokeThickness = 3;
        button.Stroke = new SolidColorBrush(Color.FromArgb("#FF9F1C"));
    }

    private void DeselectPowerUp()
    {
        if (_selectedPowerUpBorder != null)
        {
            _selectedPowerUpBorder.StrokeThickness = 0;
            _selectedPowerUpBorder.Stroke = null;
            _selectedPowerUpBorder = null;
        }
        _activePowerUp = null;
    }

    // ── Animations ──────────────────────────────────────────────────────
    private async Task AnimateSwap(int r1, int c1, int r2, int c2)
    {
        if (!_cellBorders.TryGetValue((r1, c1), out var b1) || !_cellBorders.TryGetValue((r2, c2), out var b2))
            return;

        var img1 = b1.Content as Image;
        var img2 = b2.Content as Image;
        
        if (img1 == null || img2 == null) return;

        // Animate the images swapping (smoother than moving borders)
        var src1 = img1.Source;
        var src2 = img2.Source;

        // Visual swap with translate
        await Task.WhenAll(
            img1.TranslateTo((c2 - c1) * img1.Width, (r2 - r1) * img1.Height, 150, Easing.CubicOut),
            img2.TranslateTo((c1 - c2) * img2.Width, (r1 - r2) * img2.Height, 150, Easing.CubicOut)
        );

        // Swap the sources
        img1.Source = src2;
        img2.Source = src1;

        // Reset translations
        img1.TranslationX = img1.TranslationY = 0;
        img2.TranslationX = img2.TranslationY = 0;
    }

    private async Task AnimateFailedSwap(int r1, int c1, int r2, int c2)
    {
        if (!_cellBorders.TryGetValue((r1, c1), out var b1) || !_cellBorders.TryGetValue((r2, c2), out var b2))
            return;

        // Quick wobble
        await Task.WhenAll(
            b1.TranslateTo(15, 0, 60, Easing.CubicOut),
            b2.TranslateTo(-15, 0, 60, Easing.CubicOut)
        );
        await Task.WhenAll(
            b1.TranslateTo(-10, 0, 60, Easing.CubicInOut),
            b2.TranslateTo(10, 0, 60, Easing.CubicInOut)
        );
        await Task.WhenAll(
            b1.TranslateTo(0, 0, 80, Easing.SpringOut),
            b2.TranslateTo(0, 0, 80, Easing.SpringOut)
        );
    }

    // ── Cascade Animation (match-3 falling blocks) ──────────────────────
    private async Task AnimateCascades()
    {
        if (_vm?.Puzzle == null) return;

        // Get the cascade sequence from the model
        var cascadeRounds = GetCascadeRounds();
        
        foreach (var round in cascadeRounds)
        {
            if (round.matched.Count == 0) break;
            
            // 1. Animate matched cells disappearing (scale down + fade)
            await AnimateMatchClear(round.matched);
            
            // 2. Animate gravity - blocks above fall down
            await AnimateGravity(round.fallen);
            
            // 3. Animate new blocks spawning from top
            await AnimateNewBlocks(round.newBlocks);
            
            // Small delay between cascade rounds
            await Task.Delay(150);
        }
        
        // Final board sync - just update images, don't rebuild
        UpdateCellImages();
    }

    private class CascadeRound
    {
        public List<(int r, int c)> matched = new();
        public List<(int fromR, int fromC, int toR, int toC)> fallen = new();
        public List<(int r, int c, int type)> newBlocks = new();
    }

    private List<CascadeRound> GetCascadeRounds()
    {
        var rounds = new List<CascadeRound>();
        var board = _vm.Puzzle.Board;
        
        // We need to simulate what the model does in ResolveCascades
        // but track visual changes for animation
        var workingBoard = (int[,])board.Clone();
        int rows = 8, cols = 8;
        
        while (true)
        {
            var matches = AnimalCrushPuzzle.FindAllMatches(workingBoard);
            if (matches.Count == 0) break;
            
            var round = new CascadeRound();
            round.matched = matches;
            
            // Mark matched as -1 (empty)
            foreach (var (r, c) in matches)
                workingBoard[r, c] = -1;
            
            // Apply gravity and track fallen blocks
            for (int c = 0; c < cols; c++)
            {
                int writeRow = rows - 1;
                for (int r = rows - 1; r >= 0; r--)
                {
                    if (workingBoard[r, c] >= 0)
                    {
                        if (writeRow != r)
                        {
                            round.fallen.Add((r, c, writeRow, c));
                            workingBoard[writeRow, c] = workingBoard[r, c];
                        }
                        writeRow--;
                    }
                }
                // Fill new blocks at top
                for (int r = writeRow; r >= 0; r--)
                {
                    int newType = Random.Shared.Next(_vm.Puzzle.AnimalCount);
                    workingBoard[r, c] = newType;
                    round.newBlocks.Add((r, c, newType));
                }
            }
            
            rounds.Add(round);
        }
        
        return rounds;
    }

    private async Task AnimateMatchClear(List<(int r, int c)> matched)
    {
        var tasks = new List<Task>();
        foreach (var (r, c) in matched)
        {
            if (_cellBorders.TryGetValue((r, c), out var cell))
            {
                // Pop + fade out
                tasks.Add(AnimateCellClear(cell));
            }
        }
        await Task.WhenAll(tasks);
    }

    private async Task AnimateCellClear(Border cell)
    {
        await Task.WhenAll(
            cell.ScaleTo(0.1, 120, Easing.CubicIn),
            cell.FadeTo(0, 120, Easing.CubicIn)
        );
        cell.Opacity = 1; // Will be reset when new content arrives
        cell.Scale = 1;
    }

    private async Task AnimateGravity(List<(int fromR, int fromC, int toR, int toC)> fallen)
    {
        var tasks = new List<Task>();
        
        foreach (var (fromR, fromC, toR, toC) in fallen)
        {
            if (!_cellBorders.TryGetValue((fromR, fromC), out var cell)) continue;
            
            // Calculate distance to fall
            double fallDistance = (toR - fromR) * cell.Height;
            
            // Move the cell content (image) down
            var img = cell.Content as Image;
            if (img != null)
            {
                // Animate the image falling
                tasks.Add(img.TranslateTo(0, fallDistance, 250, Easing.BounceOut));
            }
            else
            {
                tasks.Add(cell.TranslateTo(0, fallDistance, 250, Easing.BounceOut));
            }
        }
        
        await Task.WhenAll(tasks);
        
        // Reset translations - the logical positions will be updated by UpdateCellImages
        foreach (var border in _cellBorders.Values)
        {
            if (border.Content is Image img)
                img.TranslationY = 0;
            else
                border.TranslationY = 0;
        }
    }

    private async Task AnimateNewBlocks(List<(int r, int c, int type)> newBlocks)
    {
        var tasks = new List<Task>();
        
        foreach (var (r, c, type) in newBlocks)
        {
            if (!_cellBorders.TryGetValue((r, c), out var cell)) continue;
            
            // Set the new image
            var img = cell.Content as Image;
            if (img != null && type >= 0 && type < AnimalCrushPuzzle.Animals.Length)
            {
                img.Source = AnimalCrushPuzzle.Animals[type];
                img.Opacity = 0;
                img.TranslationY = -cell.Height * 2; // Start from above
                
                // Animate falling from top
                tasks.Add(AnimateBlockFall(img, cell.Height * 2));
            }
        }
        
        await Task.WhenAll(tasks);
    }

    private async Task AnimateBlockFall(Image img, double startOffsetY)
    {
        // Fall with bounce
        await img.TranslateTo(0, 0, 300, Easing.BounceOut);
        await img.FadeTo(1, 150, Easing.CubicOut);
    }

    // Update only the cell images from the model, don't recreate borders
    private void UpdateCellImages()
    {
        if (_vm?.Puzzle == null) return;
        
        foreach (var kvp in _cellBorders)
        {
            var (r, c) = kvp.Key;
            var border = kvp.Value;
            int type = _vm.Puzzle.Board[r, c];
            
            var img = border.Content as Image;
            if (img != null)
            {
                if (type >= 0 && type < AnimalCrushPuzzle.Animals.Length)
                    img.Source = AnimalCrushPuzzle.Animals[type];
                else
                    img.Source = null;
            }
        }
    }
    private async Task ApplyPowerUp(AnimalCrushPuzzle.PowerUpKind kind, int r, int c, Border border)
    {
        System.Diagnostics.Debug.WriteLine($"[AnimalCrush] ApplyPowerUp: {kind} at ({r},{c})");
        
        int gained = await _vm.UsePowerUpAsync(kind, r, c);
        if (gained < 0) return;

        switch (kind)
        {
            case AnimalCrushPuzzle.PowerUpKind.Hammer:
                await AnimateHammer(r, c);
                break;
            case AnimalCrushPuzzle.PowerUpKind.Rocket:
                await AnimateRocket(r, c);
                break;
            case AnimalCrushPuzzle.PowerUpKind.Bomb:
                await AnimateBomb(r, c);
                break;
        }

        if (gained > 0)
        {
            AudioService.Instance.Play("pop");
            AudioService.Instance.Play("correct");
        }
        else
        {
            AudioService.Instance.Play("tap");
        }
        
        // Animate cascades after power-up
        await AnimateCascades();
    }

    private async Task AnimateHammer(int r, int c)
    {
        if (!_cellBorders.TryGetValue((r, c), out var cell)) return;
        
        // Smash effect - scale down with rotation + particles
        await Task.WhenAll(
            cell.ScaleTo(0.1, 120, Easing.CubicIn),
            cell.FadeTo(0, 120, Easing.CubicIn),
            cell.RotateTo(15, 60, Easing.CubicOut)
        );
        await cell.RotateTo(0, 60, Easing.SpringOut);
        
        // The cascade animation will handle the falling blocks
    }

    private async Task AnimateRocket(int r, int c)
    {
        var affected = AnimalCrushPuzzle.GetPowerUpCells(AnimalCrushPuzzle.PowerUpKind.Rocket, r, c);
        
        // Rocket beam animation - line effect across row and column
        var tasks = new List<Task>();
        
        foreach (var (cr, cc) in affected)
        {
            if (!_cellBorders.TryGetValue((cr, cc), out var cell)) continue;
            
            // Beam flash along the line
            tasks.Add(AnimateRocketBeam(cell, cr == r));
        }
        
        await Task.WhenAll(tasks);
    }

    private async Task AnimateRocketBeam(Border cell, bool isHorizontal)
    {
        // Quick beam flash
        var originalBg = cell.BackgroundColor;
        cell.BackgroundColor = Color.FromArgb("#FF9F1C");
        await Task.Delay(60);
        cell.BackgroundColor = originalBg;
        
        // Scale pulse with rotation
        await Task.WhenAll(
            cell.ScaleTo(1.4, 100, Easing.CubicOut),
            cell.RotateTo(isHorizontal ? 0 : 90, 100, Easing.CubicOut)
        );
        await cell.ScaleTo(1.0, 150, Easing.SpringOut);
    }

    private async Task AnimateBomb(int r, int c)
    {
        var affected = AnimalCrushPuzzle.GetPowerUpCells(AnimalCrushPuzzle.PowerUpKind.Bomb, r, c);
        
        // Bomb blast wave - expanding ring
        var tasks = new List<Task>();
        
        foreach (var (cr, cc) in affected)
        {
            if (!_cellBorders.TryGetValue((cr, cc), out var cell)) continue;
            
            int dist = Math.Abs(cr - r) + Math.Abs(cc - c);
            int delay = dist * 40;
            
            tasks.Add(AnimateBombBlast(cell, delay));
        }
        
        await Task.WhenAll(tasks);
    }

    private async Task AnimateBombBlast(Border cell, int delay)
    {
        await Task.Delay(delay);
        
        // Explosion - scale up and fade with rotation
        await Task.WhenAll(
            cell.ScaleTo(1.8, 100, Easing.CubicOut),
            cell.FadeTo(0, 100, Easing.CubicOut),
            cell.RotateTo(180, 100, Easing.CubicOut)
        );
        
        // Debris fall
        await cell.ScaleTo(0.1, 150, Easing.CubicIn);
        
        // Reappear with bounce
        cell.Rotation = 0;
        await Task.WhenAll(
            cell.ScaleTo(1.2, 200, Easing.BounceOut),
            cell.FadeTo(1, 150, Easing.CubicOut)
        );
        await cell.ScaleTo(1.0, 100, Easing.CubicIn);
    }

    // ── SHOW SOLUTION ───────────────────────────────────────────────
    private async void OnShowSolutionClicked(object sender, EventArgs e)
    {
        if (_vm == null || _vm.IsGameOver) return;

        if (CoinService.GetCoins("animalcrush") < CoinService.SolutionCost)
        {
            CoinShopPopup.ShowForGame("animalcrush");
            return;
        }
        CoinService.SpendCoins("animalcrush", CoinService.SolutionCost);

        SolutionButton.IsEnabled = false;
        var hint = _vm.FindHint();
        if (hint == null)
        {
            SolutionButton.IsEnabled = true;
            return;
        }

        HighlightHint(hint.Value.r1, hint.Value.c1, hint.Value.r2, hint.Value.c2);
        await Task.Delay(1800);
        SolutionButton.IsEnabled = true;
        BuildBoard();
    }

    private void HighlightHint(int r1, int c1, int r2, int c2)
    {
        foreach (var child in BoardGrid.Children)
        {
            int row = Grid.GetRow((BindableObject)child);
            int col = Grid.GetColumn((BindableObject)child);
            bool isHint = (row == r1 && col == c1) || (row == r2 && col == c2);
            if (child is Border b)
            {
                b.StrokeThickness = isHint ? 3 : 0;
                b.Stroke = isHint ? new SolidColorBrush(Color.FromArgb("#22D3EE")) : null;
            }
        }
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

            await ResultPopup.Show(isWin, _vm.CurrentLevel, elapsed, total, stars, points,
                isWin ? null : "Time's up!", "animalcrush");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnGameEnded: {ex.Message}");
        }
    }

    private void StartNewGame()
    {
        DeselectPowerUp();
        _vm.GameEnded -= OnGameEnded;
        _vm = new AnimalCrushViewModel(_startLevel);
        BindingContext = _vm;
        _vm.GameEnded += OnGameEnded;
        SolutionButton.IsEnabled = true;
        BuildBoard();
    }

    private void OnNextLevel(object sender, EventArgs e)
    {
        _startLevel = _vm.CurrentLevel + 1;
        StartNewGame();
    }

    private void OnRetry(object sender, EventArgs e)
    {
        StartNewGame();
    }
}