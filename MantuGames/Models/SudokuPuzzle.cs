using System;
using System.Collections.Generic;
using System.Linq;

namespace MantuGames.Models
{
    public class SudokuPuzzle
    {
        public int[,] Solution { get; private set; } = new int[5, 5];
        public int[,] Board { get; private set; } = new int[5, 5];
        public bool[,] IsFixed { get; private set; } = new bool[5, 5];

        private static readonly Random _rng = new Random();

        public static SudokuPuzzle Generate(int level)
        {
            var puzzle = new SudokuPuzzle();
            puzzle.GenerateSolution();
            puzzle.CreatePuzzle(level);
            return puzzle;
        }

        // ── SOLUTION GENERATOR ───────────────────────────────────────
        private void GenerateSolution()
        {
            bool generated = false;
            int attempts = 0;

            while (!generated && attempts < 1000)
            {
                attempts++;

                // Cyclic base row, then shuffle rows/cols/digits
                int[] baseRow = { 1, 2, 3, 4, 5 };
                Shuffle(baseRow);

                int[,] temp = new int[5, 5];
                for (int r = 0; r < 5; r++)
                for (int c = 0; c < 5; c++)
                    temp[r, c] = baseRow[(c + r * 2) % 5];

                int[] rowOrder = Enumerable.Range(0, 5).OrderBy(_ => _rng.Next()).ToArray();
                int[] colOrder = Enumerable.Range(0, 5).OrderBy(_ => _rng.Next()).ToArray();

                int[,] shuffled = new int[5, 5];
                for (int r = 0; r < 5; r++)
                for (int c = 0; c < 5; c++)
                    shuffled[r, c] = temp[rowOrder[r], colOrder[c]];

                if (IsValidSolution(shuffled))
                {
                    Solution = shuffled;
                    generated = true;
                }
            }

            if (!generated)
            {
                // Safe fallback
                Solution = new int[,]
                {
                    { 1, 2, 3, 4, 5 },
                    { 2, 3, 4, 5, 1 },
                    { 3, 4, 5, 1, 2 },
                    { 4, 5, 1, 2, 3 },
                    { 5, 1, 2, 3, 4 }
                };
            }
        }

        // ── PUZZLE CREATOR ───────────────────────────────────────────
        private void CreatePuzzle(int level)
        {
            // Copy solution → board, all fixed initially
            for (int r = 0; r < 5; r++)
            for (int c = 0; c < 5; c++)
            {
                Board[r, c] = Solution[r, c];
                IsFixed[r, c] = true;
            }

            // Blanks to remove per level (out of 25 cells):
            //   Level 1 → 12 blanks (13 given)  — warm-up
            //   Level 2 → 14 blanks (11 given)
            //   Level 3 → 16 blanks (9 given)
            //   Level 4 → 17 blanks (8 given)
            //   Level 5+ → 18 blanks (7 given)  — hardest
            //
            // 5×5 Latin square needs at minimum ~7 givens to stay uniquely solvable.
            int blanks = level switch
            {
                1 => 12,
                2 => 14,
                3 => 16,
                4 => 17,
                _ => 18
            };

            // Shuffle all 25 positions and remove `blanks` of them,
            // ensuring every ROW keeps at least 1 given and every
            // COLUMN keeps at least 1 given (so no row/col is fully blank).
            var positions = Enumerable.Range(0, 25)
                .OrderBy(_ => _rng.Next())
                .ToList();

            int removed = 0;
            foreach (int pos in positions)
            {
                if (removed >= blanks) break;

                int r = pos / 5;
                int c = pos % 5;

                // Count how many givens remain in this row and column
                int rowGivens = 0, colGivens = 0;
                for (int i = 0; i < 5; i++)
                {
                    if (IsFixed[r, i]) rowGivens++;
                    if (IsFixed[i, c]) colGivens++;
                }

                // Keep at least 1 given per row and per column
                if (rowGivens <= 1 || colGivens <= 1) continue;

                Board[r, c] = 0;
                IsFixed[r, c] = false;
                removed++;
            }
        }

        // ── VALIDATION ───────────────────────────────────────────────
        private bool IsValidSolution(int[,] grid)
        {
            for (int i = 0; i < 5; i++)
            {
                var row = new HashSet<int>();
                var col = new HashSet<int>();
                for (int j = 0; j < 5; j++)
                {
                    if (!row.Add(grid[i, j])) return false;
                    if (!col.Add(grid[j, i])) return false;
                }
            }

            return true;
        }

        public bool IsSolved()
        {
            for (int r = 0; r < 5; r++)
            for (int c = 0; c < 5; c++)
                if (Board[r, c] != Solution[r, c])
                    return false;
            return true;
        }

        public bool IsValidPlacement(int row, int col, int value)
        {
            for (int c = 0; c < 5; c++)
                if (c != col && Board[row, c] == value)
                    return false;

            for (int r = 0; r < 5; r++)
                if (r != row && Board[r, col] == value)
                    return false;

            return true;
        }

        private void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}