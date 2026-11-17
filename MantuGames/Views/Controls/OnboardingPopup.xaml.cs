using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MantuGames.Views.Controls;

/// <summary>
/// First-launch setup, shown once: 1) pick a skin, 2) type a name, 3) year of birth.
/// Paid skins are shown but locked here (they can be bought later from SKINS), so a
/// new player is never asked to pay before they have even played. The age question
/// stays neutral: every year is listed and nothing is preselected.
/// </summary>
public partial class OnboardingPopup : ContentView
{
    private TaskCompletionSource? _tcs;
    private int _step = 1;
    private Skin _skin = SkinCatalog.Default;

    public OnboardingPopup()
    {
        InitializeComponent();
        ThemeStyler.RegisterDialog(Card);
        int thisYear = DateTime.Now.Year;
        YearPicker.ItemsSource = Enumerable.Range(0, 101).Select(i => (thisYear - i).ToString()).ToList();
    }

    /// <summary>Shows the setup and completes once the player, skin and age are saved.</summary>
    public Task ShowAsync()
    {
        _tcs = new TaskCompletionSource();
        _skin = SkinService.Current;
        NameEntry.Text = ProfileService.Active?.Name ?? "";
        YearPicker.SelectedIndex = -1;
        BuildSkinGrid();
        GoTo(1);
        IsVisible = true;
        Opacity = 0;
        _ = this.FadeToAsync(1, 220, Easing.CubicOut);
        return _tcs.Task;
    }

    // ── Steps ──────────────────────────────────────────────────────────────
    private void GoTo(int step)
    {
        _step = step;
        SkinStep.IsVisible = step == 1;
        NameStep.IsVisible = step == 2;
        AgeStep.IsVisible = step == 3;
        BackLabel.IsVisible = step > 1;
        HintLabel.Text = "";

        foreach (var (dot, i) in new[] { (Dot1, 1), (Dot2, 2), (Dot3, 3) })
        {
            dot.WidthRequest = i == step ? 26 : 8;
            dot.Color = Color.FromArgb(i <= step ? "#111114" : "#D5D7E0");
        }

        switch (step)
        {
            case 1:
                TitleLabel.Text = "PICK YOUR BUDDY";
                SubLabel.Text = "Your buddy sets the colours of your whole app.";
                NextLabel.Text = "NEXT";
                break;
            case 2:
                TitleLabel.Text = "WHAT'S YOUR NAME?";
                SubLabel.Text = "Just a first name or a nickname.";
                NextLabel.Text = "NEXT";
                break;
            default:
                TitleLabel.Text = "ONE LAST THING";
                SubLabel.Text = "What year were you born?";
                NextLabel.Text = "LET'S PLAY!";
                break;
        }
        ApplyPreview();
        UpdateNextState();
    }

    private void ApplyPreview()
    {
        Root.BackgroundColor = Color.FromArgb(_skin.ShellTop);
        foreach (var b in new[] { NamePreview, AgePreview })
            b.BackgroundColor = Color.FromArgb(_skin.ShellMid);
        NameMascot.Source = _skin.Mascot;
        AgeMascot.Source = _skin.Mascot;
    }

    private void UpdateNextState()
    {
        bool ok = _step switch
        {
            2 => !string.IsNullOrWhiteSpace(NameEntry.Text),
            3 => YearPicker.SelectedIndex >= 0,
            _ => true,
        };
        NextBorder.Opacity = ok ? 1 : 0.45;
    }

    private void OnNameChanged(object? sender, TextChangedEventArgs e) => UpdateNextState();
    private void OnYearChanged(object? sender, EventArgs e) => UpdateNextState();

    private void OnBack(object? sender, TappedEventArgs e)
    {
        AudioService.Instance.Play("tap");
        if (_step > 1) GoTo(_step - 1);
    }

    private async void OnNext(object? sender, TappedEventArgs e)
    {
        try
        {
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            switch (_step)
            {
                case 1:
                    GoTo(2);
                    break;
                case 2:
                    if (string.IsNullOrWhiteSpace(NameEntry.Text)) { HintLabel.Text = "Please type a name."; return; }
                    NameEntry.Unfocus();
                    GoTo(3);
                    break;
                default:
                    if (YearPicker.SelectedItem is not string text || !int.TryParse(text, out int year))
                    {
                        HintLabel.Text = "Please choose a year.";
                        return;
                    }
                    await FinishAsync(year);
                    break;
            }
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
        }
    }

    private async Task FinishAsync(int year)
    {
        // Profile first: the chosen skin is stored per player.
        ProfileService.SetUpPlayer(NameEntry.Text ?? "Player");
        SkinService.Use(_skin);
        AgeService.SetBirthYear(year); // lets ads start, with the right age settings

        AudioService.Instance.Play("win");
        await this.FadeToAsync(0, 180, Easing.CubicIn);
        IsVisible = false;
        _tcs?.TrySetResult();
    }

    // ── Skin grid ──────────────────────────────────────────────────────────
    private void BuildSkinGrid()
    {
        SkinGrid.Children.Clear();
        var skins = SkinCatalog.All;
        const int cols = 3;
        for (int i = 0; i < skins.Count; i += cols)
        {
            var row = new Grid { ColumnSpacing = 10 };
            for (int c = 0; c < cols; c++) row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (int c = 0; c < cols && i + c < skins.Count; c++)
            {
                var tile = BuildTile(skins[i + c]);
                Grid.SetColumn(tile, c);
                row.Children.Add(tile);
            }
            SkinGrid.Children.Add(row);
        }
    }

    private View BuildTile(Skin skin)
    {
        bool owned = SkinService.IsOwned(skin);
        bool selected = skin.Id == _skin.Id;

        var art = new Grid
        {
            HeightRequest = 92,
            Children =
            {
                new Image { Source = skin.Mascot, Aspect = Aspect.AspectFit, Margin = new Thickness(4), Opacity = owned ? 1 : 0.55 },
                new Image { Source = "ic_lock_ink.png", WidthRequest = 16, HeightRequest = 16, IsVisible = !owned,
                            HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(6) },
            },
        };

        var tile = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            StrokeThickness = selected ? 3 : 0,
            Stroke = Color.FromArgb("#111114"),
            Padding = new Thickness(4, 4, 4, 6),
            Background = new LinearGradientBrush(new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(skin.ShellTop), 0f),
                new GradientStop(Color.FromArgb(skin.ShellMid), 1f),
            }, new Point(0, 0), new Point(0, 1)),
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    art,
                    new FitLabel
                    {
                        Text = skin.Name, MinFontSize = 9, MaxLines = 1,
                        FontFamily = "Baloo2ExtraBold", FontSize = 12,
                        TextColor = Color.FromArgb(owned ? "#111114" : "#6E6E7C"),
                        HorizontalOptions = LayoutOptions.Center,
                    },
                },
            },
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) =>
        {
            AudioService.Instance.Play("tap");
            if (!SkinService.IsOwned(skin))
            {
                HintLabel.Text = "Unlock more buddies any time from SKINS.";
                return;
            }
            _skin = skin;
            HintLabel.Text = "";
            BuildSkinGrid();
            ApplyPreview();
        };
        tile.GestureRecognizers.Add(tap);
        return tile;
    }
}
