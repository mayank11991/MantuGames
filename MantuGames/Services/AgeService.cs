namespace MantuGames.Services;

/// <summary>
/// Remembers the player's birth year from the neutral age question asked once
/// on this device. Ads use it to decide how to treat the player: children (and
/// anyone whose age is unknown) get child-directed ads that never send the
/// advertising ID; older players get standard ads.
/// </summary>
public static class AgeService
{
    private const string BirthYearKey = "birth_year";

    /// <summary>Raised once when the player answers the age question.</summary>
    public static event EventHandler? Answered;

    public static int? BirthYear
    {
        get
        {
            int year = Preferences.Get(BirthYearKey, 0);
            return year > 0 ? year : null;
        }
    }

    public static bool IsKnown => BirthYear.HasValue;

    // The year alone can't tell if the birthday has passed, so assume it hasn't:
    // someone born 13 years ago may still be 12.
    private static int? LowestPossibleAge => BirthYear is int y ? DateTime.Now.Year - y - 1 : null;

    /// <summary>Under 13 (COPPA). Unknown age counts as a child.</summary>
    public static bool IsChild => LowestPossibleAge is not int age || age < 13;

    /// <summary>Under 16, the highest digital age of consent in the EU (GDPR). Unknown counts as under.</summary>
    public static bool IsUnderAgeOfConsent => LowestPossibleAge is not int age || age < 16;

    public static void SetBirthYear(int year)
    {
        Preferences.Set(BirthYearKey, year);
        Answered?.Invoke(null, EventArgs.Empty);
    }
}
