using Microsoft.Maui.Graphics;

namespace MantuGames.Models;

public class CtdPuzzle
{
    public int Rows { get; }
    public int Cols { get; }
    public List<CtdPair> Pairs { get; }
    public int[,] Grid { get; }

    public CtdPuzzle(int rows, int cols, List<CtdPair> pairs)
    {
        Rows = rows;
        Cols = cols;
        Pairs = pairs;
        Grid = new int[rows, cols];

        for (int i = 0; i < pairs.Count; i++)
        {
            var p = pairs[i];
            Grid[p.R1, p.C1] = i + 1;
            Grid[p.R2, p.C2] = i + 1;
        }
    }

    public static CtdPuzzle Generate(int level)
    {
        int rows, cols, pairCount;
        if (level <= 5) { rows = 5; cols = 5; pairCount = 3; }
        else if (level <= 10) { rows = 6; cols = 6; pairCount = 4; }
        else if (level <= 15) { rows = 6; cols = 7; pairCount = 5; }
        else if (level <= 20) { rows = 7; cols = 7; pairCount = 6; }
        else { rows = 7; cols = 8; pairCount = 7; }

        var rng = new Random(level * 7919);
        var colors = new[]
        {
            Color.FromArgb("#9C27B0"), // Purple
            Color.FromArgb("#4CAF50"), // Green
            Color.FromArgb("#00BCD4"), // Cyan
            Color.FromArgb("#FF9800"), // Orange
            Color.FromArgb("#E91E63"), // Pink
            Color.FromArgb("#2196F3"), // Blue
            Color.FromArgb("#FFEB3B"), // Yellow
            Color.FromArgb("#F44336"), // Red
            Color.FromArgb("#009688"), // Teal
            Color.FromArgb("#FF5722"), // Deep Orange
        };

        for (int attempt = 0; attempt < 500; attempt++)
        {
            var pairs = new List<CtdPair>();
            var occupied = new bool[rows, cols];
            bool ok = true;

            for (int i = 0; i < pairCount; i++)
            {
                var freeCells = new List<(int r, int c)>();
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        if (!occupied[r, c])
                            freeCells.Add((r, c));

                if (freeCells.Count < 2) { ok = false; break; }

                var start = freeCells[rng.Next(freeCells.Count)];
                occupied[start.r, start.c] = true;

                var endCandidates = new List<(int r, int c)>();
                foreach (var cell in freeCells)
                {
                    if (cell.r == start.r && cell.c == start.c) continue;
                    int dist = Math.Abs(cell.r - start.r) + Math.Abs(cell.c - start.c);
                    if (dist >= 2)
                        endCandidates.Add(cell);
                }

                if (endCandidates.Count == 0) { ok = false; break; }

                var end = endCandidates[rng.Next(endCandidates.Count)];
                occupied[end.r, end.c] = true;

                pairs.Add(new CtdPair
                {
                    R1 = start.r, C1 = start.c,
                    R2 = end.r, C2 = end.c,
                    Color = colors[i % colors.Length]
                });
            }

            if (!ok) continue;

            if (BfsFill(pairs, rows, cols))
                return new CtdPuzzle(rows, cols, pairs);
        }

        return CreateFallback(rows, cols, pairCount, colors);
    }

    private static bool BfsFill(List<CtdPair> pairs, int rows, int cols)
    {
        int pairCount = pairs.Count;
        int totalCells = rows * cols;

        var grid = new int[rows, cols];
        for (int i = 0; i < pairCount; i++)
        {
            grid[pairs[i].R1, pairs[i].C1] = i + 1;
            grid[pairs[i].R2, pairs[i].C2] = i + 1;
        }

        var pathCells = new HashSet<(int, int)>();
        var solutionPaths = new Dictionary<int, List<(int, int)>>();

        for (int i = 0; i < pairCount; i++)
        {
            var p = pairs[i];
            var path = FindPath(grid, rows, cols, p.R1, p.C1, p.R2, p.C2, pathCells);
            if (path == null) return false;
            solutionPaths[i + 1] = path;
            foreach (var cell in path)
                pathCells.Add(cell);
        }

        int filled = pathCells.Count;
        if (filled < totalCells * 0.7f) return false;

        return true;
    }

    private static List<(int, int)> FindPath(int[,] grid, int rows, int cols,
        int sr, int sc, int er, int ec, HashSet<(int, int)> occupied)
    {
        var visited = new bool[rows, cols];
        var parent = new (int, int)[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                parent[r, c] = (-1, -1);

        var queue = new Queue<(int, int)>();
        queue.Enqueue((sr, sc));
        visited[sr, sc] = true;

        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();

            if (r == er && c == ec)
            {
                var path = new List<(int, int)>();
                var cur = (r, c);
                while (cur != (-1, -1))
                {
                    path.Add(cur);
                    cur = parent[cur.Item1, cur.Item2];
                }
                path.Reverse();
                return path;
            }

            for (int d = 0; d < 4; d++)
            {
                int nr = r + dr[d], nc = c + dc[d];
                if (nr < 0 || nr >= rows || nc < 0 || nc >= cols) continue;
                if (visited[nr, nc]) continue;
                if (occupied.Contains((nr, nc))) continue;
                int cellVal = grid[nr, nc];
                if (cellVal != 0 && cellVal != grid[sr, sc]) continue;

                visited[nr, nc] = true;
                parent[nr, nc] = (r, c);
                queue.Enqueue((nr, nc));
            }
        }

        return null;
    }

    private static CtdPuzzle CreateFallback(int rows, int cols, int pairCount, Color[] colors)
    {
        var pairs = new List<CtdPair>();
        for (int i = 0; i < pairCount && i < rows; i++)
        {
            pairs.Add(new CtdPair
            {
                R1 = i, C1 = 0,
                R2 = i, C2 = cols - 1,
                Color = colors[i % colors.Length]
            });
        }
        return new CtdPuzzle(rows, cols, pairs);
    }
}

public class CtdPair
{
    public int R1 { get; set; }
    public int C1 { get; set; }
    public int R2 { get; set; }
    public int C2 { get; set; }
    public Color Color { get; set; }
}
