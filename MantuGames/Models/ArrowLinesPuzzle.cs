namespace MantuGames.Models;

public enum ArrowDirection { Up, Down, Left, Right }

/// <summary>
/// One bendy line on the board. <see cref="Cells"/> runs tail → head; the arrow is
/// drawn at the head, pointing in <see cref="Direction"/>. Tapping the line makes it
/// slither forward along its own path and straight out of the board.
/// </summary>
public class ArrowLine
{
    public int Id { get; init; }
    public List<(int Row, int Col)> Cells { get; } = new();
    public ArrowDirection Direction { get; set; }
    public bool IsCleared { get; set; }

    public (int Row, int Col) Head => Cells[^1];

    public static (int dr, int dc) Step(ArrowDirection d) => d switch
    {
        ArrowDirection.Up => (-1, 0),
        ArrowDirection.Down => (1, 0),
        ArrowDirection.Left => (0, -1),
        _ => (0, 1),
    };
}

/// <summary>
/// Board of bendy arrow lines. Generated backwards so it is always solvable: lines are
/// placed in reverse removal order, and each new line's exit ray (the straight run from
/// its head to the board edge) must be free of every line already placed — exactly the
/// lines that will still be on the board when this one is removed.
/// </summary>
public class ArrowLinesPuzzle
{
    public int Rows { get; private set; }
    public int Cols { get; private set; }
    public List<ArrowLine> Lines { get; } = new();

    private int[,] _owner = new int[0, 0];   // line id per cell, -1 = empty

    public int RemainingCount => Lines.Count(l => !l.IsCleared);
    public bool AllCleared => Lines.All(l => l.IsCleared);

    public static ArrowLinesPuzzle Generate(int level, Random? rng = null)
    {
        rng ??= Random.Shared;
        int size = Math.Clamp(5 + (level - 1) / 3, 5, 11);              // 5x5 … 11x11
        int maxLen = Math.Clamp(3 + level / 2, 3, 10);                   // longer, bendier lines later
        double fill = Math.Clamp(0.55 + level * 0.012, 0.55, 0.82);      // denser boards later

        // A few tries; keep the densest board.
        ArrowLinesPuzzle? best = null;
        for (int attempt = 0; attempt < 6; attempt++)
        {
            var p = new ArrowLinesPuzzle { Rows = size, Cols = size };
            p.Build(rng, maxLen, fill);
            if (best == null || p.FilledCells > best.FilledCells) best = p;
            if (p.FilledCells >= size * size * fill) break;
        }
        return best!;
    }

    private int FilledCells => Lines.Sum(l => l.Cells.Count);

    private void Build(Random rng, int maxLen, double fill)
    {
        _owner = new int[Rows, Cols];
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                _owner[r, c] = -1;

        int target = (int)(Rows * Cols * fill);
        int failures = 0;
        var dirs = new[] { ArrowDirection.Up, ArrowDirection.Down, ArrowDirection.Left, ArrowDirection.Right };

        while (FilledCells < target && failures < 400)
        {
            int hr = rng.Next(Rows), hc = rng.Next(Cols);
            if (_owner[hr, hc] != -1) { failures++; continue; }

            var dir = dirs[rng.Next(4)];
            var ray = RayCells(hr, hc, dir);
            if (ray.Any(p => _owner[p.Row, p.Col] != -1)) { failures++; continue; }

            // The cell right behind the head keeps the arrow lined up with the line.
            var (dr, dc) = ArrowLine.Step(dir);
            int br = hr - dr, bc = hc - dc;
            if (!InBounds(br, bc) || _owner[br, bc] != -1) { failures++; continue; }

            var rayset = new HashSet<(int, int)>(ray);
            var body = new List<(int Row, int Col)> { (hr, hc), (br, bc) };
            int len = rng.Next(2, maxLen + 1);
            var heading = (-dr, -dc);   // walking backwards from the head
            while (body.Count < len)
            {
                var (cr, cc) = body[^1];
                // Prefer going straight, sometimes turn — gives the snaky look.
                var options = new List<(int, int)> { heading, (heading.Item2, heading.Item1), (-heading.Item2, -heading.Item1) };
                if (rng.NextDouble() < 0.45) options.Reverse();
                bool grew = false;
                foreach (var (sr, sc) in options.OrderBy(_ => rng.NextDouble() < 0.6 ? 0 : 1))
                {
                    int nr = cr + sr, nc = cc + sc;
                    if (!InBounds(nr, nc) || _owner[nr, nc] != -1) continue;
                    if (body.Contains((nr, nc)) || rayset.Contains((nr, nc))) continue;
                    body.Add((nr, nc));
                    heading = (sr, sc);
                    grew = true;
                    break;
                }
                if (!grew) break;
            }

            body.Reverse();   // tail → head
            var line = new ArrowLine { Id = Lines.Count, Direction = dir };
            line.Cells.AddRange(body);
            foreach (var (r, c) in body) _owner[r, c] = line.Id;
            Lines.Add(line);
            failures = 0;
        }
    }

    public bool InBounds(int r, int c) => r >= 0 && r < Rows && c >= 0 && c < Cols;

    /// <summary>Cells from just past the head to the board edge.</summary>
    public List<(int Row, int Col)> RayCells(int r, int c, ArrowDirection d)
    {
        var (dr, dc) = ArrowLine.Step(d);
        var list = new List<(int Row, int Col)>();
        for (r += dr, c += dc; InBounds(r, c); r += dr, c += dc) list.Add((r, c));
        return list;
    }

    /// <summary>
    /// How many free cells the line can travel before bumping into another line;
    /// -1 means its way out is clear.
    /// </summary>
    public int FreeRunBeforeBlock(ArrowLine line)
    {
        var ray = RayCells(line.Head.Row, line.Head.Col, line.Direction);
        for (int i = 0; i < ray.Count; i++)
        {
            int owner = _owner[ray[i].Row, ray[i].Col];
            if (owner != -1 && owner != line.Id) return i;
        }
        return -1;
    }

    public bool CanEscape(ArrowLine line) => !line.IsCleared && FreeRunBeforeBlock(line) < 0;

    public void Remove(ArrowLine line)
    {
        line.IsCleared = true;
        foreach (var (r, c) in line.Cells)
            if (_owner[r, c] == line.Id) _owner[r, c] = -1;
    }

    /// <summary>Line covering a cell, or null.</summary>
    public ArrowLine? LineAt(int r, int c)
    {
        if (!InBounds(r, c)) return null;
        int id = _owner[r, c];
        return id < 0 ? null : Lines[id];
    }
}
