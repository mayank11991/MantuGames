using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.Views.Controls;
using Microsoft.Maui.Graphics;

namespace MantuGames.Views;

// ── Drawable ────────────────────────────────────────────────────────────────
internal sealed class MazeDrawable : IDrawable
{
    public MazePuzzle? Maze      { get; set; }
    public float       PlayerRow { get; set; }   // float for smooth animation
    public float       PlayerCol { get; set; }
    public HashSet<(int r, int c)> Trail { get; } = new();
    public float       PlayerBob  { get; set; }  // subtle up-down oscillation

    public float CellSize { get; private set; }
    public float OffsetX  { get; private set; }
    public float OffsetY  { get; private set; }

    // Colors (amber/orange palette)
    private static readonly Color WallColor  = Color.FromArgb("#E65100");
    private static readonly Color FloorColor = ThemeHelper.IsDarkMode? Color.FromArgb("#1F1E1E"): Color.FromArgb("#FFF3E0");
    private static readonly Color TrailColor = Color.FromArgb("#FFB74D");
    private static readonly Color ExitBg     = Color.FromArgb("#FF8F00");

    public void Draw(ICanvas canvas, RectF bounds)
    {
        if (Maze == null) return;
       
        var (ox, oy, cs) = Layout(bounds);

        // ── Floor cells (rounded) ───────────────────────────────────
        canvas.FillColor = FloorColor;
        float corner = cs * 0.18f;
        for (int r = 0; r < Maze.Rows; r++)
        for (int c = 0; c < Maze.Cols; c++)
            canvas.FillRoundedRectangle(
                ox + c * cs + 1, oy + r * cs + 1,
                cs - 2, cs - 2, corner);

        // ── Breadcrumb trail ────────────────────────────────────────
        canvas.FillColor = TrailColor;
        float tp = cs * 0.22f;
        foreach (var (tr, tc) in Trail)
            canvas.FillRoundedRectangle(
                ox + tc * cs + tp, oy + tr * cs + tp,
                cs - tp * 2, cs - tp * 2, cs * 0.18f);

        // ── Exit (rendered as DestinationImage overlay) ─────────────

        // ── Walls ───────────────────────────────────────────────────
        float wt = Math.Max(3f, cs * 0.10f);
        canvas.StrokeColor   = WallColor;
        canvas.StrokeSize    = wt;
        canvas.StrokeLineCap = LineCap.Round;

        for (int r = 0; r < Maze.Rows; r++)
        for (int c = 0; c < Maze.Cols; c++)
        {
            float x = ox + c * cs, y = oy + r * cs;
            var cell = Maze.Cells[r, c];
            if (cell.WallTop)    canvas.DrawLine(x,      y,      x + cs, y);
            if (cell.WallRight)  canvas.DrawLine(x + cs, y,      x + cs, y + cs);
            if (cell.WallBottom) canvas.DrawLine(x,      y + cs, x + cs, y + cs);
            if (cell.WallLeft)   canvas.DrawLine(x,      y,      x,      y + cs);
        }

        // ── Player highlight (image rendered on overlay) ─────────────
        // Only draw a subtle glow beneath the player image
        float px = ox + PlayerCol * cs + cs / 2f;
        float py = oy + PlayerRow * cs + cs / 2f + PlayerBob;
        float pr = cs * 0.38f;

        canvas.FillColor = Color.FromArgb("#22B388FF");
        canvas.FillCircle(px, py + 2, pr * 1.1f);
    }

    public (float ox, float oy, float cs) Layout(RectF bounds)
    {
        if (Maze == null) return (0, 0, 0);
        CellSize = Math.Min(bounds.Width / Maze.Cols, bounds.Height / Maze.Rows);
        OffsetX  = (bounds.Width  - CellSize * Maze.Cols) / 2f;
        OffsetY  = (bounds.Height - CellSize * Maze.Rows) / 2f;
        return (OffsetX, OffsetY, CellSize);
    }
}

