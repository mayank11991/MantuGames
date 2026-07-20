namespace MantuGames.Models;

public enum Direction
{
    Right,
    Down,
    DiagonalDown
}

public class PlacedWord
{
    public string Word { get; set; }
    public int StartRow { get; set; }
    public int StartCol { get; set; }
    public Direction Dir { get; set; }
    public bool Found { get; set; }
}

public class WordPuzzle
{
    public int GridSize { get; private set; }
    public char[,] Grid { get; private set; }
    public List<PlacedWord> Words { get; private set; } = new();

    private static readonly Random _rng = new();

    // Word banks by difficulty tier
    private static readonly string[] Tier1 = // 3-letter, levels 1-5 (original 20 + 50 new = 70 total)
{
    "CAT", "DOG", "SUN", "RUN", "FUN", "BIG", "RED", "HAT", "BAT", "MAP",
    "CUP", "BUS", "ANT", "BEE", "EGG", "FLY", "HEN", "JAM", "PIG", "RAT",

    // 50 new 3-letter words
    "CAR", "JAR", "BOX", "HEN", "PEN", "BUG", "OWL", "LOG", "FOX", "TOP",
    "RUG", "BAG", "WEB", "COW", "BED", "BID", "DIP", "ZIP", "MOM", "NUN",
    "GAP", "HOP", "JOT", "KIT", "LAP", "MAN", "NAP", "OIL", "PAD", "RAN",
    "SAP", "TAP", "VAN", "WAX", "YAK", "ZOO", "LED", "POT", "RIM", "SIT",
    "TUG", "VET", "WED", "YEN", "ZAP", "DAB", "FAN", "HUB", "JIG", "KIN"
};

private static readonly string[] Tier2 = // 4-letter, levels 6-12 (original 20 + 50 new = 70 total)
{
    "CAKE", "DUCK", "FISH", "FROG", "GIFT", "HAND", "JUMP", "KING", "LAMP", "MILK",
    "NEST", "OPEN", "PLAY", "QUIZ", "RAIN", "SHIP", "TREE", "UGLY", "VERY", "WOLF",

    // 50 new 4-letter words
    "BAKE", "BAND", "BARK", "BIRD", "BOMB", "BOWL", "CLIP", "COIN", "DART", "DUST",
    "EAST", "FALL", "GATE", "GIRL", "GLOW", "GOAL", "HILL", "HURT", "IRON", "JAIL",
    "KIND", "KING", "KISS", "LAND", "LION", "LONG", "LOVE", "MAIL", "MOON", "MULE",
    "NEAR", "NOSE", "NOTE", "OATH", "PACT", "PEAK", "PINE", "POND", "RACE", "RAIN",
    "ROAD", "ROCK", "ROOM", "RUSH", "SAIL", "SAND", "SEED", "SHOP", "SNOW", "SOUL"
};

private static readonly string[] Tier3 = // 5-letter, levels 13+ (original 20 + 50 new = 70 total)
{
    "APPLE", "BEACH", "CANDY", "DANCE", "EAGLE", "FLAME", "GRASS", "HAPPY",
    "ISSUE", "JELLY", "KITE", "LEMON", "MAGIC", "NIGHT", "OCEAN", "PIZZA",
    "QUEEN", "RIVER", "SLEEP", "TIGER",

    // 50 new 5-letter words
    "BRAVE", "CLOUD", "DRINK", "EARTH", "FAITH", "GIANT", "HEART", "INNER", "JUDGE", "KNIFE",
    "LIGHT", "MUSIC", "NURSE", "OFFER", "PLANE", "QUEUE", "RANCH", "SCALE", "SHARE", "TABLE",
    "UNITE", "VIRUS", "WATER", "YOUTH", "ZEBRA", "ABOUT", "BREAD", "CHAIR", "DIGIT", "ELITE",
    "FRUIT", "GLASS", "HOUSE", "INDEX", "JELLY", "KNACK", "LEARN", "MODEL", "NOISE", "OPERA",
    "PRIDE", "QUICK", "ROAST", "SOUND", "THINK", "UNDER", "VALUE", "WORLD", "YEARN", "ZESTY"
};
    public static WordPuzzle Generate(int level)
    {
        var puzzle = new WordPuzzle();
        puzzle.Build(level);
        return puzzle;
    }

