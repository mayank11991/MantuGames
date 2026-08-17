using System.Text.Json;

namespace MantuGames.Services;

public record UpdateInfo(string VersionName, int VersionCode, int MinVersionCode, string Notes, string Url);

/// <summary>
/// Checks the app store release manifest (latest.json on the website) and
/// reports whether an update is available or required.
/// </summary>
public static class UpdateService
{
    private static HttpClient? _client;

    private static HttpClient Client => _client ??= new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    public static async Task<UpdateInfo?> FetchLatestAsync()
    {
        try
        {
            string json = await Client.GetStringAsync(AppConfig.UpdateCheckUrl);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new UpdateInfo(
                VersionName: root.GetProperty("versionName").GetString() ?? "",
                VersionCode: root.GetProperty("versionCode").GetInt32(),
                MinVersionCode: root.GetProperty("minVersionCode").GetInt32(),
                Notes: root.TryGetProperty("notes", out var n) ? n.GetString() ?? "" : "",
                Url: root.TryGetProperty("url", out var u) ? u.GetString() ?? AppConfig.PlayStoreUrl : AppConfig.PlayStoreUrl);
        }
        catch
        {
            return null;
        }
    }

    public static int CurrentVersionCode
    {
        get
        {
            int.TryParse(AppInfo.BuildString, out int code);
            return code;
        }
    }

    public static bool IsUpdateAvailable(UpdateInfo latest, out bool force)
    {
        force = latest.MinVersionCode > CurrentVersionCode;
        return force || latest.VersionCode > CurrentVersionCode;
    }
}