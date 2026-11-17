using MantuGames.Services;

namespace MantuGames.Views.Controls;

/// <summary>
/// A short confetti burst drawn on a GraphicsView. Call <see cref="Burst"/>; it animates
/// for about two seconds and then clears itself. Colours follow the skin's palette.
/// </summary>
public class ConfettiView : GraphicsView, IDrawable
{
    private sealed class Piece
    {
        public float X, Y, Vx, Vy, Angle, Spin, W, H;
        public Color Color = Colors.White;
        public bool Round;
    }

    private static readonly string[] DefaultColors = { "#FF4F6D", "#FFC83D", "#3DC86A", "#2F8CFF", "#B07CFF" };

    private readonly List<Piece> _pieces = new();
    private readonly Random _rng = new();
    private IDispatcherTimer? _timer;
    private DateTime _start;
    private const double DurationSec = 2.2;

    public ConfettiView()
    {
        Drawable = this;
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
    }

    /// <summary>Starts a burst from the top centre. <paramref name="amount"/> scales the piece count.</summary>
    public void Burst(double amount = 1.0)
    {
        float w = (float)Width, h = (float)Height;
        if (w <= 0 || h <= 0) return;

        var colours = DefaultColors.Select((hex, i) => ThemeColors.Get(i, hex)).ToArray();
        _pieces.Clear();
        int n = (int)(90 * amount);
        for (int i = 0; i < n; i++)
        {
            _pieces.Add(new Piece
            {
                X = w / 2 + (float)(_rng.NextDouble() - 0.5) * w * 0.3f,
                Y = h * 0.22f,
                Vx = (float)(_rng.NextDouble() - 0.5) * w * 1.3f,
                Vy = -(float)(_rng.NextDouble() * h * 0.9 + h * 0.25),
                Angle = (float)(_rng.NextDouble() * 360),
                Spin = (float)(_rng.NextDouble() - 0.5) * 720,
                W = (float)(_rng.NextDouble() * 6 + 6),
                H = (float)(_rng.NextDouble() * 4 + 4),
                Color = colours[_rng.Next(colours.Length)],
                Round = _rng.NextDouble() < 0.3,
            });
        }

        _start = DateTime.UtcNow;
        _last = _start;
        _timer ??= Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private DateTime _last;

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        float dt = (float)Math.Min((now - _last).TotalSeconds, 0.05);
        _last = now;
        float gravity = (float)Height * 1.6f;
        foreach (var p in _pieces)
        {
            p.Vy += gravity * dt;
            p.Vx *= 0.99f;
            p.X += p.Vx * dt;
            p.Y += p.Vy * dt;
            p.Angle += p.Spin * dt;
        }
        if ((now - _start).TotalSeconds > DurationSec)
        {
            _timer?.Stop();
            _pieces.Clear();
        }
        Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        double t = (DateTime.UtcNow - _start).TotalSeconds;
        float alpha = (float)Math.Clamp((DurationSec - t) / 0.6, 0, 1);
        foreach (var p in _pieces)
        {
            canvas.SaveState();
            canvas.Translate(p.X, p.Y);
            canvas.Rotate(p.Angle);
            canvas.FillColor = p.Color.WithAlpha(alpha);
            if (p.Round) canvas.FillCircle(0, 0, p.H / 2 + 1);
            else canvas.FillRectangle(-p.W / 2, -p.H / 2, p.W, p.H);
            canvas.RestoreState();
        }
    }
}
