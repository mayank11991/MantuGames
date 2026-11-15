using MantuGames.Services;

namespace MantuGames.Views.Controls;

/// <summary>
/// The active player's avatar with a thought bubble. <see cref="Text"/> holds the level label
/// (bindable, and settable from code like the old LevelLabel); the small line rotates through
/// playful "thinking" phrases and can be set with <see cref="Think"/> / <see cref="Cheer"/>.
/// </summary>
public partial class ThinkingBuddy : ContentView
{
    private static readonly string[] Thoughts =
    {
        "Hmm… let me think!", "I can do this!", "Think, think, think…", "Ooh, tricky one!",
        "One step at a time", "Let's go!", "Easy peasy?", "Brain power ON",
    };

    private static readonly string[] Cheers = { "Yay! Nailed it!", "Woohoo!", "Super smart!", "Awesome!" };

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ThinkingBuddy), "LEVEL 1",
            propertyChanged: (b, _, n) => ((ThinkingBuddy)b).TextLabel.Text = (n as string ?? "").ToUpperInvariant());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ThinkingBuddy()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            RefreshAvatar();
            Think();
            StartIdleBob();
        };
    }

    public void RefreshAvatar()
    {
        var img = ProfileService.Active?.AvatarImage;
        AvatarImage.Source = string.IsNullOrEmpty(img) ? "a1.png" : img;
    }

    /// <summary>Show a random thinking phrase.</summary>
    public void Think() => ThoughtLabel.Text = Thoughts[Random.Shared.Next(Thoughts.Length)];

    /// <summary>Show a happy phrase with a little pop.</summary>
    public async void Cheer()
    {
        ThoughtLabel.Text = Cheers[Random.Shared.Next(Cheers.Length)];
        await Bubble.ScaleToAsync(1.12, 120, Easing.CubicOut);
        await Bubble.ScaleToAsync(1.0, 160, Easing.SpringOut);
    }

    private bool _bobbing;

    private async void StartIdleBob()
    {
        if (_bobbing) return;
        _bobbing = true;
        try
        {
            while (IsLoaded)
            {
                await Bubble.TranslateToAsync(0, -3, 900, Easing.SinInOut);
                await Bubble.TranslateToAsync(0, 0, 900, Easing.SinInOut);
            }
            Bubble.TranslationY = 0;
        }
        catch { /* view torn down */ }
        finally { _bobbing = false; }
    }
}
