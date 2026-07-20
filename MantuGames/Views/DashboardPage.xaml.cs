using MantuGames.Helpers;
using MantuGames.Services;
using MantuGames.ViewModels;
using MantuGames.Views.Controls;

namespace MantuGames.Views;

internal sealed class BgLinesDrawable : IDrawable
{
    public float Phase { get; set; }

    private static readonly Color LineColor = Color.FromArgb("#FFE9C8");

    private static readonly (float x1, float y1, float x2, float y2, float w)[] LinesDown =
    {
        (0f, 0.05f, 0.5f, 0.12f, 1.5f),
        (0.5f, 0.12f, 1f, 0.08f, 1f),
        (0f, 0.18f, 1f, 0.25f, 1.5f),
        (0f, 0.30f, 0.6f, 0.22f, 1f),
        (0.6f, 0.22f, 1f, 0.35f, 1.5f),
        (0f, 0.38f, 1f, 0.42f, 1f),
        (0f, 0.48f, 0.4f, 0.55f, 2f),
        (0.4f, 0.55f, 1f, 0.50f, 1f),
        (0f, 0.58f, 0.7f, 0.65f, 1.5f),
        (0.7f, 0.65f, 1f, 0.60f, 1f),
        (0f, 0.68f, 1f, 0.75f, 1.5f),
        (0f, 0.78f, 0.5f, 0.85f, 1f),
        (0.5f, 0.85f, 1f, 0.80f, 1.5f),
        (0f, 0.88f, 1f, 0.92f, 1f),
        (0f, 0.95f, 0.6f, 1f, 1.5f),
        (0.6f, 1f, 1f, 0.96f, 1f),
    };

    private static readonly (float x1, float y1, float x2, float y2, float w)[] LinesUp;

    static BgLinesDrawable()
    {
        LinesUp = new (float, float, float, float, float)[LinesDown.Length];
        for (int i = 0; i < LinesDown.Length; i++)
        {
            var (x1, y1, x2, y2, w) = LinesDown[i];
            LinesUp[i] = (x1, 1f - y1, x2, 1f - y2, w);
        }
    }

    public void Draw(ICanvas canvas, RectF bounds)
    {
        float downOffset = Phase % bounds.Height;
        float upOffset = -Phase % bounds.Height;
        if (upOffset < 0) upOffset += bounds.Height;

        DrawLines(canvas, bounds, LinesDown, downOffset);
        DrawLines(canvas, bounds, LinesUp, upOffset);
    }

    private static void DrawLines(ICanvas canvas, RectF bounds,
        (float x1, float y1, float x2, float y2, float w)[] lines, float offset)
    {
        foreach (var (x1n, y1n, x2n, y2n, w) in lines)
        {
            canvas.StrokeColor = LineColor.WithAlpha(0.18f);
            canvas.StrokeSize = w;

            float y1 = y1n * bounds.Height + offset;
            float y2 = y2n * bounds.Height + offset;

            if (y1 > bounds.Height && y2 > bounds.Height)
            {
                y1 -= bounds.Height;
                y2 -= bounds.Height;
            }

            if (y1 < 0 && y2 < 0)
            {
                y1 += bounds.Height;
                y2 += bounds.Height;
            }

            canvas.DrawLine(
                x1n * bounds.Width, y1,
                x2n * bounds.Width, y2);

            canvas.DrawLine(
                x1n * bounds.Width, y1 - bounds.Height,
                x2n * bounds.Width, y2 - bounds.Height);
        }
    }
}

public partial class DashboardPage : ContentPage
{
   // private readonly BgLinesDrawable _bgDrawable = new();
    private IDispatcherTimer _bgAnimTimer;

    private const int GameRows = 4; // 8 games / 2 columns
    private const double RowSpacing = 1; // matches GridItemsLayout.VerticalItemSpacing
    private const double MinCardHeight = 48;

    private double _cardImageHeight = 130;
    public double CardImageHeight
    {
        get => _cardImageHeight;
        set
        {
            if (Math.Abs(_cardImageHeight - value) < 0.5) return;
            _cardImageHeight = value;
            OnPropertyChanged();
        }
    }

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardViewModel();
        this.AddBannerAd();
        SettingsPopup.EditProfileRequested += OnEditProfile;
        GamesCollectionView.SizeChanged += OnGamesGridSizeChanged;
    }

    private void OnGamesGridSizeChanged(object sender, EventArgs e)
    {
        double available = GamesCollectionView.Height;
        if (available <= 0) return;

        double rowHeight = (available - (GameRows - 1) * RowSpacing) / GameRows;
        CardImageHeight = Math.Max(MinCardHeight, rowHeight);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
       // BgCanvas.Drawable = _bgDrawable;
        _bgAnimTimer = Dispatcher.CreateTimer();
        _bgAnimTimer.Interval = TimeSpan.FromMilliseconds(30);
        _bgAnimTimer.Tick += (s, e) =>
        {
           // _bgDrawable.Phase += 0.3f;
            BgCanvas.Invalidate();
        };
        _bgAnimTimer.Start();

        // Show profile popup on first launch
        if (!Preferences.Get("profile_setup_done", false))
        {
            ProfilePopup.Show();
        }

        SettingsPopup.ProgressReset += OnProgressReset;
        RefreshPoints();

        // Show player name and total points
        string name = Preferences.Get("player_name", "");
        PlayerNameLabel.Text = string.IsNullOrEmpty(name) ? "" : name;

        // Start background music
        AudioService.Instance.StartMusic();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        SettingsPopup.ProgressReset -= OnProgressReset;
        _bgAnimTimer?.Stop();
        AudioService.Instance.StopMusic();
    }

    private void RefreshPoints()
    {
        int totalPoints = 0;
        var games = DashboardViewModel.Games;
        foreach (var g in games)
            totalPoints += ProgressService.Instance.GetTotalPoints(g.Id);
        PointsLabel.Text = totalPoints > 0 ? $"{totalPoints} pts" : "";
    }

    private void OnProgressReset(object sender, EventArgs e)
    {
        RefreshPoints();
    }

    private void OnEditProfile(object sender, EventArgs e)
    {
        ProfilePopup.Show();
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        AudioService.Instance.Play("tap");
        VibrationHelper.Click();
        SettingsPopup.Show();
    }
}