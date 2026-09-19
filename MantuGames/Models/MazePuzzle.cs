namespace MantuGames.Models;

public class MazeCell
{
    public bool WallTop    = true;
    public bool WallRight  = true;
    public bool WallBottom = true;
    public bool WallLeft   = true;
    public bool Visited    = false;
}

public class MazePuzzle
{
    public int Rows { get; }
    public int Cols { get; }
    public MazeCell[,] Cells { get; }
    public (int r, int c) Start { get; } = (0, 0);
    public (int r, int c) End   { get; }

    public MazePuzzle(int rows, int cols, int seed = 0)
    {
        Rows  = rows;
        Cols  = cols;
        Cells = new MazeCell[rows, cols];
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            Cells[r, c] = new MazeCell();

        End = (rows - 1, cols - 1);
        Generate(seed);
    }

    // ── Recursive-backtracking (winding paths) ──────────────────────────────
    private void GenerateRecursive(int seed)
    {
        var rng   = new Random(seed);
        var stack = new Stack<(int r, int c)>();

        Cells[0, 0].Visited = true;
        stack.Push((0, 0));

        int[] dr = { -1,  0, 1,  0 };
        int[] dc = {  0,  1, 0, -1 };

        while (stack.Count > 0)
        {
            var (r, c) = stack.Peek();

            var unvisited = new List<int>(4);
            for (int d = 0; d < 4; d++)
            {
                int nr = r + dr[d], nc = c + dc[d];
                if (nr >= 0 && nr < Rows && nc >= 0 && nc < Cols && !Cells[nr, nc].Visited)
                    unvisited.Add(d);
            }

            if (unvisited.Count == 0) { stack.Pop(); continue; }

            int dir  = unvisited[rng.Next(unvisited.Count)];
            int nr2  = r + dr[dir];
            int nc2  = c + dc[dir];

            switch (dir)
            {
                case 0: Cells[r,  c ].WallTop    = false; Cells[nr2, nc2].WallBottom = false; break;
                case 1: Cells[r,  c ].WallRight   = false; Cells[nr2, nc2].WallLeft   = false; break;
                case 2: Cells[r,  c ].WallBottom  = false; Cells[nr2, nc2].WallTop    = false; break;
                case 3: Cells[r,  c ].WallLeft    = false; Cells[nr2, nc2].WallRight  = false; break;
            }

            Cells[nr2, nc2].Visited = true;
            stack.Push((nr2, nc2));
        }
    }

    // ── Prim's algorithm (more branching, open feel) ────────────────────────
    private void GeneratePrim(int seed)
    {
        var rng = new Random(seed);
        var inMaze = new bool[Rows, Cols];
        var frontier = new List<(int r, int c, int fromR, int fromC, int dir)>();

        inMaze[0, 0] = true;
        AddFrontiers(0, 0, inMaze, frontier, rng);

        int[] dr = { -1, 0, 1, 0 };
        int[] dc = { 0, 1, 0, -1 };

        while (frontier.Count > 0)
        {
            int idx = rng.Next(frontier.Count);
            var (r, c, fr, fc, dir) = frontier[idx];
            frontier.RemoveAt(idx);

            if (inMaze[r, c]) continue;

            inMaze[r, c] = true;

            // Carve wall between (fr,fc) and (r,c)
            switch (dir)
            {
                case 0: Cells[fr, fc].WallTop = false;    Cells[r, c].WallBottom = false; break;
                case 1: Cells[fr, fc].WallRight = false;  Cells[r, c].WallLeft = false;   break;
                case 2: Cells[fr, fc].WallBottom = false; Cells[r, c].WallTop = false;    break;
                case 3: Cells[fr, fc].WallLeft = false;   Cells[r, c].WallRight = false;  break;
            }

            AddFrontiers(r, c, inMaze, frontier, rng);
        }
    }

    private void AddFrontiers(int r, int c, bool[,] inMaze,
        List<(int r, int c, int fromR, int fromC, int dir)> frontier, Random rng)
    {
        int[] dr = { -1, 0, 1, 0 };
        int[] dc = { 0, 1, 0, -1 };
        for (int d = 0; d < 4; d++)
        {
            int nr = r + dr[d], nc = c + dc[d];
            if (nr >= 0 && nr < Rows && nc >= 0 && nc < Cols && !inMaze[nr, nc])
                frontier.Add((nr, nc, r, c, d));
        }
    }

