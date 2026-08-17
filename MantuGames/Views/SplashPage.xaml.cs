using MantuGames.Services;

namespace MantuGames.Views;

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
                X = cx,
                Y = cy,
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
            if (p.Life <= 0)
            {
                _particles.RemoveAt(i);
                continue;
            }

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

public partial class SplashPage : ContentPage
{
    private readonly DustEffect _dust = new();
    private IDispatcherTimer _dustTimer;

    public SplashPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async Task AnimateSlothAsync()
    {
        try
        {
            await Task.Delay(150);
            SlothImage.Opacity = 1;
            SlothImage.TranslationY = -Root.Height;
            AudioService.Instance.Play("whoosh");
            await Task.WhenAll(
                SlothImage.TranslateTo(0, 0, 1500, Easing.BounceOut),
                SlothImage.FadeTo(1, 400, Easing.CubicOut));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AnimateSlothAsync: {ex.Message}");
        }
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            Loaded -= OnLoaded;

            _ = IPlatformApplication.Current!.Services.GetRequiredService<MantuGames.Services.AudioService>()
                .PreloadAsync();

            DustCanvas.Drawable = _dust;
            _dustTimer = Dispatcher.CreateTimer();
            _dustTimer.Interval = TimeSpan.FromMilliseconds(16);
            _dustTimer.Tick += (s, args) => DustCanvas.Invalidate();
            _dustTimer.Start();

            // Sloth scrolls down from the top of the screen to sit at the
            // bottom of the GAMINGS text, AFTER the letter animations finish.

            string text = "MANTU";
            var charLabels = new List<Label>();

            foreach (char c in text)
            {
                var lbl = new Label
                {
                    Text = c.ToString(),
                    FontFamily = "Orbitron",
                    FontSize = 44,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#E7ECF5"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TranslationY = -400,
                    Opacity = 0,
                };
                MantuRow.Children.Add(lbl);
                charLabels.Add(lbl);
            }

            for (int i = 0; i < charLabels.Count; i++)
            {
                await Task.Delay(100);
                charLabels[i].Opacity = 1;
                AudioService.Instance.Play("thud");
                await charLabels[i].TranslateTo(0, 0, 250, Easing.CubicOut);

                var cb = charLabels[i].Bounds;
                double charCenterX = MantuRow.Bounds.X + cb.X + cb.Width / 2 + CenterStack.Bounds.X;
                double cy = MantuRow.Bounds.Y + cb.Y + cb.Height / 2 + CenterStack.Bounds.Y;
                _dust.Burst((float)charCenterX, (float)cy);
            }

            await Task.Delay(300);

            GamingsLabel.TranslationX = 0;
            AudioService.Instance.Play("whoosh");
            GamingsLabel.TranslationY = 80;
            GamingsLabel.Opacity = 1;
            await GamingsLabel.TranslateTo(0, 0, 400, Easing.CubicOut);

            // Sloth arrives only once the GAMINGS animation has finished.
            await AnimateSlothAsync();

            // Wait for dust to settle
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
            System.Diagnostics.Debug.WriteLine($"Error in OnLoaded: {ex.Message}");
        }
    }
}