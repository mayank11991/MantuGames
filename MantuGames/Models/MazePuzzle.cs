namespace MantuGames.Models;

public class MazeCell
{
    public bool WallTop    = true;
    public bool WallRight  = true;
    public bool WallBottom = true;
    public bool WallLeft   = true;
    public bool Visited    = false;
}

/// <summary>
/// A maze carved inside a picture shape (heart, cup, star, house…). Only cells inside
/// the shape (<see cref="Active"/>) are part of the maze; every level gets a different
/// shape. The entrance sits on the bottom edge of the shape and the exit is the cell
/// farthest from it along the maze, so the route always crosses the whole picture.
/// </summary>
public class MazePuzzle
{
    public int Rows { get; }
    public int Cols { get; }
    public MazeCell[,] Cells { get; }
    public bool[,] Active { get; }
    public string ShapeName { get; }
    public (int r, int c) Start { get; private set; }
    public (int r, int c) End   { get; private set; }

    private static readonly (int dr, int dc)[] Dirs = { (-1, 0), (0, 1), (1, 0), (0, -1) };

    public MazePuzzle(int rows, int cols, MazeShape shape, int seed)
    {
        Rows = rows;
        Cols = cols;
        ShapeName = shape.Name;
        Cells = new MazeCell[rows, cols];
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            Cells[r, c] = new MazeCell();

        Active = BuildMask(shape);
        var rng = new Random(seed);

        // Entrance: bottom-most active cell nearest the middle (like the cup's foot).
        Start = AllActive()
            .OrderByDescending(p => p.r)
            .ThenBy(p => Math.Abs(p.c - (cols - 1) / 2.0))
            .First();

        if ((seed / 7) % 2 == 0) CarveBacktracker(rng);
        else CarvePrim(rng);

        End = Farthest(Start);
    }

    public bool IsActive(int r, int c) => r >= 0 && r < Rows && c >= 0 && c < Cols && Active[r, c];

    public bool CanMove(int r, int c, int dr, int dc)
    {
        if (!IsActive(r, c) || !IsActive(r + dr, c + dc)) return false;
        var cell = Cells[r, c];
        return (dr, dc) switch
        {
            (-1, 0) => !cell.WallTop,
            (1, 0)  => !cell.WallBottom,
            (0, -1) => !cell.WallLeft,
            (0, 1)  => !cell.WallRight,
            _ => false,
        };
    }

