namespace MantuGames.Models;

/// <summary>
/// Candy-Crush-style match-3 puzzle: swap adjacent tiles, match 3+ in a
/// row or column, clear them and let the board cascade. Board is generated
/// with no pre-existing matches but at least one valid move.
/// </summary>
public class AnimalCrushPuzzle
{
    public const int Rows = 8;
    public const int Cols = 8;

    /// <summary>Animal type index per cell; -1 = empty (should never persist).</summary>
    public int[,] Board { get; private set; } = new int[Rows, Cols];

    /// <summary>Number of distinct animal types (5 easy, 6 hard).</summary>
    public int AnimalCount { get; private set; } = 5;

    public int TimerSec    { get; private set; } = 80;
    public int TargetScore { get; private set; } = 200;

    public static string[] Animals { get; } =
    {
        "cat.png", "panda.png", "penguin.png", "lion.png", "elephant.png", "owl.png",
    };

    /// <summary>
    /// Creates a board for the given level. <paramref name="timerSec"/> and
    /// <paramref name="targetScore"/> come from the caller (the game page),
    /// so the model stays free of platform services.
    /// </summary>
    public static AnimalCrushPuzzle Generate(int level, int timerSec, int targetScore)
    {
        var puzzle = new AnimalCrushPuzzle
        {
            AnimalCount = level <= 3 ? 5 : 6,
            TimerSec    = timerSec,
            TargetScore = targetScore,
        };

        int attempts = 0;
        do
        {
            puzzle.Fill();
            attempts++;
        }
        while (!puzzle.HasPossibleMove() && attempts < 50);

        return puzzle;
    }

