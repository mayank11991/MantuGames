using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MantuGames.Views.Controls;

public partial class GameTimerView : ContentView
{
    // ── Bindable Properties ──────────────────────────────────────

    public static readonly BindableProperty TotalSecondsProperty =
        BindableProperty.Create(nameof(TotalSeconds), typeof(int), typeof(GameTimerView), 210);

    public static readonly BindableProperty SecondsRemainingProperty =
        BindableProperty.Create(nameof(SecondsRemaining), typeof(int), typeof(GameTimerView), 210,
            propertyChanged: (b, o, n) => ((GameTimerView)b).Update());

    public int TotalSeconds
    {
        get => (int)GetValue(TotalSecondsProperty);
        set => SetValue(TotalSecondsProperty, value);
    }

    public int SecondsRemaining
    {
        get => (int)GetValue(SecondsRemainingProperty);
        set => SetValue(SecondsRemainingProperty, value);
    }

    private CancellationTokenSource _pulseCts;

    public GameTimerView()
    {
        InitializeComponent();
        BarTrack.SizeChanged += (s, e) => Update();
    }

    private void Update()
    {
        int sec = SecondsRemaining;
        int total = TotalSeconds > 0 ? TotalSeconds : 1;

        TimerLabel.Text = $"{sec / 60}:{sec % 60:D2}";

        double progress = (double)sec / total;

        Color barColor;
        Color glowColor;
        string hint = "";

        if (progress > 0.5)
        {
            barColor = Color.FromArgb("#22D3EE");
            glowColor = Color.FromArgb("#22D3EE");
            TimerLabel.TextColor = barColor;
            TimerHint.TextColor = barColor;
        }
        else if (progress > 0.25)
        {
            barColor = Color.FromArgb("#F59E0B");
            glowColor = Color.FromArgb("#F59E0B");
            TimerLabel.TextColor = barColor;
            TimerHint.TextColor = barColor;
            hint = "HURRY UP!";
        }
        else
        {
            barColor = Color.FromArgb("#EF4444");
            glowColor = Color.FromArgb("#F87171");
            TimerLabel.TextColor = barColor;
            TimerHint.TextColor = barColor;
            hint = "TIME IS RUNNING OUT!";
            StartPulse(barColor);
        }

        TimerHint.Text = hint;

        BarFill.BackgroundColor = barColor;
        var shadow = BarFill.Shadow;
        if (shadow != null)
            shadow.Brush = new SolidColorBrush(glowColor);

        double trackWidth = BarTrack.Width > 0 ? BarTrack.Width : 300;
        double targetWidth = System.Math.Max(0, progress * trackWidth);

        if (Math.Abs(BarFill.WidthRequest - targetWidth) > 1)
            BarFill.WidthRequest = targetWidth;
    }

    private async void StartPulse(Color color)
    {
        StopPulse();
        _pulseCts = new CancellationTokenSource();
        var ct = _pulseCts.Token;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await BarFill.ScaleTo(1.05, 400, Easing.SinInOut);
                await BarFill.ScaleTo(1.0, 400, Easing.SinInOut);
            }
        }
        catch (TaskCanceledException) { }
    }

    private void StopPulse()
    {
        _pulseCts?.Cancel();
        _pulseCts?.Dispose();
        _pulseCts = null;
        BarFill.ScaleTo(1.0, 100);
    }
}