using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.Views.Controls;
using Microsoft.Maui.Graphics;

namespace MantuGames.Views;

// ── Drawable ────────────────────────────────────────────────────────────────
internal sealed class MazeDrawable : IDrawable
{
    public MazePuzzle? Maze { get; set; }
    public float PlayerRow { get; set; }
    public float PlayerCol { get; set; }
    public HashSet<(int r, int c)> Trail { get; } = new();
    public float PlayerBob { get; set; }

    public float CellSize { get; private set; }
    public float OffsetX { get; private set; }
    public float OffsetY { get; private set; }

    private static readonly Color WallColor = Color.FromArgb("#5DB83A");
    private static readonly Color RimColor = Color.FromArgb("#3E8E25");
    private static readonly Color FloorColor = Color.FromArgb("#FFEFC2");
    private static readonly Color TrailColor = Color.FromArgb("#FFC83D");

    public void Draw(ICanvas canvas, RectF bounds)
    {
        if (Maze == null) return;

        var (ox, oy, cs) = Layout(bounds);

        // 1) The picture: a dark rim, then the green hedge, drawn only under the shape's cells
        //    (each cell slightly oversized so neighbours melt into one smooth silhouette).
        DrawSilhouette(canvas, ox, oy, cs, cs * 0.2f, RimColor);
        DrawSilhouette(canvas, ox, oy, cs, cs * 0.1f, WallColor);

        // 2) Each open cell is a round clearing
        canvas.FillColor = FloorColor;
        float radius = cs * 0.38f;
        for (int r = 0; r < Maze.Rows; r++)
        for (int c = 0; c < Maze.Cols; c++)
        {
            if (!Maze.Active[r, c]) continue;
            canvas.FillCircle(ox + c * cs + cs / 2f, oy + r * cs + cs / 2f, radius);
        }

        // 3) Passages join neighbouring clearings
        float pw = cs * 0.36f;
        for (int r = 0; r < Maze.Rows; r++)
        for (int c = 0; c < Maze.Cols; c++)
        {
            if (!Maze.Active[r, c]) continue;
            float cx = ox + c * cs + cs / 2f;
            float cy = oy + r * cs + cs / 2f;
            if (Maze.CanMove(r, c, 0, 1))
                canvas.FillRoundedRectangle(cx, cy - pw / 2f, cs, pw, pw / 2f);
            if (Maze.CanMove(r, c, 1, 0))
                canvas.FillRoundedRectangle(cx - pw / 2f, cy, pw, cs, pw / 2f);
        }

        // Entrance marker: a small orange arrow under the start clearing.
        var (sr, sc) = Maze.Start;
        float sx = ox + sc * cs + cs / 2f, sy = oy + sr * cs + cs / 2f;
        var arrow = new PathF();
        float a = cs * 0.22f;
        arrow.MoveTo(sx, sy + cs * 0.52f);
        arrow.LineTo(sx - a, sy + cs * 0.52f + a);
        arrow.LineTo(sx + a, sy + cs * 0.52f + a);
        arrow.Close();
        canvas.FillColor = Color.FromArgb("#FF8A3D");
        canvas.FillPath(arrow);

        // 4) Breadcrumb trail — tiny dots
        canvas.FillColor = TrailColor;
        float tr = cs * 0.08f;
        foreach (var (trr, tc) in Trail)
        {
            float cx = ox + tc * cs + cs / 2f;
            float cy = oy + trr * cs + cs / 2f;
            canvas.FillCircle(cx, cy, tr);
        }

        // 5) Player glow
        float px = ox + PlayerCol * cs + cs / 2f;
        float py = oy + PlayerRow * cs + cs / 2f + PlayerBob;
        float pr = cs * 0.22f;
        canvas.FillColor = Color.FromArgb("#22B388FF");
        canvas.FillCircle(px, py + 2, pr * 1.1f);
    }

    private void DrawSilhouette(ICanvas canvas, float ox, float oy, float cs, float grow, Color color)
    {
        canvas.FillColor = color;
        for (int r = 0; r < Maze!.Rows; r++)
        for (int c = 0; c < Maze.Cols; c++)
        {
            if (!Maze.Active[r, c]) continue;
            canvas.FillRoundedRectangle(ox + c * cs - grow, oy + r * cs - grow, cs + grow * 2, cs + grow * 2, cs * 0.3f);
        }
    }

