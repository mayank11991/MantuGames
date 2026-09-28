using MantuGames.Services;

namespace MantuGames.Views;

// ── DUST EFFECT ─────────────────────────────────────────────────────────
internal sealed class DustEffect : IDrawable
{
    private readonly List<DustParticle> _particles = new();
    private static readonly Random _rng = new();

    public void Burst(float cx, float cy, int count = 12)
    {
        for (int i = 0; i < count; i++)
        {
            double angle = _rng.NextDouble() * Math.PI * 2;
            double speed = 30 + _rng.NextDouble() * 80;
            _particles.Add(new DustParticle
            {
                X = cx, Y = cy,
                Vx = (float)(Math.Cos(angle) * speed),
                Vy = (float)(Math.Sin(angle) * speed) - 20,
                Life = 0.4f + (float)_rng.NextDouble() * 0.3f,
                Size = 2f + (float)_rng.NextDouble() * 2.5f,
            });
        }
    }

    public void Draw(ICanvas canvas, RectF bounds)
    {
        float dt = 0.016f;
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Life -= dt;
            if (p.Life <= 0) { _particles.RemoveAt(i); continue; }
            p.X += p.Vx * dt;
            p.Y += p.Vy * dt;
            p.Vy += 120 * dt;
            float alpha = Math.Clamp(p.Life / 0.5f, 0, 1);
            canvas.FillColor = Color.FromArgb("#ffffff").WithAlpha(alpha * 0.7f);
            canvas.FillCircle(p.X, p.Y, p.Size * (0.5f + p.Life * 0.5f));
            canvas.FillColor = Color.FromArgb("#ffffff").WithAlpha(alpha * 0.4f);
            canvas.FillCircle(p.X - 1, p.Y - 1, p.Size * 0.5f * (0.5f + p.Life * 0.5f));
        }
        _particles.RemoveAll(p => p.Life <= 0);
    }

    public bool IsEmpty => _particles.Count == 0;

    private class DustParticle
    {
        public float X, Y, Vx, Vy, Life, Size;
    }
}

// ── SPLASH PAGE ─────────────────────────────────────────────────────────
public partial class SplashPage : ContentPage
{
    private readonly DustEffect _dust = new();
    private IDispatcherTimer _dustTimer;
    private Label[] MantuLetters;

    public SplashPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async Task AnimateHammerAsync()
    {
        try
        {
            await Task.Delay(150);
            SlothImage.Opacity = 1;
            SlothImage.TranslationY = -Root.Height;
            AudioService.Instance.Play("whoosh");

            await SlothImage.TranslateTo(0, 0, 800, Easing.CubicIn);

            AudioService.Instance.Play("thud");
            await ShakeScreenAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AnimateHammerAsync: {ex.Message}");
        }
    }

    private async Task ShakeScreenAsync()
    {
        int shakeCount = 6;
        double shakeMagnitude = 8;

        for (int i = 0; i < shakeCount; i++)
        {
            double offsetX = (i % 2 == 0 ? 1 : -1) * shakeMagnitude * (1.0 - (double)i / shakeCount);
            await Root.TranslateTo(offsetX, 0, 30, Easing.Linear);
        }
        await Root.TranslateTo(0, 0, 30, Easing.Linear);
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            Loaded -= OnLoaded;

            _ = IPlatformApplication.Current!.Services.GetRequiredService<AudioService>()
                .PreloadAsync();

            MantuLetters = [L0, L1, L2, L3, L4];

            DustCanvas.Drawable = _dust;
            _dustTimer = Dispatcher.CreateTimer();
            _dustTimer.Interval = TimeSpan.FromMilliseconds(16);
            _dustTimer.Tick += (s, args) => DustCanvas.Invalidate();
            _dustTimer.Start();

            // Hide all letters
            foreach (var lbl in MantuLetters)
            {
                lbl.Opacity = 0;
                lbl.TranslationY = -300;
            }

            await Task.Delay(100);

            // ── PHASE 1: Letters drop one by one ──────────────────────
            for (int i = 0; i < MantuLetters.Length; i++)
            {
                await Task.Delay(100);
                MantuLetters[i].Opacity = 1;
                AudioService.Instance.Play("thud");
                await MantuLetters[i].TranslateTo(0, 0, 250, Easing.CubicOut);

                double screenX = MantuRow.Bounds.X + MantuLetters[i].X + MantuLetters[i].Bounds.Width / 2 + CenterStack.Bounds.X;
                double screenY = MantuRow.Bounds.Y + MantuLetters[i].Y + MantuLetters[i].Bounds.Height / 2 + CenterStack.Bounds.Y;
                _dust.Burst((float)screenX, (float)screenY);
            }

            await Task.Delay(300);

            // ── PHASE 2: GAMINGS drops in from below ──────────────────
            AudioService.Instance.Play("thud");
            GamingsLabel.TranslationX = 0;
            GamingsLabel.TranslationY = 80;
            GamingsLabel.Opacity = 1;
            await GamingsLabel.TranslateTo(0, 0, 400, Easing.CubicOut);

            // ── PHASE 3: Hammer drops below GAMINGS ───────────────────
            await AnimateHammerAsync();

            // ── PHASE 4: Wait for dust, then fade ─────────────────────
            while (!_dust.IsEmpty)
                await Task.Delay(50);

            _dustTimer?.Stop();
            await Task.Delay(1800);

            await MantuRow.FadeTo(0, 200, Easing.CubicIn);
            await GamingsLabel.FadeTo(0, 200, Easing.CubicIn);
            await SlothImage.FadeTo(0, 200, Easing.CubicIn);
            Application.Current!.Windows[0].Page = new AppShell();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnLoaded: {ex.Message}");
        }
    }
}
