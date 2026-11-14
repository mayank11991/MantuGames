namespace MantuGames.Views;

public class FakeAdPage : ContentPage
{
    private Label _countdownLabel;
    private Label _statusLabel;
    private int _secondsRemaining;
    private IDispatcherTimer _timer;

    public FakeAdPage(int seconds = 30)
    {
        _secondsRemaining = seconds;
        BackgroundColor = Colors.Black;

        var mainLayout = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 20,
        };

        _statusLabel = new Label
        {
            Text = "Loading reward...",
            FontSize = 18,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
        };

        _countdownLabel = new Label
        {
            Text = _secondsRemaining.ToString(),
            FontSize = 48,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
        };

        var progress = new ProgressBar
        {
            Progress = 0,
            ProgressColor = Color.FromArgb("#6D4AFF"),
            WidthRequest = 250,
            HeightRequest = 8,
            HorizontalOptions = LayoutOptions.Center,
        };

        mainLayout.Children.Add(_statusLabel);
        mainLayout.Children.Add(_countdownLabel);
        mainLayout.Children.Add(progress);

        Content = new Grid { Children = { mainLayout } };

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) =>
        {
            _secondsRemaining--;
            _countdownLabel.Text = _secondsRemaining.ToString();
            progress.Progress = (double)(seconds - _secondsRemaining) / seconds;

            if (_secondsRemaining <= 5)
                _statusLabel.Text = "Reward unlocked!";

            if (_secondsRemaining <= 0)
            {
                _timer.Stop();
                FakeAdClosed?.Invoke(this, EventArgs.Empty);
            }
        };
    }

    public event EventHandler FakeAdClosed;

    public void Start() => _timer.Start();

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
    }
}