    public (float ox, float oy, float cs) Layout(RectF bounds)
    {
        if (Maze == null) return (0, 0, 0);
        // Half a cell of margin on every side for the rim, plus room for the entrance arrow.
        CellSize = Math.Min(bounds.Width / (Maze.Cols + 0.6f), bounds.Height / (Maze.Rows + 1.2f));
        OffsetX = (bounds.Width - CellSize * Maze.Cols) / 2f;
        OffsetY = (bounds.Height - CellSize * (Maze.Rows + 0.6f)) / 2f + CellSize * 0.1f;
        return (OffsetX, OffsetY, CellSize);
    }
}

// ── Page ─────────────────────────────────────────────────────────────────────
[QueryProperty(nameof(Level), "level")]
public partial class MazeRunnerPage : ContentPage
{
    private int _level = 1;

    private MazePuzzle? _maze;
    private MazeDrawable _drawable = new();
    private int _playerRow;
    private int _playerCol;
    private int _moves;
    private bool _isMoving;
    private bool _gameEnded;
    private int _totalSec;
    private int _remainSec;
    private IDispatcherTimer? _gameTimer;
    private IDispatcherTimer? _animTimer;
    private DateTime _startTime;
    private float _bobPhase;

    public string Level
    {
        set
        {
            if (int.TryParse(Uri.UnescapeDataString(value ?? "1"), out int l))
                _level = l;
        }
    }