// ── Page ─────────────────────────────────────────────────────────────────────
[QueryProperty(nameof(Level), "level")]
public partial class MazeRunnerPage : ContentPage
{
    private int _level = 1;

    private MazePuzzle?       _maze;
    private MazeDrawable      _drawable = new();
    private int               _playerRow;
    private int               _playerCol;
    private int               _moves;
    private bool              _isMoving;
    private bool              _gameEnded;
    private int               _totalSec;
    private int               _remainSec;
    private IDispatcherTimer? _gameTimer;
    private IDispatcherTimer? _animTimer;
    private DateTime          _startTime;
    private float             _bobPhase;

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
        this.FadeTo(1, 350);
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
        _maze       = MazePuzzle.ForLevel(_level);
        _playerRow  = 0;
        _playerCol  = 0;
        _moves      = 0;
        _gameEnded  = false;
        _totalSec   = ProgressService.GetTimerSeconds(_level);
        _remainSec  = _totalSec;
        _startTime  = DateTime.Now;

        LevelLabel.Text = $"Level {_level}  ({_maze.Rows}×{_maze.Cols})";
        MovesLabel.Text = "Moves: 0";

        _drawable = new MazeDrawable
        {
            Maze      = _maze,
            PlayerRow = 0f,
            PlayerCol = 0f,
        };
        _drawable.Trail.Add((0, 0));
        MazeCanvas.Drawable = _drawable;

