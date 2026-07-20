namespace MantuGames.Helpers;

public static class BannerHelper
{
    public static void AddBannerAd(this ContentPage page)
    {
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
    }
}