    public MazeRunnerPage()
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
        this.FadeToAsync(1, 350);
        PauseOverlay.Resumed += OnResumeGame;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseOverlay.Resumed -= OnResumeGame;
        StopTimers();
    }

    // ── Level init ──────────────────────────────────────────────────────────
    private void StartLevel()
    {
        _maze = MazePuzzle.ForLevel(_level);
        (_playerRow, _playerCol) = _maze.Start;
        _moves = 0;
        _gameEnded = false;
        _totalSec = ProgressService.GetTimerSeconds(_level);
        _remainSec = _totalSec;
        _startTime = DateTime.Now;

      
            int coins = CoinService.Balance;
            SolutionCoinLabel.Text = $" Show Solution \n* Costs {CoinService.SolutionCost} coins — you have {coins}";
      

        DestinationImage.IsVisible = true;

        LevelLabel.Text = $"Level {_level} · {_maze.ShapeName}";
        MovesLabel.Text = "Moves: 0";

        _drawable = new MazeDrawable
        {
            Maze = _maze,
            PlayerRow = _playerRow,
            PlayerCol = _playerCol,
        };
        _drawable.Trail.Add(_maze.Start);
        MazeCanvas.Drawable = _drawable;

        // Reset destination image state from previous win animation
        DestinationImage.Opacity = 1;
        DestinationImage.Scale = 1;

        UpdateTimerUI();
        ResultPopup.Hide();
        StopTimers();
        StartTimers();
    }

    // ── Pause / Resume ──────────────────────────────────────────────────────
    private void OnPause(object? sender, EventArgs e)
    {
        _gameTimer?.Stop();
        _animTimer?.Stop();
        PauseOverlay.Show();
    }

    private void OnResumeGame(object? sender, EventArgs e)
    {
        _gameTimer?.Start();
        _animTimer?.Start();
    }

    private void OnShowSolutionClicked(object? sender, EventArgs e)
    {
        if (_maze == null || _gameEnded) return;

        if (CoinService.Balance < CoinService.SolutionCost)
        {
            CoinShopPopup.ShowForGame("mazerunner");
            return;
        }

        CoinService.SpendForSolution("mazerunner");
        int coins = CoinService.Balance;
        SolutionCoinLabel.Text = $"* Costs {CoinService.SolutionCost} coins — you have {coins}";

        SolutionCoinLabel.IsEnabled = false;
        _ = RevealPathAsync();
    }

    // Shortest path (BFS) from start to exit, then auto-walk the player along it.
    private async Task RevealPathAsync()
    {
        try
        {
            var path = SolvePath();
            if (path == null || path.Count == 0) return;

            _drawable.Trail.Clear();
            foreach (var step in path)
                _drawable.Trail.Add(step);
            for (int i = 0; i < path.Count; i++)
            {
                _drawable.PlayerRow = path[i].r;
                _drawable.PlayerCol = path[i].c;
                UpdatePlayerImagePosition();
                MazeCanvas.Invalidate();
                await Task.Delay(90);
                if (_gameEnded) return;
            }

            // Hide flag before player arrives at exit
            DestinationImage.IsVisible = false;

            await Task.Delay(400);
            if (!_gameEnded)
                EndGame(win: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RevealPathAsync: {ex.Message}");
        }
    }

    private List<(int r, int c)>? SolvePath() =>
        _maze?.PathTo((_playerRow, _playerCol), _maze.End);

    // ── Timers ──────────────────────────────────────────────────────────────
    private void StartTimers()
    {
        _gameTimer = Dispatcher.CreateTimer();
        _gameTimer.Interval = TimeSpan.FromSeconds(1);
        _gameTimer.Tick += OnGameTick;
        _gameTimer.Start();

        _animTimer = Dispatcher.CreateTimer();
        _animTimer.Interval = TimeSpan.FromMilliseconds(16);
        _animTimer.Tick += OnAnimTick;
        _animTimer.Start();
    }

    private void StopTimers()
    {
        _gameTimer?.Stop();
        _animTimer?.Stop();
    }

    private void OnGameTick(object? s, EventArgs e)
    {
        if (_gameEnded) return;
        _remainSec = Math.Max(0, _remainSec - 1);
        UpdateTimerUI();
        if (_remainSec is 15 or 10 or 5 or 3 or 2 or 1)
            AudioService.Instance.Play("countdown");
        if (_remainSec <= 0) EndGame(win: false);
    }

    private void OnAnimTick(object? s, EventArgs e)
    {
        _bobPhase += 0.07f;
        _drawable.PlayerBob = MathF.Sin(_bobPhase) * 2.5f;
        MazeCanvas.Invalidate();
        UpdatePlayerImagePosition();
    }

    private void UpdatePlayerImagePosition()
    {
        if (_maze == null) return;
        float cs = _drawable.CellSize;
        if (cs <= 0) return;
        float ox = _drawable.OffsetX;
        float oy = _drawable.OffsetY;

        float px = ox + _drawable.PlayerCol * cs + cs / 2f;
        float py = oy + _drawable.PlayerRow * cs + cs / 2f + _drawable.PlayerBob;

        float imgHalf = cs * 0.36f;
        PlayerImage.WidthRequest = cs * 0.72f;
        PlayerImage.HeightRequest = cs * 0.72f;
        PlayerImage.TranslationX = px - imgHalf;
        PlayerImage.TranslationY = py - imgHalf;

        float dstHalf = cs * 0.34f;
        float dx = ox + _maze.End.c * cs + cs / 2f;
        float dy = oy + _maze.End.r * cs + cs / 2f;
        DestinationImage.WidthRequest = cs * 0.68f;
        DestinationImage.HeightRequest = cs * 0.68f;
        DestinationImage.TranslationX = dx - dstHalf;
        DestinationImage.TranslationY = dy - dstHalf;
    }

    // ── Pan handler (distance-proportional movement) ───────────────────────
    private double _panTotalX, _panTotalY;

    private void OnPanMaze(object? s, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panTotalX = 0;
                _panTotalY = 0;
                break;
            case GestureStatus.Running:
                _panTotalX = e.TotalX;
                _panTotalY = e.TotalY;
                break;
            case GestureStatus.Completed:
                if (_isMoving || _gameEnded || _maze == null) return;
                double dx = _panTotalX;
                double dy = _panTotalY;
                if (Math.Abs(dx) < 20 && Math.Abs(dy) < 20) return;

                float cs = GetCellSize();
                if (cs <= 0) return;

                int dr = 0, dc = 0, cells;
                if (Math.Abs(dx) >= Math.Abs(dy))
                {
                    cells = (int)(Math.Abs(dx) / cs);
                    dc = dx > 0 ? 1 : -1;
                }
                else
                {
                    cells = (int)(Math.Abs(dy) / cs);
                    dr = dy > 0 ? 1 : -1;
                }

                cells = Math.Max(1, Math.Min(cells, 10));
                _ = TryMoveMultiple(dr, dc, cells);
                break;
        }
    }

    private float GetCellSize()
    {
        return _maze == null ? 0 : _drawable.CellSize;
    }

    private async Task TryMoveMultiple(int dr, int dc, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_gameEnded || _maze == null) break;
            await TryMove(dr, dc, showBump: false);
        }
    }

    // ── Movement logic ──────────────────────────────────────────────────────
    private async Task TryMove(int dr, int dc, bool showBump = true)
    {
        if (_isMoving || _gameEnded || _maze == null) return;

        bool canMove = _maze.CanMove(_playerRow, _playerCol, dr, dc);

        if (!canMove)
        {
            if (!showBump) return;
            _isMoving = true;
            AudioService.Instance.Play("bump");
            await MazeCanvas.TranslateToAsync(dc * 6, dr * 6, 60, Easing.CubicOut);
            await MazeCanvas.TranslateToAsync(0, 0, 80, Easing.SpringOut);
            _isMoving = false;
            return;
        }

        _isMoving = true;
        AudioService.Instance.Play("move");
        int newRow = _playerRow + dr;
        int newCol = _playerCol + dc;

        float startRow = _playerRow;
        float startCol = _playerCol;

        // Smooth translation animation (~150 ms)
        var tcs = new TaskCompletionSource();
        var anim = new Animation(t =>
        {
            _drawable.PlayerRow = startRow + dr * (float)t;
            _drawable.PlayerCol = startCol + dc * (float)t;
            MazeCanvas.Invalidate();
        }, 0, 1, Easing.CubicOut);

        anim.Commit(this, "MazeMove", rate: 16, length: 150,
            finished: (_, __) =>
            {
                _playerRow = newRow;
                _playerCol = newCol;
                _drawable.PlayerRow = newRow;
                _drawable.PlayerCol = newCol;
                _drawable.Trail.Add((newRow, newCol));
                _moves++;
                MovesLabel.Text = $"Moves: {_moves}";
                MazeCanvas.Invalidate();
                _isMoving = false;
                CheckWin();
                tcs.TrySetResult();
            });

        await tcs.Task;
    }

    // ── Win check ───────────────────────────────────────────────────────────
    private async void CheckWin()
    {
        if (_maze == null || _gameEnded) return;
        if (_playerRow != _maze.End.r || _playerCol != _maze.End.c) return;

        _gameEnded = true;
        StopTimers();

        int elapsed = (int)(DateTime.Now - _startTime).TotalSeconds;
        int stars = ProgressService.CalcStars(elapsed, _totalSec);
        int coins = stars switch { 3 => 5, 2 => 3, 1 => 1, _ => 0 };

        await DoWinAnimation();
        await Task.Delay(100);
        _ = ResultPopup.Show(true, _level, elapsed, _totalSec, stars, coins, gameId: "mazerunner");
    }

    private async Task DoWinAnimation()
    {
        try
        {
            // Simple fade out of destination, then short delay
            DestinationImage.IsVisible = false;
            await Task.Delay(200);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DoWinAnimation: {ex.Message}");
        }
    }

    private async void CreateSparkles()
    {
        try
        {
            int count = 12;
            float px = (float)(PlayerImage.TranslationX + PlayerImage.WidthRequest / 2.0);
            float py = (float)(PlayerImage.TranslationY + PlayerImage.HeightRequest / 2.0);
            var rng = new Random();

            for (int i = 0; i < count; i++)
            {
                var sparkle = new Image
                {
                    Source = "star_kawaii_filled.svg",
                    WidthRequest = 16 + i * 2,
                    HeightRequest = 16 + i * 2,
                    Opacity = 0,
                    InputTransparent = true,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start,
                    Rotation = rng.Next(0, 360)
                };

                double rx = rng.NextDouble() * 80 - 40;
                double ry = rng.NextDouble() * 80 - 40;

                sparkle.TranslationX = px + rx;
                sparkle.TranslationY = py + ry;
                MazeOverlay.Children.Add(sparkle);

                double delay = rng.NextDouble() * 400;
                _ = Task.Run(async () =>
                {
                    await Task.Delay((int)delay);
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await sparkle.ScaleToAsync(1.5 + rng.NextDouble(), 250, Easing.SpringOut);
                        await sparkle.FadeToAsync(1, 150, Easing.CubicOut);
                        await Task.WhenAll(
                            sparkle.ScaleToAsync(0.3, 350, Easing.CubicIn),
                            sparkle.FadeToAsync(0, 350, Easing.CubicIn)
                        );
                        MazeOverlay.Children.Remove(sparkle);
                    });
                });
            }
        }
        catch
        {
        }
    }

    private void EndGame(bool win)
    {
        if (_gameEnded) return;
        _gameEnded = true;
        StopTimers();
        _ = ResultPopup.Show(win, _level, _totalSec, _totalSec, gameId: "mazerunner");
    }

    // ── UI helpers ──────────────────────────────────────────────────────────
    private void UpdateTimerUI()
    {
        MazeTimer.TotalSeconds = _totalSec;
        MazeTimer.SecondsRemaining = _remainSec;
    }

    // ── Navigation ──────────────────────────────────────────────────────────
    private void OnNextLevel(object? sender, EventArgs e)
    {
        _level++;
        StartLevel();
    }

    private void OnRetry(object? sender, EventArgs e) => StartLevel();

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        try
        {
            bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave",
                "Stay");
            if (!leave) return;
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnBackClicked: {ex.Message}");
        }
    }
}