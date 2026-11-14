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
    private string _gameId = null!;
    private string _gameRoute = null!;
    private bool _isNavigating;

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

    private Color _gameAccentColor = Color.FromArgb("#7C5CFF");

    private void BuildMap()
    {
        if (string.IsNullOrEmpty(_gameId)) return;

        var info = DashboardViewModel.Games.FirstOrDefault(g => g.Id == _gameId);
        _gameRoute = info?.Route ?? _gameId;

        if (info?.CardColor != null)
            _gameAccentColor = Color.FromArgb(info.CardColor);

        Backdrop.AccentColor = _gameAccentColor;
        GameTitleLabel.Text = (info?.Title ?? _gameId).ToUpperInvariant();
        HeroThumb.Source = info?.ImageName;
        InstructionsLabel.Text = GameRules.GetRules(_gameId);
        CoinsLabel.Text = CoinService.GetCoins(_gameId).ToString();

        Levels.Clear();
        var levels = ProgressService.Instance.GetLevels(_gameId, 50);
        int done = 0, stars = 0;
        foreach (var lp in levels)
        {
            if (lp.IsCompleted) { done++; stars += lp.Stars; }
            Levels.Add(CreateLevelTileViewModel(lp));
        }
        ProgressLabel.Text = $"{done} / {levels.Count} levels";
        StarsLabel.Text = stars.ToString();
    }

    // Tile states: completed = white + green candy lip + stars; current = orange CTA;
    // locked = translucent white with a lock.
    private LevelTileViewModel CreateLevelTileViewModel(LevelProgress lp)
    {
        bool current = lp.IsUnlocked && !lp.IsCompleted;

        var vm = new LevelTileViewModel
        {
            LevelNumber = lp.LevelNumber,
            IsLocked = lp.IsLocked,
            IsCompleted = lp.IsCompleted,
            IsCurrent = current,
            Stars = lp.IsCompleted ? lp.Stars : 0,
            BgColor = lp.IsLocked ? Color.FromArgb("#80FFFFFF") : current ? Color.FromArgb("#FF8A3D") : Colors.White,
            StrokeColor = lp.IsLocked ? Colors.Transparent : current ? Colors.White : Color.FromArgb("#3DC86A"),
            FaceColor = Colors.Transparent,
            FaceOpacity = lp.IsLocked ? 0.75 : 1.0,
            // Candy-button lip: orange for "play me next", green for cleared levels.
            ShadowBrush = lp.IsLocked ? Brush.Transparent : new SolidColorBrush(Color.FromArgb(current ? "#D4611A" : "#239448")),
            ShowStars = lp.IsCompleted,
            LevelText = lp.LevelNumber.ToString(),
        };

        vm.Star1 = vm.Stars >= 1;
        vm.Star2 = vm.Stars >= 2;
        vm.Star3 = vm.Stars >= 3;
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
            ProgressService.ClearAssisted(_gameId);
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
        // Back from a game without finishing: any Show Solution used there ends with that attempt.
        if (!string.IsNullOrEmpty(_gameId))
            ProgressService.ClearAssisted(_gameId);
        BuildMap();
        AudioService.Instance.StartMusic();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
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
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public int LevelNumber { get; set; }
    public bool IsLocked { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public int Stars { get; set; }
    public Color BgColor { get; set; } = null!;
    public Color StrokeColor { get; set; } = null!;
    public Color FaceColor { get; set; } = null!;
    public double FaceOpacity { get; set; }
    public Brush ShadowBrush { get; set; } = null!;
    public bool ShowNumber { get; set; }
    public bool ShowLock { get; set; }
    public bool ShowStars { get; set; }
    public string LevelText { get; set; } = "";
    public bool Star1 { get; set; }
    public bool Star2 { get; set; }
    public bool Star3 { get; set; }
    public bool Star4 { get; set; }
    public bool Star5 { get; set; }

    public string Star1Icon => Star1 ? "star_kawaii_filled.png" : "star_kawaii_empty.png";
    public string Star2Icon => Star2 ? "star_kawaii_filled.png" : "star_kawaii_empty.png";
    public string Star3Icon => Star3 ? "star_kawaii_filled.png" : "star_kawaii_empty.png";

    public View FaceContent => IsLocked
        ? new Image
        {
            Source = "ic_lock_ink.png",
            HeightRequest = 22,
            WidthRequest = 22,
            Opacity = 0.55,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        }
        : new Label
        {
            Text = LevelText,
            FontFamily = "Display",
            FontSize = 26,
            TextColor = IsCurrent ? Colors.White : Color.FromArgb("#111114"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
}