    // ── Kruskal-inspired (clusters merge, varied shapes) ────────────────────
    private void GenerateKruskal(int seed)
    {
        var rng = new Random(seed);
        int total = Rows * Cols;
        var parent = new int[total];
        for (int i = 0; i < total; i++) parent[i] = i;

        var edges = new List<(int a, int b, int r, int c, int dir)>();
        int[] dr = { -1, 0, 1, 0 };
        int[] dc = { 0, 1, 0, -1 };

        for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
        for (int d = 0; d < 4; d++)
        {
            int nr = r + dr[d], nc = c + dc[d];
            if (nr >= 0 && nr < Rows && nc >= 0 && nc < Cols)
                edges.Add((r * Cols + c, nr * Cols + nc, r, c, d));
        }

        // Shuffle edges
        for (int i = edges.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (edges[i], edges[j]) = (edges[j], edges[i]);
        }

        foreach (var (a, b, r, c, dir) in edges)
        {
            int ra = Find(parent, a), rb = Find(parent, b);
            if (ra == rb) continue;

            parent[ra] = rb;
            int nr = r + dr[dir], nc = c + dc[dir];
            switch (dir)
            {
                case 0: Cells[r, c].WallTop = false;    Cells[nr, nc].WallBottom = false; break;
                case 1: Cells[r, c].WallRight = false;  Cells[nr, nc].WallLeft = false;   break;
                case 2: Cells[r, c].WallBottom = false; Cells[nr, nc].WallTop = false;    break;
                case 3: Cells[r, c].WallLeft = false;   Cells[nr, nc].WallRight = false;  break;
            }
        }
    }

    private static int Find(int[] parent, int x)
    {
        while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; }
        return x;
    }

    // ── Recursive-division (rooms + corridors feel) ─────────────────────────
    private void GenerateDivision(int seed)
    {
        // Start with all walls carved (open grid), then add walls
        for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Cols; c++)
        {
            Cells[r, c].WallTop = false;
            Cells[r, c].WallRight = false;
            Cells[r, c].WallBottom = false;
            Cells[r, c].WallLeft = false;
        }

        // Add border walls
        for (int c = 0; c < Cols; c++)
        {
            Cells[0, c].WallTop = true;
            Cells[Rows - 1, c].WallBottom = true;
        }
        for (int r = 0; r < Rows; r++)
        {
            Cells[r, 0].WallLeft = true;
            Cells[r, Cols - 1].WallRight = true;
        }

        var rng = new Random(seed);
        Divide(1, 1, Cols - 2, Rows - 2, rng);
    }

    private void Divide(int x, int y, int w, int h, Random rng)
    {
        if (w < 2 || h < 2) return;

        bool horizontal = h > w ? true : w > h ? false : rng.Next(2) == 0;

        if (horizontal)
        {
            if (h < 3) return;
            int wallY = y + 2 + rng.Next((h - 2) / 2) * 2;
            if (wallY >= y + h - 1) wallY = y + h - 2;
            int holeX = x + rng.Next(w / 2) * 2;
            if (holeX >= x + w) holeX = x + w - 1;

            for (int xx = x; xx < x + w; xx++)
            {
                if (xx == holeX) continue;
                if (wallY > 0 && wallY < Rows && xx > 0 && xx < Cols)
                {
                    Cells[wallY - 1, xx].WallBottom = true;
                    Cells[wallY, xx].WallTop = true;
                }
            }

            Divide(x, y, w, wallY - y, rng);
            Divide(x, wallY + 1, w, y + h - wallY - 1, rng);
        }
        else
        {
            if (w < 3) return;
            int wallX = x + 2 + rng.Next((w - 2) / 2) * 2;
            if (wallX >= x + w - 1) wallX = x + w - 2;
            int holeY = y + rng.Next(h / 2) * 2;
            if (holeY >= y + h) holeY = y + h - 1;

            for (int yy = y; yy < y + h; yy++)
            {
                if (yy == holeY) continue;
                if (wallX > 0 && wallX < Cols && yy > 0 && yy < Rows)
                {
                    Cells[yy, wallX - 1].WallRight = true;
                    Cells[yy, wallX].WallLeft = true;
                }
            }

            Divide(x, y, wallX - x, h, rng);
            Divide(wallX + 1, y, x + w - wallX - 1, h, rng);
        }
    }

    // ── Dispatch to generator based on level ────────────────────────────────
    private void Generate(int seed)
    {
        int style = (seed / 137) % 4;
        switch (style)
        {
            case 0: GenerateRecursive(seed); break;
            case 1: GeneratePrim(seed); break;
            case 2: GenerateKruskal(seed); break;
            case 3: GenerateDivision(seed); break;
        }
    }

    // ── Factory: vary size and shape per level ──────────────────────────────
    public static MazePuzzle ForLevel(int level)
    {
        // Alternate between square and slightly rectangular mazes
        int size;
        if (level <= 3)      size = 5;
        else if (level <= 7) size = 7;
        else if (level <= 12) size = 9;
        else                 size = 11;

        // Some levels are rectangular for variety
        int rows = size, cols = size;
        int variant = level % 5;
        if (variant == 3 && size >= 7) cols = size + 2;  // wider
        if (variant == 4 && size >= 7) rows = size + 2;  // taller

        return new MazePuzzle(rows, cols, seed: level * 137 + 42);
    }
}
