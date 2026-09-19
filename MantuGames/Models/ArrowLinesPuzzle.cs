namespace MantuGames.Models;

public enum ArrowDirection { Up, Down, Left, Right }

public class ArrowCell
{
    public int Row { get; set; }
    public int Col { get; set; }
    public ArrowDirection Direction { get; set; }
    public bool IsCleared { get; set; }
    public int Index => Row * 100 + Col;

    public char Symbol => Direction switch
    {
        ArrowDirection.Up => '\u2191',
        ArrowDirection.Down => '\u2193',
        ArrowDirection.Left => '\u2190',
        ArrowDirection.Right => '\u2192',
        _ => '?'
    };
}

public class ArrowLinesPuzzle
{
    public int Rows { get; private set; }
    public int Cols { get; private set; }
    public List<ArrowCell> Arrows { get; private set; } = new();
    public int TargetArrows { get; private set; }

    private static readonly Random Rng = new();

    public static ArrowLinesPuzzle Generate(int level)
    {
        var puzzle = new ArrowLinesPuzzle();
        puzzle.Rows = 9;
        puzzle.Cols = 9;

        // More aggressive scaling
        if (level <= 3)       puzzle.TargetArrows = 8 + level * 2;
        else if (level <= 8)  puzzle.TargetArrows = 14 + (level - 3) * 3;
        else if (level <= 15) puzzle.TargetArrows = 29 + (level - 8) * 2;
        else if (level <= 25) puzzle.TargetArrows = 43 + (level - 15);
        else if (level <= 40) puzzle.TargetArrows = 53 + (level - 25) / 2;
        else                  puzzle.TargetArrows = 60 + (level - 40) / 3;

        puzzle.TargetArrows = Math.Min(puzzle.TargetArrows, puzzle.Rows * puzzle.Cols - 2);

        puzzle.GenerateSolvable();
        return puzzle;
    }

    private void GenerateSolvable()
    {
        var board = new bool[Rows, Cols];
        var placed = new List<ArrowCell>();

        int maxAttempts = 2000;
        int attempts = 0;

        while (placed.Count < TargetArrows && attempts < maxAttempts)
        {
            attempts++;
            int r = Rng.Next(Rows);
            int c = Rng.Next(Cols);
            if (board[r, c]) continue;

            var dirs = new[] { ArrowDirection.Up, ArrowDirection.Down, ArrowDirection.Left, ArrowDirection.Right };
            var validDirs = new List<ArrowDirection>();

            foreach (var dir in dirs)
            {
                if (CanEscape(board, r, c, dir) && !CausesDeadlock(placed, r, c, dir))
                    validDirs.Add(dir);
            }

            if (validDirs.Count == 0) continue;

            var chosenDir = validDirs[Rng.Next(validDirs.Count)];
            var arrow = new ArrowCell { Row = r, Col = c, Direction = chosenDir };
            board[r, c] = true;
            placed.Add(arrow);
        }

        // Fill remaining without deadlock check (best effort)
        while (placed.Count < TargetArrows)
        {
            int r = Rng.Next(Rows);
            int c = Rng.Next(Cols);
            if (board[r, c]) continue;

            var dirs = new[] { ArrowDirection.Up, ArrowDirection.Down, ArrowDirection.Left, ArrowDirection.Right };
            var shuffled = dirs.OrderBy(_ => Rng.Next()).ToArray();
            bool placed2 = false;
            foreach (var dir in shuffled)
            {
                if (!CausesDeadlock(placed, r, c, dir))
                {
                    var arrow = new ArrowCell { Row = r, Col = c, Direction = dir };
                    board[r, c] = true;
                    placed.Add(arrow);
                    placed2 = true;
                    break;
                }
            }
            if (!placed2)
            {
                // Last resort
                var arrow = new ArrowCell { Row = r, Col = c, Direction = (ArrowDirection)Rng.Next(4) };
                board[r, c] = true;
                placed.Add(arrow);
            }
        }

        for (int i = placed.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (placed[i], placed[j]) = (placed[j], placed[i]);
        }

        Arrows = placed;
    }