    /// <summary>Shortest route from <paramref name="from"/> to the exit (inclusive), or null.</summary>
    public List<(int r, int c)>? PathTo((int r, int c) from, (int r, int c) to)
    {
        var prev = new Dictionary<(int, int), (int, int)> { [from] = (-1, -1) };
        var q = new Queue<(int r, int c)>();
        q.Enqueue(from);
        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == to) break;
            foreach (var (dr, dc) in Dirs)
            {
                var n = (cur.r + dr, cur.c + dc);
                if (prev.ContainsKey(n) || !CanMove(cur.r, cur.c, dr, dc)) continue;
                prev[n] = cur;
                q.Enqueue(n);
            }
        }
        if (!prev.ContainsKey(to)) return null;
        var path = new List<(int r, int c)>();
        for (var p = to; p != (-1, -1); p = prev[p]) path.Add(p);
        path.Reverse();
        return path;
    }

    // ── Mask ────────────────────────────────────────────────────────────────
    private bool[,] BuildMask(MazeShape shape)
    {
        var m = new bool[Rows, Cols];
        for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
        {
            double u = (c + 0.5) / Cols * 2 - 1;
            double v = (r + 0.5) / Rows * 2 - 1;
            m[r, c] = shape.Contains(u, v);
        }

        // Keep only the largest 4-connected piece so every cell is reachable.
        var seen = new bool[Rows, Cols];
        List<(int, int)> best = new();
        for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
        {
            if (!m[r, c] || seen[r, c]) continue;
            var comp = new List<(int, int)>();
            var q = new Queue<(int r, int c)>();
            q.Enqueue((r, c)); seen[r, c] = true;
            while (q.Count > 0)
            {
                var (cr, cc) = q.Dequeue();
                comp.Add((cr, cc));
                foreach (var (dr, dc) in Dirs)
                {
                    int nr = cr + dr, nc = cc + dc;
                    if (nr < 0 || nr >= Rows || nc < 0 || nc >= Cols || !m[nr, nc] || seen[nr, nc]) continue;
                    seen[nr, nc] = true;
                    q.Enqueue((nr, nc));
                }
            }
            if (comp.Count > best.Count) best = comp;
        }

        var result = new bool[Rows, Cols];
        foreach (var (r, c) in best) result[r, c] = true;
        return result;
    }

    private IEnumerable<(int r, int c)> AllActive()
    {
        for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
            if (Active[r, c]) yield return (r, c);
    }

    // ── Carving ─────────────────────────────────────────────────────────────
    private void Open(int r, int c, int dr, int dc)
    {
        var a = Cells[r, c];
        var b = Cells[r + dr, c + dc];
        switch ((dr, dc))
        {
            case (-1, 0): a.WallTop = false;    b.WallBottom = false; break;
            case (1, 0):  a.WallBottom = false; b.WallTop = false;    break;
            case (0, -1): a.WallLeft = false;   b.WallRight = false;  break;
            case (0, 1):  a.WallRight = false;  b.WallLeft = false;   break;
        }
    }

    /// <summary>Recursive backtracker: long winding corridors.</summary>
    private void CarveBacktracker(Random rng)
    {
        var stack = new Stack<(int r, int c)>();
        Cells[Start.r, Start.c].Visited = true;
        stack.Push(Start);
        while (stack.Count > 0)
        {
            var (r, c) = stack.Peek();
            var options = Dirs.Where(d => IsActive(r + d.dr, c + d.dc) && !Cells[r + d.dr, c + d.dc].Visited).ToList();
            if (options.Count == 0) { stack.Pop(); continue; }
            var (dr, dc) = options[rng.Next(options.Count)];
            Open(r, c, dr, dc);
            Cells[r + dr, c + dc].Visited = true;
            stack.Push((r + dr, c + dc));
        }
    }

    /// <summary>Randomised Prim: more branches and dead ends.</summary>
    private void CarvePrim(Random rng)
    {
        var frontier = new List<(int r, int c, int dr, int dc)>();
        void AddFrontier(int r, int c)
        {
            foreach (var (dr, dc) in Dirs)
                if (IsActive(r + dr, c + dc) && !Cells[r + dr, c + dc].Visited)
                    frontier.Add((r, c, dr, dc));
        }

        Cells[Start.r, Start.c].Visited = true;
        AddFrontier(Start.r, Start.c);
        while (frontier.Count > 0)
        {
            int i = rng.Next(frontier.Count);
            var (r, c, dr, dc) = frontier[i];
            frontier.RemoveAt(i);
            if (Cells[r + dr, c + dc].Visited) continue;
            Open(r, c, dr, dc);
            Cells[r + dr, c + dc].Visited = true;
            AddFrontier(r + dr, c + dc);
        }
    }

    private (int r, int c) Farthest((int r, int c) from)
    {
        var dist = new Dictionary<(int, int), int> { [from] = 0 };
        var q = new Queue<(int r, int c)>();
        q.Enqueue(from);
        var far = from;
        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (dist[cur] > dist[far]) far = cur;
            foreach (var (dr, dc) in Dirs)
            {
                var n = (cur.r + dr, cur.c + dc);
                if (dist.ContainsKey(n) || !CanMove(cur.r, cur.c, dr, dc)) continue;
                dist[n] = dist[cur] + 1;
                q.Enqueue(n);
            }
        }
        return far;
    }

    // ── Factory: a new picture every level, bigger as levels go up ──────────
    public static MazePuzzle ForLevel(int level)
    {
        level = Math.Max(1, level);
        // Small grids early so every cell is big and easy to see; star needs a bit more room.
        int size = level <= 5 ? 9 : level <= 14 ? 11 : level <= 29 ? 13 : 15;

        var shape = MazeShape.All[(level - 1) % MazeShape.All.Length];
        size = Math.Max(size, shape.MinSize);
        // Phone screens are tall, so pictures are stretched a little vertically to use the
        // whole play area instead of leaving empty bands above and below.
        int rows = size + 3, cols = size;
        if (shape.Tall) rows += 2;
        if (shape.Wide) { cols += 2; rows -= 2; }

        return new MazePuzzle(rows, cols, shape, seed: level * 137 + 42);
    }
}

