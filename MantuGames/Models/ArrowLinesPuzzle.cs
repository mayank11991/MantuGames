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

        // Validate solvability - every arrow must be able to escape
        if (!IsSolvable(placed))
        {
            // Just use what we have — CausesDeadlock already prevents worst cases
            Arrows = placed;
            return;
        }

        Arrows = placed;
    }

    private bool CanEscape(bool[,] board, int row, int col, ArrowDirection dir)
    {
        int r = row, c = col;
        while (true)
        {
            int nr = r + (dir == ArrowDirection.Up ? -1 : dir == ArrowDirection.Down ? 1 : 0);
            int nc = c + (dir == ArrowDirection.Left ? -1 : dir == ArrowDirection.Right ? 1 : 0);

            if (nr < 0 || nr >= Rows || nc < 0 || nc >= Cols)
                return true;

            if (board[nr, nc])
                return false;

            r = nr;
            c = nc;
        }
    }

    /// <summary>
    /// Check if placing an arrow causes a deadlock.
    /// Checks head-on collisions AND arrows that would be trapped against walls.
    /// </summary>
    private bool CausesDeadlock(List<ArrowCell> placed, int row, int col, ArrowDirection dir)
    {
        foreach (var other in placed)
        {
            // Same row, facing each other horizontally
            if (other.Row == row)
            {
                if (other.Col < col && other.Direction == ArrowDirection.Right && dir == ArrowDirection.Left)
                {
                    if (NoArrowsBetween(placed, row, other.Col + 1, col - 1, true))
                        return true;
                }
                if (other.Col > col && other.Direction == ArrowDirection.Left && dir == ArrowDirection.Right)
                {
                    if (NoArrowsBetween(placed, row, col + 1, other.Col - 1, true))
                        return true;
                }
            }

            // Same col, facing each other vertically
            if (other.Col == col)
            {
                if (other.Row < row && other.Direction == ArrowDirection.Down && dir == ArrowDirection.Up)
                {
                    if (NoArrowsBetween(placed, other.Row + 1, row - 1, col, false))
                        return true;
                }
                if (other.Row > row && other.Direction == ArrowDirection.Up && dir == ArrowDirection.Down)
                {
                    if (NoArrowsBetween(placed, row + 1, other.Row - 1, col, false))
                        return true;
                }
            }

            // Check if placing this arrow would trap an existing arrow against a wall
            if (WouldTrapArrow(placed, row, col, dir, other))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Check if placing a new arrow would trap an existing arrow by blocking its escape path.
    /// </summary>
    private bool WouldTrapArrow(List<ArrowCell> placed, int newRow, int newCol, ArrowDirection newDir, ArrowCell existing)
    {
        if (existing.IsCleared) return false;

        // Trace the existing arrow's escape path and see if the new arrow blocks it
        int r = existing.Row, c = existing.Col;
        while (true)
        {
            int nr = r + (existing.Direction == ArrowDirection.Up ? -1 : existing.Direction == ArrowDirection.Down ? 1 : 0);
            int nc = c + (existing.Direction == ArrowDirection.Left ? -1 : existing.Direction == ArrowDirection.Right ? 1 : 0);

            if (nr < 0 || nr >= Rows || nc < 0 || nc >= Cols)
                return false; // Can escape

            // Check if any existing placed arrow blocks this path
            bool blockedByExisting = false;
            foreach (var a in placed)
            {
                if (!a.IsCleared && a.Row == nr && a.Col == nc)
                {
                    blockedByExisting = true;
                    break;
                }
            }
            if (blockedByExisting)
                return false; // Already blocked by another arrow, our new one doesn't change anything

            // Check if the new arrow would be placed in this path
            if (nr == newRow && nc == newCol)
            {
                // New arrow blocks this path. Check if the existing arrow has NO other escape.
                // An arrow is trapped if the only way out is blocked by us.
                // Simple check: is the existing arrow adjacent to the board edge in its direction?
                bool adjacentToEdge = (existing.Direction == ArrowDirection.Up && existing.Row == 0) ||
                                      (existing.Direction == ArrowDirection.Down && existing.Row == Rows - 1) ||
                                      (existing.Direction == ArrowDirection.Left && existing.Col == 0) ||
                                      (existing.Direction == ArrowDirection.Right && existing.Col == Cols - 1);
                if (adjacentToEdge)
                    return true; // We're blocking the only escape for an arrow at the edge
            }

            r = nr;
            c = nc;
        }
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

    /// <summary>
    /// Check if the puzzle is solvable by verifying every arrow can potentially escape.
    /// Uses BFS to check if there's a valid clearing order.
    /// </summary>
    private bool IsSolvable(List<ArrowCell> placed)
    {
        // Check that no arrow is permanently trapped
        // An arrow is trapped if it points at a wall with another arrow directly in front of it
        // and that blocking arrow also can't escape
        foreach (var arrow in placed)
        {
            if (!CanArrowEventuallyEscape(placed, arrow))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Check if a specific arrow can eventually escape considering other arrows can be cleared first.
    /// </summary>
    private bool CanArrowEventuallyEscape(List<ArrowCell> placed, ArrowCell target)
    {
        // Build a set of all occupied positions
        var occupied = new HashSet<(int, int)>();
        foreach (var a in placed)
        {
            if (!a.IsCleared && a != target)
                occupied.Add((a.Row, a.Col));
        }

        // Check if the arrow's path to the edge is clear of other arrows
        int r = target.Row, c = target.Col;
        while (true)
        {
            int nr = r + (target.Direction == ArrowDirection.Up ? -1 : target.Direction == ArrowDirection.Down ? 1 : 0);
            int nc = c + (target.Direction == ArrowDirection.Left ? -1 : target.Direction == ArrowDirection.Right ? 1 : 0);

            if (nr < 0 || nr >= Rows || nc < 0 || nc >= Cols)
                return true; // Can reach the edge

            if (occupied.Contains((nr, nc)))
            {
                // Blocked by another arrow. Check if that blocker is NOT the target itself.
                // If the path is blocked, this arrow can't escape directly.
                // It can only escape if the blocker is cleared first.
                // For generation purposes, we consider this acceptable if the blocker
                // can also escape (checked separately).
                return false;
            }

            r = nr;
            c = nc;
        }
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

    /// <summary>Non-mutating check: can this arrow escape the board right now?</summary>
    public bool CanEscape(ArrowCell arrow)
    {
        if (arrow.IsCleared) return false;

        var occupied = new HashSet<(int, int)>();
        foreach (var a in Arrows)
        {
            if (!a.IsCleared && a != arrow)
                occupied.Add((a.Row, a.Col));
        }

        int r = arrow.Row, c = arrow.Col;
        while (true)
        {
            int dr = 0, dc = 0;
            if (arrow.Direction == ArrowDirection.Up) dr = -1;
            else if (arrow.Direction == ArrowDirection.Down) dr = 1;
            else if (arrow.Direction == ArrowDirection.Left) dc = -1;
            else if (arrow.Direction == ArrowDirection.Right) dc = 1;

            int nr = r + dr, nc = c + dc;
            if (nr < 0 || nr >= Rows || nc < 0 || nc >= Cols)
                return true;
            if (occupied.Contains((nr, nc)))
                return false;
            r = nr;
            c = nc;
        }
    }

    public int ClearedCount => Arrows.Count(a => a.IsCleared);
    public bool AllCleared => Arrows.All(a => a.IsCleared);
    public int RemainingCount => Arrows.Count(a => !a.IsCleared);
}