    private bool CanEscape(bool[,] board, int row, int col, ArrowDirection dir)
    {
        return dir switch
        {
            ArrowDirection.Up => row == 0 || !board[row - 1, col],
            ArrowDirection.Down => row == Rows - 1 || !board[row + 1, col],
            ArrowDirection.Left => col == 0 || !board[row, col - 1],
            ArrowDirection.Right => col == Cols - 1 || !board[row, col + 1],
            _ => false
        };
    }

    /// <summary>
    /// Check if placing an arrow causes a deadlock (two arrows facing each other head-on).
    /// </summary>
    private bool CausesDeadlock(List<ArrowCell> placed, int row, int col, ArrowDirection dir)
    {
        // For each existing arrow, check if this new one faces it head-on
        foreach (var other in placed)
        {
            // Same row, facing each other horizontally
            if (other.Row == row)
            {
                // other is to the left of new, and other points Right while new points Left
                if (other.Col < col && other.Direction == ArrowDirection.Right && dir == ArrowDirection.Left)
                {
                    // Check no arrows between them
                    if (NoArrowsBetween(placed, row, other.Col + 1, col - 1, true))
                        return true;
                }
                // other is to the right of new, and other points Left while new points Right
                if (other.Col > col && other.Direction == ArrowDirection.Left && dir == ArrowDirection.Right)
                {
                    if (NoArrowsBetween(placed, row, col + 1, other.Col - 1, true))
                        return true;
                }
            }

            // Same col, facing each other vertically
            if (other.Col == col)
            {
                // other is above new, and other points Down while new points Up
                if (other.Row < row && other.Direction == ArrowDirection.Down && dir == ArrowDirection.Up)
                {
                    if (NoArrowsBetween(placed, other.Row + 1, row - 1, col, false))
                        return true;
                }
                // other is below new, and other points Up while new points Down
                if (other.Row > row && other.Direction == ArrowDirection.Up && dir == ArrowDirection.Down)
                {
                    if (NoArrowsBetween(placed, row + 1, other.Row - 1, col, false))
                        return true;
                }
            }
        }
        return false;
    }

    private bool NoArrowsBetween(List<ArrowCell> placed, int start, int end, int fixedCoord, bool sameRow)
    {
        foreach (var a in placed)
        {
            if (sameRow && a.Row == fixedCoord && a.Col >= start && a.Col <= end)
                return false;
            if (!sameRow && a.Col == fixedCoord && a.Row >= start && a.Row <= end)
                return false;
        }
        return true;
    }

    public List<(int Row, int Col)> SimulateMove(ArrowCell arrow)
    {
        var path = new List<(int Row, int Col)>();
        int r = arrow.Row;
        int c = arrow.Col;

        var occupied = new HashSet<(int, int)>();
        foreach (var a in Arrows)
        {
            if (!a.IsCleared && a != arrow)
                occupied.Add((a.Row, a.Col));
        }

        while (true)
        {
            int nr = r + arrow.Direction switch
            {
                ArrowDirection.Up => -1,
                ArrowDirection.Down => 1,
                _ => 0
            };
            int nc = c + arrow.Direction switch
            {
                ArrowDirection.Left => -1,
                ArrowDirection.Right => 1,
                _ => 0
            };

            if (nr < 0 || nr >= Rows || nc < 0 || nc >= Cols)
            {
                arrow.IsCleared = true;
                break;
            }

            if (occupied.Contains((nr, nc)))
                break;

            path.Add((nr, nc));
            r = nr;
            c = nc;
        }

        return path;
    }

    public int ClearedCount => Arrows.Count(a => a.IsCleared);
    public bool AllCleared => Arrows.All(a => a.IsCleared);
    public int RemainingCount => Arrows.Count(a => !a.IsCleared);
}