        UpdateTimerUI();
        ResultPopup.Hide();
        StopTimers();
        StartTimers();
    }

    // ── Pause / Resume ──────────────────────────────────────────────────────
    private void OnPause(object sender, EventArgs e)
    {
        _gameTimer?.Stop();
        _animTimer?.Stop();
        PauseOverlay.Show();
    }

    private void OnResumeGame(object sender, EventArgs e)
    {
        _gameTimer?.Start();
        _animTimer?.Start();
    }

    private void OnRulesClicked(object sender, EventArgs e)
    {
        RulesPopup.Show(GameRules.GetRules("mazerunner"));
    }

    // ── Timers ──────────────────────────────────────────────────────────────
    private void StartTimers()
    {
        _gameTimer          = Dispatcher.CreateTimer();
        _gameTimer.Interval = TimeSpan.FromSeconds(1);
        _gameTimer.Tick    += OnGameTick;
        _gameTimer.Start();

        _animTimer          = Dispatcher.CreateTimer();
        _animTimer.Interval = TimeSpan.FromMilliseconds(16);
        _animTimer.Tick    += OnAnimTick;
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
        _bobPhase            += 0.07f;
        _drawable.PlayerBob   = MathF.Sin(_bobPhase) * 2.5f;
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

        float imgHalf = cs * 0.38f;
        PlayerImage.WidthRequest  = cs * 0.76f;
        PlayerImage.HeightRequest = cs * 0.76f;
        PlayerImage.TranslationX  = px - imgHalf;
        PlayerImage.TranslationY  = py - imgHalf;

        float dstHalf = cs * 0.44f;
        float dx = ox + (_maze.Cols - 1) * cs + cs / 2f;
        float dy = oy + (_maze.Rows - 1) * cs + cs / 2f;
        DestinationImage.WidthRequest  = cs * 0.88f;
        DestinationImage.HeightRequest = cs * 0.88f;
        DestinationImage.TranslationX  = dx - dstHalf;
        DestinationImage.TranslationY  = dy - dstHalf;
    }

    // ── D-pad handlers ──────────────────────────────────────────────────────
    private void OnMoveUp   (object s, TappedEventArgs e) => _ = TryMove(-1,  0);
    private void OnMoveDown (object s, TappedEventArgs e) => _ = TryMove( 1,  0);
    private void OnMoveLeft (object s, TappedEventArgs e) => _ = TryMove( 0, -1);
    private void OnMoveRight(object s, TappedEventArgs e) => _ = TryMove( 0,  1);

    // ── Pan handler (distance-proportional movement) ───────────────────────
    private double _panTotalX, _panTotalY;
    private void OnPanMaze(object s, PanUpdatedEventArgs e)
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
        if (_maze == null) return 0;
        float w = (float)MazeCanvas.Width;
        float h = (float)MazeCanvas.Height;
        if (w <= 0 || h <= 0) return 0;
        return Math.Min(w / _maze.Cols, h / _maze.Rows);
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

        var cell = _maze.Cells[_playerRow, _playerCol];
        bool canMove = (dr == -1 && !cell.WallTop)
                    || (dr ==  1 && !cell.WallBottom)
                    || (dc == -1 && !cell.WallLeft)
                    || (dc ==  1 && !cell.WallRight);

        if (!canMove)
        {
            if (!showBump) return;
            _isMoving = true;
            AudioService.Instance.Play("bump");
            await MazeCanvas.TranslateTo(dc * 6, dr * 6, 60, Easing.CubicOut);
            await MazeCanvas.TranslateTo(0,       0,     80, Easing.SpringOut);
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
        int stars   = ProgressService.CalcStars(elapsed, _totalSec);
        int pts     = ProgressService.CalcPoints(stars, elapsed, _totalSec);

        await DoWinAnimation();
        await Task.Delay(600);
        _ = ResultPopup.Show(true, _level, elapsed, _totalSec, stars, pts, gameId: "mazerunner");
    }

    private async Task DoWinAnimation()
    {
        try
        {
            for (int i = 0; i < 3; i++)
            {
                await MazeCanvas.ScaleTo(1.06, 100, Easing.SpringOut);
                await MazeCanvas.ScaleTo(1.00, 80, Easing.SpringIn);
            }

            await Task.WhenAll(
                DestinationImage.FadeTo(0, 350, Easing.CubicIn),
                DestinationImage.ScaleTo(0.2, 350, Easing.CubicIn)
            );

            CreateSparkles();

            await Task.Delay(300);

            for (int i = 0; i < 4; i++)
            {
                await PlayerImage.ScaleTo(1.5, 130, Easing.SpringOut);
                await PlayerImage.ScaleTo(0.8, 130, Easing.SpringIn);
            }
            await PlayerImage.ScaleTo(1.0, 100, Easing.SpringOut);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in DoWinAnimation: {ex.Message}");
        }
    }

    private async void CreateSparkles()
    {
        try
        {
            string[] icons = { "✨", "⭐", "🌟", "💫" };
            int count = 12;
            float px = (float)(PlayerImage.TranslationX + PlayerImage.WidthRequest / 2.0);
            float py = (float)(PlayerImage.TranslationY + PlayerImage.HeightRequest / 2.0);
            var rng = new Random();

            for (int i = 0; i < count; i++)
            {
                var sparkle = new Label
                {
                    Text = icons[i % icons.Length],
                    FontSize = 14 + i * 2,
                    Opacity = 0,
                    InputTransparent = true,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start
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
                        await sparkle.ScaleTo(1.5 + rng.NextDouble(), 250, Easing.SpringOut);
                        await sparkle.FadeTo(1, 150, Easing.CubicOut);
                        await Task.WhenAll(
                            sparkle.ScaleTo(0.3, 350, Easing.CubicIn),
                            sparkle.FadeTo(0, 350, Easing.CubicIn)
                        );
                        MazeOverlay.Children.Remove(sparkle);
                    });
                });
            }
        }
        catch { }
    }

    private void EndGame(bool win)
    {
        if (_gameEnded) return;
        _gameEnded = true;
        StopTimers();
        _ = ResultPopup.Show(win, _level, _totalSec, _totalSec);
    }

    // ── UI helpers ──────────────────────────────────────────────────────────
    private void UpdateTimerUI()
    {
        TimerLabel.Text   = $"⏱ {_remainSec / 60}:{_remainSec % 60:D2}";
        TimerBar.Progress = (double)_remainSec / _totalSec;
        TimerLabel.TextColor = _remainSec <= 15
            ? Color.FromArgb("#FF5252")
            : Colors.White;
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
}
