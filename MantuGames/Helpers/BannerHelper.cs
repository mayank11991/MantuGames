using MantuGames.Services;

namespace MantuGames.Helpers;

public static class BannerHelper
{
    private static readonly List<Grid> _bannerContainers = new();

    public static void AddBannerAd(this ContentPage page)
    {
        if (IapService.RemoveAdsOwned) return;

        var existingContent = page.Content;
        if (existingContent == null) return;

        page.Content = null;

        var banner = new Plugin.MauiMtAdmob.Controls.MTAdView
        {
            AdsId = AppConfig.BannerAdUnitId,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.End,
        };
        var bannerContainer = new Grid
        {
            HeightRequest = 60,
            VerticalOptions = LayoutOptions.End,
        };
        bannerContainer.Children.Add(banner);

        var wrapper = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            },
            Children = { existingContent },
        };
        wrapper.Children.Add(bannerContainer);
        Grid.SetRow(bannerContainer, 1);

        page.Content = wrapper;

        _bannerContainers.Add(bannerContainer);
    }

    /// <summary>Removes all live banner ad containers (called after Remove Ads purchase).</summary>
    public static void RemoveBanner()
    {
        foreach (var container in _bannerContainers)
        {
            if (container.Parent is Grid wrapper)
                wrapper.Children.Remove(container);
        }
        _bannerContainers.Clear();
    }
}