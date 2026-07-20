using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class ProfilePopup : ContentView
{
    private bool _submitted;
    private int _day = 1;
    private int _month = 1;
    private int _year;

    private static readonly string[] Months =
        { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
          "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

    public ProfilePopup()
    {
        InitializeComponent();
        LoadSaved();
        UpdateLabels();
    }

    private void LoadSaved()
    {
        string dob = Preferences.Get("player_dob", "");
        if (!string.IsNullOrEmpty(dob))
        {
            try
            {
                var dt = DateTime.Parse(dob);
                _day = dt.Day;
                _month = dt.Month;
                _year = dt.Year;
                return;
            }
            catch { }
        }
        _year = DateTime.Now.Year - 10;
        _month = 1;
        _day = 1;
    }

    private void UpdateLabels()
    {
        DayLabel.Text = _day.ToString();
        MonthLabel.Text = Months[_month - 1];
        YearLabel.Text = _year.ToString();
    }

    private int DaysInMonth() => DateTime.DaysInMonth(_year, _month);

    // ── Day taps ──
    private void OnDayUpTap(object sender, TappedEventArgs e)
    {
        int max = DaysInMonth();
        _day = _day >= max ? 1 : _day + 1;
        UpdateLabels();
    }

    private void OnDayDownTap(object sender, TappedEventArgs e)
    {
        int max = DaysInMonth();
        _day = _day <= 1 ? max : _day - 1;
        UpdateLabels();
    }

    // ── Month taps ──
    private void OnMonthUpTap(object sender, TappedEventArgs e)
    {
        _month = _month >= 12 ? 1 : _month + 1;
        ClampDay();
        UpdateLabels();
    }

    private void OnMonthDownTap(object sender, TappedEventArgs e)
    {
        _month = _month <= 1 ? 12 : _month - 1;
        ClampDay();
        UpdateLabels();
    }

    // ── Year taps ──
    private void OnYearUpTap(object sender, TappedEventArgs e)
    {
        _year = Math.Min(DateTime.Now.Year, _year + 1);
        ClampDay();
        UpdateLabels();
    }

    private void OnYearDownTap(object sender, TappedEventArgs e)
    {
        _year = Math.Max(DateTime.Now.Year - 80, _year - 1);
        ClampDay();
        UpdateLabels();
    }

    private void ClampDay()
    {
        int max = DaysInMonth();
        if (_day > max) _day = max;
    }

    public void Show()
    {
        _submitted = false;
        LoadSaved();
        NameEntry.Text = Preferences.Get("player_name", "");
        NameEntry.PlaceholderColor = Color.FromArgb("#DCC8A8");
        UpdateLabels();
        IsVisible = true;
        Opacity = 0;
        Scale = 0.8;
        Overlay.IsVisible = true;
        this.FadeTo(1, 300, Easing.CubicOut);
        this.ScaleTo(1, 300, Easing.SpringOut);
    }

    public void Hide()
    {
        this.FadeTo(0, 200, Easing.CubicIn);
        this.ScaleTo(0.8, 200, Easing.CubicIn).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Overlay.IsVisible = false;
                IsVisible = false;
            });
        });
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e)
    {
        // Consume tap to prevent passing through
    }

    private void OnSkip(object sender, TappedEventArgs e)
    {
        _submitted = true;
        Preferences.Set("profile_setup_done", true);
        Hide();
    }

    private void OnSubmit(object sender, TappedEventArgs e)
    {
        if (_submitted) return;
        string name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            NameEntry.PlaceholderColor = Color.FromArgb("#C0392B");
            return;
        }

        _submitted = true;

        Preferences.Set("player_name", name);
        Preferences.Set("player_dob", $"{Months[_month - 1]} {_day}, {_year}");
        Preferences.Set("profile_setup_done", true);

        AudioService.Instance.Play("win");
        Hide();
    }
}
