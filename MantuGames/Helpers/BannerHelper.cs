using MantuGames.Services;

namespace MantuGames.Helpers;

public static class BannerHelper
{
    // Weak references so pages that were popped can be garbage-collected.
    private static readonly List<WeakReference<Grid>> _bannerContainers = new();

    public static void AddBannerAd(this ContentPage page)
    {
        if (IapService.RemoveAdsOwned) return;

        var existingContent = page.Content;
        if (existingContent == null) return;

        page.Content = null;

        // Collapsed until an ad is actually attached (no empty strip before consent).
        var bannerContainer = new Grid
        {
            HeightRequest = 0,
            VerticalOptions = LayoutOptions.End,
        };

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

        lock (_bannerContainers)
        {
            _bannerContainers.RemoveAll(w => !w.TryGetTarget(out _));
            _bannerContainers.Add(new WeakReference<Grid>(bannerContainer));
        }

        if (ConsentService.CanRequestAds)
        {
            AttachBanner(bannerContainer);
        }
        else
        {
            // Consent still being gathered: attach once ads are allowed.
            var weak = new WeakReference<Grid>(bannerContainer);
            EventHandler? handler = null;
            handler = (s, e) =>
            {
                ConsentService.AdsAllowed -= handler;
                if (weak.TryGetTarget(out var container))
                    MainThread.BeginInvokeOnMainThread(() => AttachBanner(container));
            };
            ConsentService.AdsAllowed += handler;
        }
    }

    private static void AttachBanner(Grid container)
    {
        try
        {
            if (IapService.RemoveAdsOwned || container.Children.Count > 0) return;

            container.Children.Add(new Plugin.MauiMtAdmob.Controls.MTAdView
            {
                AdsId = AppConfig.BannerAdUnitId,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.End,
            });
            container.HeightRequest = 60;
        }
        catch (Exception ex)
        {
            CrashGuardService.Log(ex);
        }
    }

    /// <summary>Removes all live banner ad containers (called after Remove Ads purchase).</summary>
    public static void RemoveBanner()
    {
        lock (_bannerContainers)
        {
            foreach (var weak in _bannerContainers)
            {
                if (weak.TryGetTarget(out var container) && container.Parent is Grid wrapper)
                    wrapper.Children.Remove(container);
            }
            _bannerContainers.Clear();
        }
    }
}