/// <summary>A picture outline in normalised coordinates: u, v ∈ [-1, 1], v grows downwards.</summary>
public sealed class MazeShape
{
    public string Name { get; }
    public bool Tall { get; init; }
    public int MinSize { get; init; } = 9;
    public bool Wide { get; init; }
    private readonly Func<double, double, bool> _contains;

    public MazeShape(string name, Func<double, double, bool> contains)
    {
        Name = name;
        _contains = contains;
    }

    public bool Contains(double u, double v) => _contains(u, v);

    private static bool InPoly(double x, double y, (double x, double y)[] poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            var (x1, y1) = poly[i];
            var (x2, y2) = poly[j];
            if ((y1 > y) != (y2 > y) && x < (x2 - x1) * (y - y1) / (y2 - y1) + x1)
                inside = !inside;
        }
        return inside;
    }

    private static readonly (double, double)[] StarPoly = Enumerable.Range(0, 10).Select(i =>
    {
        double a = (-90 + 36 * i) * Math.PI / 180;
        double rad = i % 2 == 0 ? 1.0 : 0.5;
        return (Math.Cos(a) * rad, Math.Sin(a) * rad + 0.1);
    }).ToArray();

    private static readonly (double, double)[] HousePoly =
        { (0, -1), (1, -0.15), (0.78, -0.15), (0.78, 1), (-0.78, 1), (-0.78, -0.15), (-1, -0.15) };

    private static readonly (double, double)[] TreePoly =
    {
        (0, -1), (0.55, -0.45), (0.3, -0.45), (0.8, 0.15), (0.45, 0.15), (1, 0.7), (0.22, 0.7),
        (0.22, 1), (-0.22, 1), (-0.22, 0.7), (-1, 0.7), (-0.45, 0.15), (-0.8, 0.15), (-0.3, -0.45), (-0.55, -0.45),
    };

    private static readonly (double, double)[] RocketPoly =
    {
        (0, -1), (0.4, -0.5), (0.42, 0.45), (0.8, 0.82), (0.8, 1), (-0.8, 1), (-0.8, 0.82), (-0.42, 0.45), (-0.4, -0.5),
    };

    private static readonly (double, double)[] FishTail = { (0.42, 0), (1, -0.62), (1, 0.62) };

    public static readonly MazeShape[] All =
    {
        new("Circle", (u, v) => u * u + v * v <= 1.0),
        new("Heart", (u, v) =>
        {
            double x = u * 1.18, y = -v * 1.18 + 0.18;
            double a = x * x + y * y - 1;
            return a * a * a - x * x * y * y * y <= 0;
        }),
        new("Diamond", (u, v) => Math.Abs(u) + Math.Abs(v) <= 1.05),
        new("Sundae", (u, v) =>
        {
            if (v <= 0.35)
            {
                // Rounded ice-cream scoop on top, glass bowl below.
                double half = v < -0.55 ? 0.96 * Math.Sqrt(Math.Max(0, 1 - Math.Pow((v + 0.55) / 0.5, 2)))
                            : v <= -0.2 ? 0.96
                            : 0.96 * Math.Sqrt(Math.Max(0, 1 - Math.Pow((v + 0.2) / 0.62, 2)));
                return Math.Abs(u) <= half;
            }
            if (v <= 0.72) return Math.Abs(u) <= 0.17;
            return Math.Abs(u) <= 0.72;
        }) { Tall = true },
        new("House", (u, v) => InPoly(u, v, HousePoly)),
        new("Star", (u, v) => InPoly(u, v, StarPoly)) { MinSize = 13 },
        new("Plus", (u, v) => Math.Abs(u) <= 0.4 || Math.Abs(v) <= 0.4),
        new("Fish", (u, v) => Math.Pow((u + 0.15) / 0.68, 2) + Math.Pow(v / 0.64, 2) <= 1 || InPoly(u, v, FishTail)) { Wide = true },
        new("Tree", (u, v) => InPoly(u, v, TreePoly)) { Tall = true },
        new("Donut", (u, v) => { double d = u * u + v * v; return d <= 1.0 && d >= 0.16; }),
        new("Rocket", (u, v) => InPoly(u, v, RocketPoly)) { Tall = true },
    };
}
