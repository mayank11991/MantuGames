using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using MantuGames.Helpers;

namespace MantuGames.Models;

public class ConnectTheDotsPuzzle
{
    public int GridSize { get; }
    public IReadOnlyList<DotPair> Pairs { get; }
    public IReadOnlySet<int> Obstacles { get; }
    public IReadOnlyDictionary<int, List<int>> SolutionPaths { get; }
    public int TimerSeconds { get; }

    public ConnectTheDotsPuzzle(int gridSize, List<DotPair> pairs, HashSet<int> obstacles, Dictionary<int, List<int>> solutionPaths, int timerSeconds)
    {
        GridSize = gridSize;
        Pairs = pairs;
        Obstacles = obstacles;
        SolutionPaths = solutionPaths;
        TimerSeconds = timerSeconds;
    }

    public static ConnectTheDotsPuzzle Generate(int level)
    {
        var config = GetLevelConfig(level);
        var rng = new Random(level * 12345 + 42);

        for (int attempt = 0; attempt < 100; attempt++)
        {
            var puzzle = TryGenerate(config, rng);
            if (puzzle != null)
                return puzzle;
        }

        return CreateFallbackPuzzle(config);
    }

    private static LevelConfig GetLevelConfig(int level)
    {
        return level switch
        {
            <= 3 => new LevelConfig { GridSize = 5, PairCount = 3, TimerSeconds = 150, MaxObstacles = 0 },
            <= 6 => new LevelConfig { GridSize = 6, PairCount = 4, TimerSeconds = 120, MaxObstacles = 0 },
            <= 10 => new LevelConfig { GridSize = 6, PairCount = 6, TimerSeconds = 100, MaxObstacles = 0 },
            <= 15 => new LevelConfig { GridSize = 7, PairCount = 7, TimerSeconds = 90, MaxObstacles = 2 },
            <= 20 => new LevelConfig { GridSize = 7, PairCount = 8, TimerSeconds = 80, MaxObstacles = 3 },
            <= 25 => new LevelConfig { GridSize = 8, PairCount = 9, TimerSeconds = 70, MaxObstacles = 4 },
            _ => new LevelConfig { GridSize = 8, PairCount = 10, TimerSeconds = 60, MaxObstacles = 5 }
        };
    }

    private static ConnectTheDotsPuzzle TryGenerate(LevelConfig config, Random rng)
    {
        int size = config.GridSize;
        int totalCells = size * size;

        var pairs = new List<DotPair>();
        var occupied = new bool[totalCells];
        var gridPairs = new int[totalCells];
        for (int i = 0; i < totalCells; i++) gridPairs[i] = -1;

        var colors = GetPairColors(config.PairCount);

        for (int pairId = 0; pairId < config.PairCount; pairId++)
        {
            var validPositions = new List<int>();
            for (int i = 0; i < totalCells; i++)
            {
                if (!occupied[i])
                    validPositions.Add(i);
            }

            if (validPositions.Count < 2)
                return null;

            int startIdx = validPositions[rng.Next(validPositions.Count)];
            occupied[startIdx] = true;
            gridPairs[startIdx] = pairId;

            var endCandidates = new List<int>();
            foreach (int idx in validPositions)
            {
                if (idx == startIdx) continue;
                int sr = startIdx / size, sc = startIdx % size;
                int er = idx / size, ec = idx % size;
                int dist = Math.Abs(sr - er) + Math.Abs(sc - ec);
                if (dist >= Math.Max(3, size / 2))
                    endCandidates.Add(idx);
            }

            if (endCandidates.Count == 0)
                return null;

            int endIdx = endCandidates[rng.Next(endCandidates.Count)];
            occupied[endIdx] = true;
            gridPairs[endIdx] = pairId;

            pairs.Add(new DotPair
            {
                Id = pairId,
                Color = colors[pairId],
                StartRow = startIdx / size,
                StartCol = startIdx % size,
                EndRow = endIdx / size,
                EndCol = endIdx % size
            });
        }

        var solutionPaths = new Dictionary<int, List<int>>();
        var pathCells = new HashSet<int>();

        foreach (var pair in pairs)
        {
            var path = FindPath(gridPairs, size, pair.StartRow, pair.StartCol, pair.EndRow, pair.EndCol, pair.Id, pathCells, rng);
            if (path == null || path.Count < 2)
                return null;

            solutionPaths[pair.Id] = path;
            foreach (int cell in path)
                pathCells.Add(cell);
        }

        var emptyCells = new List<int>();
        for (int i = 0; i < totalCells; i++)
        {
            if (!pathCells.Contains(i))
                emptyCells.Add(i);
        }

        var obstacles = new HashSet<int>();
        int obstacleCount = Math.Min(config.MaxObstacles, emptyCells.Count / 3);
        for (int i = 0; i < obstacleCount && emptyCells.Count > 0; i++)
        {
            int idx = rng.Next(emptyCells.Count);
            obstacles.Add(emptyCells[idx]);
            emptyCells.RemoveAt(idx);
        }

        return new ConnectTheDotsPuzzle(config.GridSize, pairs, obstacles, solutionPaths, config.TimerSeconds);
    }

