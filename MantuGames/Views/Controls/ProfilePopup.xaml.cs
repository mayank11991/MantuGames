using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.Views.Controls;

public partial class ProfilePopup : ContentView
{
    private bool _submitted;
    private int _day = 1;
    private int _month = 1;
    private int _year;

    public event EventHandler<PlayerProfile>? ProfileCreated;

    private static readonly string[] Months =
        { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
          "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

    public ProfilePopup()
    {
        InitializeComponent();
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        DayLabel.Text = _day.ToString();
        MonthLabel.Text = Months[_month - 1];
        YearLabel.Text = _year.ToString();
    }

    private int DaysInMonth() => DateTime.DaysInMonth(_year, _month);

    // ── Swipe spinners: swipe up = increase, swipe down = decrease ──
    private float _dayPanY, _monthPanY, _yearPanY;

    private void OnDayPan(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started: _dayPanY = 0; break;
            case GestureStatus.Running: _dayPanY = (float)e.TotalY; break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                StepDay((int)Math.Round(-_dayPanY / 60.0));
                break;
        }
    }

    private void OnMonthPan(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started: _monthPanY = 0; break;
            case GestureStatus.Running: _monthPanY = (float)e.TotalY; break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                StepMonth((int)Math.Round(-_monthPanY / 60.0));
                break;
        }
    }

    private void OnYearPan(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started: _yearPanY = 0; break;
            case GestureStatus.Running: _yearPanY = (float)e.TotalY; break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                StepYear((int)Math.Round(-_yearPanY / 60.0));
                break;
        }
    }

    private void StepDay(int delta)
    {
        if (delta == 0) return;
        int max = DaysInMonth();
        _day = ((_day - 1 + delta) % max + max) % max + 1;
        UpdateLabels();
    }

    private void StepMonth(int delta)
    {
        if (delta == 0) return;
        _month = ((_month - 1 + delta) % 12 + 12) % 12 + 1;
        ClampDay();
        UpdateLabels();
    }

    private void StepYear(int delta)
    {
        if (delta == 0) return;
        _year = Math.Max(DateTime.Now.Year - 80,
                         Math.Min(DateTime.Now.Year, _year + delta));
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
        _year = DateTime.Now.Year - 10;
        _month = 1;
        _day = 1;
        NameEntry.Text = "";
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

        var profile = ProfileService.AddProfile(name, $"{Months[_month - 1]} {_day}, {_year}");

        AudioService.Instance.Play("win");
        Hide();
        ProfileCreated?.Invoke(this, profile);
    }
}