    private void Build(int level)
    {
        // Pick word bank and count based on level
        string[] bank;
        int wordCount;
        if (level <= 5)
        {
            bank = Tier1;
            GridSize = 6;
            wordCount = 3 + (level - 1) % 2;
        }
        else if (level <= 12)
        {
            bank = Tier2;
            GridSize = 7;
            wordCount = 3 + (level - 6) % 3;
        }
        else
        {
            bank = Tier3;
            GridSize = 8;
            wordCount = 3 + (level - 13) % 3;
        }

        wordCount = Math.Min(wordCount, 5);

        Grid = new char[GridSize, GridSize];
        Words.Clear();

        // Pick random words
        var picked = bank.OrderBy(_ => _rng.Next()).Take(wordCount * 3).ToList();

        int placed = 0;
        foreach (var word in picked)
        {
            if (placed >= wordCount) break;
            if (TryPlace(word)) placed++;
        }

        // Fill remaining cells with random letters
        for (int r = 0; r < GridSize; r++)
        for (int c = 0; c < GridSize; c++)
            if (Grid[r, c] == '\0')
                Grid[r, c] = (char)('A' + _rng.Next(26));
    }

    private bool TryPlace(string word)
    {
        var directions = Enum.GetValues<Direction>().OrderBy(_ => _rng.Next()).ToArray();
        for (int attempt = 0; attempt < 50; attempt++)
        {
            var dir = directions[attempt % directions.Length];
            int row = _rng.Next(GridSize);
            int col = _rng.Next(GridSize);

            if (CanPlace(word, row, col, dir))
            {
                Place(word, row, col, dir);
                Words.Add(new PlacedWord { Word = word, StartRow = row, StartCol = col, Dir = dir });
                return true;
            }
        }

        return false;
    }

    private bool CanPlace(string word, int row, int col, Direction dir)
    {
        int dr = dir == Direction.Down || dir == Direction.DiagonalDown ? 1 : 0;
        int dc = dir == Direction.Right || dir == Direction.DiagonalDown ? 1 : 0;

        for (int i = 0; i < word.Length; i++)
        {
            int r = row + dr * i;
            int c = col + dc * i;
            if (r < 0 || r >= GridSize || c < 0 || c >= GridSize) return false;
            if (Grid[r, c] != '\0' && Grid[r, c] != word[i]) return false;
        }

        return true;
    }

    private void Place(string word, int row, int col, Direction dir)
    {
        int dr = dir == Direction.Down || dir == Direction.DiagonalDown ? 1 : 0;
        int dc = dir == Direction.Right || dir == Direction.DiagonalDown ? 1 : 0;
        for (int i = 0; i < word.Length; i++)
            Grid[row + dr * i, col + dc * i] = word[i];
    }

    public bool AllFound() => Words.All(w => w.Found);

    public PlacedWord CheckSelection(List<(int Row, int Col)> selected)
    {
        if (selected.Count < 2) return null;

        // Build string from selection
        string attempt = new(selected.Select(s => Grid[s.Row, s.Col]).ToArray());
        string reversed = new(attempt.Reverse().ToArray());

        foreach (var pw in Words.Where(w => !w.Found))
        {
            if (pw.Word == attempt || pw.Word == reversed)
            {
                // Verify cells actually match the word placement
                var wordCells = GetWordCells(pw);
                var selSet = selected.ToHashSet();
                if (wordCells.All(c => selSet.Contains(c)) && selSet.Count == wordCells.Count)
                {
                    pw.Found = true;
                    return pw;
                }
            }
        }

        return null;
    }

    public List<(int Row, int Col)> GetWordCells(PlacedWord pw)
    {
        int dr = pw.Dir == Direction.Down || pw.Dir == Direction.DiagonalDown ? 1 : 0;
        int dc = pw.Dir == Direction.Right || pw.Dir == Direction.DiagonalDown ? 1 : 0;
        var cells = new List<(int, int)>();
        for (int i = 0; i < pw.Word.Length; i++)
            cells.Add((pw.StartRow + dr * i, pw.StartCol + dc * i));
        return cells;
    }
}