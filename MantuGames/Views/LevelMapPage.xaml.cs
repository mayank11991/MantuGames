using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using MantuGames.ViewModels;

namespace MantuGames.Views;

[QueryProperty(nameof(GameId), "gameId")]
public partial class LevelMapPage : ContentPage
{
    private string _gameId;
    private string _gameRoute;
    private bool _isNavigating;
    private bool _isNebulaAnimating = false;
    private CancellationTokenSource _nebulaCts;

    public string GameId
    {
        get => _gameId;
        set
        {
            _gameId = Uri.UnescapeDataString(value ?? "");
            BuildMap();
        }
    }

    public ObservableCollection<LevelTileViewModel> Levels { get; } = new();
    public ICommand LevelTappedCommand { get; }

    public LevelMapPage()
    {
        InitializeComponent();
        BindingContext = this;
        LevelTappedCommand = new Command<LevelTileViewModel>(async vm => await OnLevelTapped(vm));
        this.AddBannerAd();
    }

    private Color _gameAccentColor = Color.FromArgb("#8ECCCC");

    private void BuildMap()
    {
        if (string.IsNullOrEmpty(_gameId)) return;

        var info = DashboardViewModel.Games.FirstOrDefault(g => g.Id == _gameId);
        _gameRoute = info?.Route ?? _gameId;

        if (info?.CardColor != null)
        {
            _gameAccentColor = Color.FromArgb(info.CardColor);
        }

        // Set game title
        GameTitleLabel.Text = info?.Title ?? _gameId;

        // Set instructions
        InstructionsLabel.Text = GameRules.GetRules(_gameId);

        // Set coins for this game
        int coins = CoinService.GetCoins(_gameId);
        CoinsLabel.Text = $"{coins}";

        Levels.Clear();

        int highest = ProgressService.Instance.GetHighestUnlocked(_gameId);
        int show = highest + 5;

        var levels = ProgressService.Instance.GetLevels(_gameId, show);
        foreach (var lp in levels)
        {
            var vm = CreateLevelTileViewModel(lp);
            Levels.Add(vm);
        }
    }

    private LevelTileViewModel CreateLevelTileViewModel(LevelProgress lp)
    {
        Color accent = lp.IsLocked ? Color.FromArgb("#99888888") : _gameAccentColor;

        var vm = new LevelTileViewModel
        {
            LevelNumber = lp.LevelNumber,
            IsLocked = lp.IsLocked,
            IsCompleted = lp.IsCompleted,
            Stars = lp.IsCompleted ? lp.Stars : 0,
            BgColor = Color.FromArgb("#182136"),
            StrokeColor = accent,
            FaceColor = accent,
            FaceOpacity = lp.IsLocked ? 0.6 : 1.0,
            ShadowBrush = lp.IsLocked ? Brush.Transparent : new SolidColorBrush(Color.FromArgb("#882D2A3D")),
            ShowStars = lp.IsCompleted,
            LevelText = lp.LevelNumber.ToString(),
        };

        // Set star visibility
        vm.Star1 = vm.Stars >= 1;
        vm.Star2 = vm.Stars >= 2;
        vm.Star3 = vm.Stars >= 3;
        vm.Star4 = false;
        vm.Star5 = false;

        return vm;
    }

    private async Task OnLevelTapped(LevelTileViewModel vm)
    {
        if (_isNavigating || vm == null || vm.IsLocked) return;
        _isNavigating = true;
        try
        {
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            Console.WriteLine($"Navigating to {_gameRoute}?level={vm.LevelNumber}");
            await Shell.Current.GoToAsync($"{_gameRoute}?level={vm.LevelNumber}");
            Console.WriteLine($"Navigation completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Navigation error: {ex.Message}");
        }
        finally
        {
            _isNavigating = false;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BuildMap();
        AudioService.Instance.StartMusic();
        StartNebulaAnimation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopNebulaAnimation();
    }

    private void StartNebulaAnimation()
    {
        if (_isNebulaAnimating) return;
        _isNebulaAnimating = true;
        _nebulaCts = new CancellationTokenSource();
        var token = _nebulaCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                double phase1 = 0, phase2 = 0, phase3 = 0;
                while (!token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        if (Nebula1 != null && Nebula2 != null && Nebula3 != null)
                        {
                            Nebula1.TranslationX = 30 * Math.Sin(phase1);
                            Nebula1.TranslationY = -20 * Math.Cos(phase1);
                            Nebula1.Scale = 1 + 0.05 * Math.Sin(phase1);

                            Nebula2.TranslationX = -20 * Math.Sin(phase2);
                            Nebula2.TranslationY = 25 * Math.Cos(phase2);
                            Nebula2.Scale = 1 + 0.05 * Math.Cos(phase2);

                            Nebula3.TranslationX = -15 * Math.Sin(phase3);
                            Nebula3.TranslationY = -30 * Math.Cos(phase3);
                            Nebula3.Scale = 1 + 0.03 * Math.Sin(phase3);
                        }
                    });

                    phase1 += 0.02;
                    phase2 += 0.018;
                    phase3 += 0.022;
                    await Task.Delay(16, token);
                }
            }
            catch (OperationCanceledException) { }
            catch { }
        }, token);
    }

    private void StopNebulaAnimation()
    {
        if (!_isNebulaAnimating) return;
        _isNebulaAnimating = false;
        _nebulaCts?.Cancel();
        _nebulaCts?.Dispose();
        _nebulaCts = null;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnBackClicked: {ex.Message}");
        }
    }
}

public class LevelTileViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public int LevelNumber { get; set; }
    public bool IsLocked { get; set; }
    public bool IsCompleted { get; set; }
    public int Stars { get; set; }
    public Color BgColor { get; set; }
    public Color StrokeColor { get; set; }
    public Color FaceColor { get; set; }
    public double FaceOpacity { get; set; }
    public Brush ShadowBrush { get; set; }
    public bool ShowNumber { get; set; }
    public bool ShowLock { get; set; }
    public bool ShowStars { get; set; }
    public string LevelText { get; set; }
    public bool Star1 { get; set; }
    public bool Star2 { get; set; }
    public bool Star3 { get; set; }
    public bool Star4 { get; set; }
    public bool Star5 { get; set; }

    public View FaceContent => IsLocked
        ? new Image
        {
            Source = "lock",
            HeightRequest = 30,
            WidthRequest = 30,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        }
        : new Label
        {
            Text = LevelText,
            FontFamily = "MomoTrustDisplay",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#160D02"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
}
