namespace MantuGames.Models;

/// <summary>What the player has done so far, used to decide which badges are earned.</summary>
public sealed record PlayerSnapshot(
    int TotalWins,
    int LevelsDone,
    int BestStreak,
    IReadOnlyDictionary<string, int> WinsByGame)
{
    public int GamesWithAWin => WinsByGame.Count(kv => kv.Value > 0);
    public int Wins(string gameId) => WinsByGame.TryGetValue(gameId, out var w) ? w : 0;
}

/// <summary>A badge: earned once <see cref="Progress"/> reaches <see cref="Target"/>.</summary>
public sealed record Badge(string Id, string Title, string Description, string Icon, int Target, Func<PlayerSnapshot, int> Progress)
{
    public bool IsEarned(PlayerSnapshot s) => Progress(s) >= Target;
}

public static class BadgeCatalog
{
    /// <summary>Coins paid when a badge is earned.</summary>
    public const int Reward = 10;

    public const int AllGamesCount = 11;

    public static readonly IReadOnlyList<Badge> All = new[]
    {
        new Badge("first_win",  "First Win!",     "Win your first level",          "star_kawaii_filled.png", 1,   s => s.TotalWins),
        new Badge("wins_10",    "Winner",         "Win 10 levels",                 "icon_trophy.png",        10,  s => s.TotalWins),
        new Badge("wins_50",    "Champion",       "Win 50 levels",                 "icon_trophy.png",        50,  s => s.TotalWins),
        new Badge("wins_100",   "Super Star",     "Win 100 levels",                "icon_trophy.png",        100, s => s.TotalWins),
        new Badge("levels_25",  "Level Climber",  "Finish 25 different levels",    "star_kawaii_filled.png", 25,  s => s.LevelsDone),
        new Badge("levels_100", "Mountain Top",   "Finish 100 different levels",   "star_kawaii_filled.png", 100, s => s.LevelsDone),
        new Badge("explorer",   "Explorer",       "Win in 5 different games",      "gi_mazerunner.png",      5,   s => s.GamesWithAWin),
        new Badge("all_games",  "Game Master",    "Win in all 11 games",           "gi_arrowlines.png",      AllGamesCount, s => s.GamesWithAWin),
        new Badge("streak_3",   "On Fire",        "Daily challenge 3 days in a row",  "coin.png",           3,   s => s.BestStreak),
        new Badge("streak_7",   "Week Hero",      "Daily challenge 7 days in a row",  "coin.png",           7,   s => s.BestStreak),
        new Badge("streak_30",  "Unstoppable",    "Daily challenge 30 days in a row", "coin.png",           30,  s => s.BestStreak),
        new Badge("sudoku_10",  "Sudoku Star",    "Win 10 Sudoku levels",          "gi_sudoku.png",          10,  s => s.Wins("sudoku")),
        new Badge("maze_10",    "Maze Runner",    "Win 10 Maze levels",            "gi_mazerunner.png",      10,  s => s.Wins("mazerunner")),
        new Badge("math_10",    "Math Whiz",      "Win 10 Math levels",            "gi_mathchallenge.png",   10,  s => s.Wins("mathchallenge")),
        new Badge("memory_10",  "Memory Master",  "Win 10 Memory levels",          "gi_cardmemory.png",      10,  s => s.Wins("cardmemory")),
        new Badge("word_10",    "Word Wizard",    "Win 10 Word Search levels",     "gi_wordfinder.png",      10,  s => s.Wins("wordfinder")),
    };

    /// <summary>Badges earned in <paramref name="s"/> that are not in <paramref name="alreadyEarned"/>.</summary>
    public static IReadOnlyList<Badge> NewlyEarned(PlayerSnapshot s, ISet<string> alreadyEarned) =>
        All.Where(b => !alreadyEarned.Contains(b.Id) && b.IsEarned(s)).ToList();
}