    private static List<int> FindPath(int[] gridPairs, int size, int sr, int sc, int er, int ec, int pairId, HashSet<int> usedCells, Random rng)
    {
        var visited = new bool[size * size];
        var parent = new int[size * size];
        for (int i = 0; i < parent.Length; i++) parent[i] = -1;

        var queue = new Queue<int>();
        int start = sr * size + sc;
        queue.Enqueue(start);
        visited[start] = true;

        int[] dr = { -1, 1, 0, 0 };
        int[] dc = { 0, 0, -1, 1 };
        var directions = new List<int> { 0, 1, 2, 3 };

        while (queue.Count > 0)
        {
            int curr = queue.Dequeue();
            int r = curr / size, c = curr % size;

            if (r == er && c == ec)
            {
                var path = new List<int>();
                int p = curr;
                while (p != -1)
                {
                    path.Add(p);
                    p = parent[p];
                }
                path.Reverse();
                return path;
            }

            directions.Shuffle(rng);

            foreach (int dir in directions)
            {
                int nr = r + dr[dir];
                int nc = c + dc[dir];
                if (nr < 0 || nr >= size || nc < 0 || nc >= size) continue;

                int nidx = nr * size + nc;
                if (visited[nidx]) continue;
                if (usedCells.Contains(nidx)) continue;
                if (gridPairs[nidx] != -1 && gridPairs[nidx] != pairId) continue;

                visited[nidx] = true;
                parent[nidx] = curr;
                queue.Enqueue(nidx);
            }
        }

        return null;
    }

    private static ConnectTheDotsPuzzle CreateFallbackPuzzle(LevelConfig config)
    {
        int size = config.GridSize;
        var pairs = new List<DotPair>();
        var colors = GetPairColors(config.PairCount);

        for (int i = 0; i < config.PairCount; i++)
        {
            pairs.Add(new DotPair
            {
                Id = i,
                Color = colors[i],
                StartRow = i,
                StartCol = 0,
                EndRow = i,
                EndCol = size - 1
            });
        }

        var solutionPaths = new Dictionary<int, List<int>>();
        for (int i = 0; i < config.PairCount; i++)
        {
            var path = new List<int>();
            for (int c = 0; c < size; c++)
                path.Add(i * size + c);
            solutionPaths[i] = path;
        }

        return new ConnectTheDotsPuzzle(size, pairs, new HashSet<int>(), solutionPaths, config.TimerSeconds);
    }

    private static Color[] GetPairColors(int count)
    {
        var baseColors = new[]
        {
            Color.FromArgb("#FF9C27B0"), // Purple/Magenta
            Color.FromArgb("#FF4CAF50"), // Green
            Color.FromArgb("#FF00BCD4"), // Cyan
            Color.FromArgb("#FFFF9800"), // Orange
            Color.FromArgb("#FFE91E63"), // Pink
            Color.FromArgb("#FF2196F3"), // Blue
            Color.FromArgb("#FFFFEB3B"), // Yellow
            Color.FromArgb("#FFF44336"), // Red
            Color.FromArgb("#FF9E9E9E"), // Gray
            Color.FromArgb("#FF795548"), // Brown
        };

        var result = new Color[count];
        for (int i = 0; i < count; i++)
            result[i] = baseColors[i % baseColors.Length];
        return result;
    }

    private class LevelConfig
    {
        public int GridSize { get; set; }
        public int PairCount { get; set; }
        public int TimerSeconds { get; set; }
        public int MaxObstacles { get; set; }
    }
}

public class DotPair
{
    public int Id { get; set; }
    public Color Color { get; set; }
    public int StartRow { get; set; }
    public int StartCol { get; set; }
    public int EndRow { get; set; }
    public int EndCol { get; set; }

    public int StartIndex(int gridSize) => StartRow * gridSize + StartCol;
    public int EndIndex(int gridSize) => EndRow * gridSize + EndCol;
}

internal static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list, Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}