    private void Fill()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                Board[r, c] = PickNoMatch(r, c);
    }

    private int PickNoMatch(int r, int c)
    {
        while (true)
        {
            int t = Random.Shared.Next(AnimalCount);
            // avoid 3 in a row horizontally
            if (c >= 2 && Board[r, c - 1] == t && Board[r, c - 2] == t) continue;
            // avoid 3 in a column vertically
            if (r >= 2 && Board[r - 1, c] == t && Board[r - 2, c] == t) continue;
            // avoid 2x2 square
            if (r >= 1 && c >= 1 && Board[r - 1, c] == t && Board[r, c - 1] == t && Board[r - 1, c - 1] == t) continue;
            if (r >= 1 && c + 1 < Cols && Board[r - 1, c] == t && Board[r, c + 1] == t && Board[r - 1, c + 1] == t) continue;
            if (r + 1 < Rows && c >= 1 && Board[r + 1, c] == t && Board[r, c - 1] == t && Board[r + 1, c - 1] == t) continue;
            if (r + 1 < Rows && c + 1 < Cols && Board[r + 1, c] == t && Board[r, c + 1] == t && Board[r + 1, c + 1] == t) continue;
            return t;
        }
    }

    // ── Match detection ────────────────────────────────────────────────
    public static List<(int r, int c)> FindAllMatches(int[,] board)
    {
        var matched = new HashSet<(int, int)>();

        for (int r = 0; r < Rows; r++)
        {
            int run = 1;
            for (int c = 1; c <= Cols; c++)
            {
                bool same = c < Cols && board[r, c] >= 0 &&
                            board[r, c] == board[r, c - 1];
                if (same) { run++; continue; }

                if (run >= 3)
                    for (int i = c - run; i < c; i++)
                        matched.Add((r, i));
                run = 1;
            }
        }

        for (int c = 0; c < Cols; c++)
        {
            int run = 1;
            for (int r = 1; r <= Rows; r++)
            {
                bool same = r < Rows && board[r, c] >= 0 &&
                            board[r, c] == board[r - 1, c];
                if (same) { run++; continue; }

                if (run >= 3)
                    for (int i = r - run; i < r; i++)
                        matched.Add((i, c));
                run = 1;
            }
        }

        // 2x2 squares of the same animal also count as a match
        for (int r = 0; r < Rows - 1; r++)
            for (int c = 0; c < Cols - 1; c++)
            {
                int t = board[r, c];
                if (t >= 0 && board[r, c + 1] == t &&
                    board[r + 1, c] == t && board[r + 1, c + 1] == t)
                {
                    matched.Add((r, c));
                    matched.Add((r, c + 1));
                    matched.Add((r + 1, c));
                    matched.Add((r + 1, c + 1));
                }
            }

        return matched.ToList();
    }

    private static bool CreatesMatch(int[,] board, int r1, int c1, int r2, int c2)
    {
        (board[r1, c1], board[r2, c2]) = (board[r2, c2], board[r1, c1]);
        var matches = FindAllMatches(board);
        (board[r1, c1], board[r2, c2]) = (board[r2, c2], board[r1, c1]);
        return matches.Count > 0;
    }

    /// <summary>True when at least one adjacent swap would create a match.</summary>
    public bool HasPossibleMove()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
            {
                if (c + 1 < Cols && CreatesMatch(Board, r, c, r, c + 1)) return true;
                if (r + 1 < Rows && CreatesMatch(Board, r, c, r + 1, c)) return true;
            }
        return false;
    }

    /// <summary>First swap that creates a match, or null.</summary>
    public (int r1, int c1, int r2, int c2)? FindHint()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
            {
                if (c + 1 < Cols && CreatesMatch(Board, r, c, r, c + 1))
                    return (r, c, r, c + 1);
                if (r + 1 < Rows && CreatesMatch(Board, r, c, r + 1, c))
                    return (r, c, r + 1, c);
            }
        return null;
    }

    // ── Swap + resolve ─────────────────────────────────────────────────
    /// <summary>
    /// Swaps two adjacent tiles. Returns true (and resolves cascades,
    /// filling <paramref name="scoreOut"/> with the gained points) when the
    /// swap creates a match; otherwise swaps back.
    /// </summary>
    public bool TrySwap(int r1, int c1, int r2, int c2, out int scoreOut)
    {
        scoreOut = 0;
        if (r1 < 0 || c1 < 0 || r2 < 0 || c2 < 0 ||
            r1 >= Rows || c1 >= Cols || r2 >= Rows || c2 >= Cols) return false;
        if (Math.Abs(r1 - r2) + Math.Abs(c1 - c2) != 1) return false;

        (Board[r1, c1], Board[r2, c2]) = (Board[r2, c2], Board[r1, c1]);
        if (FindAllMatches(Board).Count == 0)
        {
            (Board[r1, c1], Board[r2, c2]) = (Board[r2, c2], Board[r1, c1]);
            return false;
        }

        scoreOut = ResolveCascades();
        return true;
    }

    /// <summary>Clears matches, applies gravity, refills and cascades until stable.</summary>
    private int ResolveCascades()
    {
        int total = 0;
        int round = 1;

        while (true)
        {
            var matches = FindAllMatches(Board);
            if (matches.Count == 0) break;

            int tiles = 0;
            foreach (var (r, c) in matches)
            {
                if (Board[r, c] >= 0) { tiles++; Board[r, c] = -1; }
            }

            int roundScore = tiles * 10 * round;
            if (tiles >= 4) roundScore += (tiles - 3) * 20;
            total += roundScore;
            round++;

            ApplyGravity();
        }

        return total;
    }

    private void ApplyGravity()
    {
        for (int c = 0; c < Cols; c++)
        {
            int write = Rows - 1;
            for (int r = Rows - 1; r >= 0; r--)
            {
                if (Board[r, c] >= 0)
                    Board[write--, c] = Board[r, c];
            }
            for (int r = write; r >= 0; r--)
                Board[r, c] = Random.Shared.Next(AnimalCount);
        }
    }

    /// <summary>Re-seeds the board until a move exists (used when stuck).</summary>
    public void Reshuffle()
    {
        int attempts = 0;
        do
        {
            Fill();
            attempts++;
        }
        while (!HasPossibleMove() && attempts < 50);
    }

    // ── Power-ups ─────────────────────────────────────────────────────
    public enum PowerUpKind { Hammer, Rocket, Bomb }

    /// <summary>
    /// Cells affected by a power-up placed at (r, c):
    /// hammer = the tile itself; rocket = whole row + column;
    /// bomb = the tile + up to 3 tiles in each direction (plus shape).
    /// </summary>
    public static List<(int r, int c)> GetPowerUpCells(PowerUpKind kind, int r, int c)
    {
        var cells = new List<(int, int)>();

        switch (kind)
        {
            case PowerUpKind.Hammer:
                cells.Add((r, c));
                break;

            case PowerUpKind.Rocket:
                for (int i = 0; i < Cols; i++) cells.Add((r, i));
                for (int i = 0; i < Rows; i++) cells.Add((i, c));
                break;

            case PowerUpKind.Bomb:
                for (int i = 1; i <= 3; i++)
                {
                    if (r - i >= 0)          cells.Add((r - i, c));
                    if (r + i < Rows)        cells.Add((r + i, c));
                    if (c - i >= 0)          cells.Add((r, c - i));
                    if (c + i < Cols)        cells.Add((r, c + i));
                }
                cells.Add((r, c));
                break;
        }

        return cells;
    }

    /// <summary>
    /// Applies a power-up at (r, c): clears its cells, lets gravity refill
    /// and resolves any cascades. Returns the score gained.
    /// </summary>
    public int UsePowerUp(PowerUpKind kind, int r, int c)
    {
        if (r < 0 || c < 0 || r >= Rows || c >= Cols) return 0;

        int cleared = 0;
        foreach (var (cr, cc) in GetPowerUpCells(kind, r, c))
            if (Board[cr, cc] >= 0) { Board[cr, cc] = -1; cleared++; }

        if (cleared == 0) return 0;

        ApplyGravity();
        return cleared * 10 + ResolveCascades();
    }
}