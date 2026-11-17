using MantuGames.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MantuGames.Views.Controls;

/// <summary>
/// Gives dialog cards the current skin's dialog style (background, border, corner
/// radius, hard shadow). Free skins get each dialog's original XAML look back.
/// Dialogs register their card once; every registered card repaints on a skin change.
/// </summary>
public static class ThemeStyler
{
    private sealed record Original(Brush? Fill, Color? Background, Brush? Stroke, double Thickness, IShape? Shape, Shadow? Shadow);

    private static readonly List<WeakReference<Border>> _cards = new();
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Border, Original> _originals = new();

    public static void RegisterDialog(Border card)
    {
        _originals.AddOrUpdate(card, new Original(card.Background, card.BackgroundColor, card.Stroke, card.StrokeThickness, card.StrokeShape, card.Shadow));
        lock (_cards) _cards.Add(new WeakReference<Border>(card));
        Apply(card);
    }

    public static void RefreshAll()
    {
        List<Border> live = new();
        lock (_cards)
        {
            _cards.RemoveAll(w => !w.TryGetTarget(out _));
            foreach (var w in _cards)
                if (w.TryGetTarget(out var b)) live.Add(b);
        }
        foreach (var b in live) Apply(b);
    }

    private static void Apply(Border card)
    {
        var theme = SkinService.Current.Theme;
        if (theme == null)
        {
            if (_originals.TryGetValue(card, out var o))
            {
                card.Background = o.Fill;
                card.BackgroundColor = o.Background;
                card.Stroke = o.Stroke;
                card.StrokeThickness = o.Thickness;
                card.StrokeShape = o.Shape;
                card.Shadow = o.Shadow;
            }
            return;
        }

        card.Background = new SolidColorBrush(Color.FromArgb(theme.DialogBackground));
        card.BackgroundColor = Color.FromArgb(theme.DialogBackground);
        card.Stroke = new SolidColorBrush(Color.FromArgb(theme.DialogBorder));
        card.StrokeThickness = theme.DialogBorderWidth;
        card.StrokeShape = new RoundRectangle { CornerRadius = theme.DialogCornerRadius };
        card.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb(theme.DialogShadow)),
            Offset = new Point(6, 8),
            Radius = 0,
            Opacity = 1,
        };
    }
}
