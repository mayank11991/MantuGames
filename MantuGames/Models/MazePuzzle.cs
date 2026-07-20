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

    // ── Recursive-backtracking maze generator ───────────────────────────────
    private void Generate(int seed)
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

    // ── Factory: choose size based on level ─────────────────────────────────
    public static MazePuzzle ForLevel(int level)
    {
        int size = level <= 3 ? 5 : level <= 7 ? 7 : level <= 12 ? 9 : 11;
        return new MazePuzzle(size, size, seed: level * 137 + 42);
    }
}
