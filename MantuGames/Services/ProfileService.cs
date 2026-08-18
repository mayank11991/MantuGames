using System.Text.Json;
using MantuGames.Models;

namespace MantuGames.Services;

/// <summary>
/// Netflix-style profiles: each profile keeps its own game progress,
/// points and coins. The active profile prefixes every progress key so
/// profiles never share data.
/// </summary>
public static class ProfileService
{
    private const string ProfilesKey = "profiles_json";
    private const string ActiveKey   = "active_profile_id";
    private const string MigrationKey = "profiles_migrated_v1";

    private static List<PlayerProfile> _profiles = new();
    private static PlayerProfile? _active;
    private static bool _loaded;

    public static event EventHandler? ActiveChanged;
    public static event EventHandler? ProfilesChanged;

    public static IReadOnlyList<PlayerProfile> Profiles => _profiles;
    public static PlayerProfile? Active => _active;

    private static readonly string[] AvatarPalette =
    {
        "#22D3EE", "#A855F7", "#FF9F1C", "#34D399",
        "#F43F5E", "#3B82F6", "#FACC15", "#EC4899",
    };

    /// <summary>Namespace for a given storage key — keeps progress separate per profile.</summary>
    public static string Key(string name) =>
        _active != null ? $"p{_active.Id}_{name}" : name;

    public static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            var json = Preferences.Default.Get(ProfilesKey, "");
            if (!string.IsNullOrEmpty(json))
                _profiles = JsonSerializer.Deserialize<List<PlayerProfile>>(json) ?? new();
            else
                MigrateLegacy();
        }
        catch
        {
            _profiles = new List<PlayerProfile>();
        }

        var activeId = Preferences.Default.Get(ActiveKey, "");
        _active = _profiles.FirstOrDefault(p => p.Id == activeId);
    }

    private static void MigrateLegacy()
    {
        // Seed one profile that inherits any existing single-player progress.
        var profile = new PlayerProfile
        {
            Name = Preferences.Default.Get("player_name", "Player 1"),
            Color = AvatarPalette[0],
        };
        if (string.IsNullOrWhiteSpace(profile.Name)) profile.Name = "Player 1";

        _profiles.Add(profile);
        Save();

        if (Preferences.Default.Get(MigrationKey, false)) return;

        // Copy legacy single-player progress into the new profile's namespace.
        string[] games =
        {
            "sudoku", "wordfinder", "mathchallenge", "towerofhanoi",
            "cardmemory", "puzzlepets", "blockpuzzle", "mazerunner",
            // "animalcrush", // Paused: not in 8-game release
        };
        bool hadProgress = false;
        foreach (var g in games)
        {
            for (int i = 1; i <= 200; i++)
            {
                hadProgress |= CopyLegacy($"{g}_level_{i}_state");
                hadProgress |= CopyLegacy($"{g}_level_{i}_stars");
            }
            hadProgress |= CopyLegacy($"{g}_total_points");
            hadProgress |= CopyLegacy($"{g}_coins");
            hadProgress |= CopyLegacy($"{g}_stats_played");
            hadProgress |= CopyLegacy($"{g}_stats_won");
            hadProgress |= CopyLegacy($"{g}_stats_best");
        }
        Preferences.Default.Set(MigrationKey, true);

        // Only auto-select the seeded profile when it actually inherited
        // progress. On a fresh install the user must pick one (or add a new
        // one) — the picker cannot be skipped.
        if (hadProgress)
            Activate(profile);
    }

    private static bool CopyLegacy(string key)
    {
        try
        {
            if (Preferences.Default.ContainsKey(key))
            {
                Preferences.Default.Set(ProfileService.Key(key), Preferences.Default.Get<int>(key, 0));
                return true;
            }
        }
        catch { }
        return false;
    }

    public static PlayerProfile AddProfile(string name)
    {
        var profile = new PlayerProfile
        {
            Name = name.Trim(),
            Color = AvatarPalette[_profiles.Count % AvatarPalette.Length],
        };
        _profiles.Add(profile);
        Save();
        ProfilesChanged?.Invoke(null, EventArgs.Empty);
        return profile;
    }

    public static void Activate(PlayerProfile profile)
    {
        _active = profile;
        Preferences.Default.Set(ActiveKey, profile.Id);
        ActiveChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void RenameProfile(PlayerProfile profile, string newName)
    {
        profile.Name = newName.Trim();
        Save();
        ProfilesChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void Save()
    {
        Preferences.Default.Set(ProfilesKey, JsonSerializer.Serialize(_profiles));
    }
}
