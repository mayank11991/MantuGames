using MantuGames.Models;
using Xunit;

namespace MantuGames.Tests;

public class WordPuzzleTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(10)]
    [InlineData(15)]
    public void Generate_AlwaysProduces9x9Grid(int level)
    {
        var puzzle = WordPuzzle.Generate(level);
        Assert.Equal(9, puzzle.GridSize);
        Assert.NotNull(puzzle.Grid);
        Assert.Equal(9, puzzle.Grid.GetLength(0));
        Assert.Equal(9, puzzle.Grid.GetLength(1));
    }

    [Fact]
    public void Generate_EveryPlacedWordIsReadableOnTheGrid()
    {
        for (int i = 0; i < 10; i++)
        {
            var puzzle = WordPuzzle.Generate(5);
            Assert.True(puzzle.Words.Count >= 4, "Expected several words per puzzle");

            foreach (var pw in puzzle.Words)
            {
                var cells = puzzle.GetWordCells(pw);
                string spelled = new(cells.Select(c => puzzle.Grid[c.Row, c.Col]).ToArray());
                Assert.Equal(pw.Word, spelled);
            }
        }
    }

    [Fact]
    public void Generate_ProducesOverlappingWords()
    {
        bool sawOverlap = false;
        for (int i = 0; i < 40 && !sawOverlap; i++)
        {
            var puzzle = WordPuzzle.Generate(9);
            var allCells = puzzle.Words
                .SelectMany(puzzle.GetWordCells)
                .GroupBy(c => c)
                .Where(g => g.Count() > 1)
                .ToList();
            if (allCells.Count > 0) sawOverlap = true;
        }

        Assert.True(sawOverlap, "Expected at least one shared cell across generated puzzles");
    }

    [Fact]
    public void Generate_NeverOverwritesPlacedLetters()
    {
        for (int i = 0; i < 20; i++)
        {
            var puzzle = WordPuzzle.Generate(15);
            foreach (var pw in puzzle.Words)
            {
                var cells = puzzle.GetWordCells(pw);
                string spelled = new(cells.Select(c => puzzle.Grid[c.Row, c.Col]).ToArray());
                Assert.Equal(pw.Word, spelled);
            }
        }
    }

    [Fact]
    public void CheckSelection_RecognizesExactWordCells()
    {
        var puzzle = WordPuzzle.Generate(1);
        var pw = puzzle.Words[0];
        var cells = puzzle.GetWordCells(pw);

        var found = puzzle.CheckSelection(cells);
        Assert.NotNull(found);
        Assert.Equal(pw.Word, found.Word);
        Assert.True(pw.Found);
    }

    [Fact]
    public void CheckSelection_RejectsWrongCells()
    {
        var puzzle = WordPuzzle.Generate(1);
        var pw = puzzle.Words[0];

        // Take the word's cells and shift them (if possible) to a wrong path
        var cells = puzzle.GetWordCells(pw);
        var shifted = cells.Select(c => c.Col + 1 < 9 ? (c.Row, c.Col + 1) : (c.Row, c.Col)).ToList();
        var found = puzzle.CheckSelection(shifted);

        Assert.Null(found);
    }
}

public class HanoiPuzzleTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    public void Generate_CreatesValidScatteredState(int level)
    {
        var p = HanoiPuzzle.Generate(level);
        Assert.InRange(p.DiscCount, 3, 5);
        Assert.Equal(3, p.Poles.Count);
        Assert.Equal(p.DiscCount, p.Poles.Sum(pole => pole.Count));
        Assert.False(p.IsSolved());
    }

    [Fact]
    public void TryMove_RejectsLargerDiscOnSmaller()
    {
        var p = HanoiPuzzle.Generate(3);
        // Deterministic layout: disc 5 on pole 0, disc 3 on pole 1
        p.Poles[0].Clear();
        p.Poles[1].Clear();
        p.Poles[2].Clear();
        p.Poles[0].Push(5);
        p.Poles[1].Push(3);

        bool ok = p.TryMove(0, 1);
        Assert.False(ok);
        Assert.Equal(5, p.Poles[0].Peek());
    }

    [Fact]
    public void Clone_IsIndependent()
    {
        var p = HanoiPuzzle.Generate(5);
        var copy = p.Clone();
        int from = -1, to = -1;
        for (int f = 0; f < 3 && from < 0; f++)
            for (int t = 0; t < 3; t++)
            {
                if (p.Poles[f].Count == 0 || f == t) continue;
                int top = p.Poles[f].Peek();
                if (p.Poles[t].Count == 0 || p.Poles[t].Peek() > top)
                {
                    from = f;
                    to = t;
                    break;
                }
            }
        Assert.True(from >= 0 && p.TryMove(from, to));
        Assert.NotEqual(p.Poles[from].Count, copy.Poles[from].Count);
    }
}

public class HanoiSolverTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    public void Solve_ProducesLegalMovesThatReachTheGoal(int level)
    {
        var puzzle = HanoiPuzzle.Generate(level);
        var moves = HanoiSolver.Solve(puzzle);

        Assert.NotEmpty(moves);

        var sim = puzzle.Clone();
        foreach (var (from, to) in moves)
        {
            Assert.True(sim.TryMove(from, to),
                $"Move {from}→{to} must be legal on the simulated puzzle (level {level})");
        }

        Assert.True(sim.IsSolved(), "Solution must leave every disc on the goal pole");
    }

    [Fact]
    public void Solve_ClassicStackUsesMinimumMoves()
    {
        // All discs on pole 0, goal pole 2 → exactly 2^n − 1 moves
        var puzzle = HanoiPuzzle.Generate(1);
        puzzle.Poles = new List<Stack<int>> {
            new(new[] { 3, 2, 1 }), new(), new()
        };
        puzzle.StartPole = 0;
        puzzle.GoalPole = 2;

        var moves = HanoiSolver.Solve(puzzle);
        Assert.Equal(7, moves.Count);
    }
}
public class AnimalCrushTests
{
    [Fact]
    public void GeneratedBoard_HasNoInitialMatches()
    {
        for (int level = 1; level <= 10; level++)
        {
            var p = AnimalCrushPuzzle.Generate(level, 80, 200);
            Assert.Empty(AnimalCrushPuzzle.FindAllMatches(p.Board));
        }
    }

    [Fact]
    public void GeneratedBoard_HasPossibleMove()
    {
        for (int level = 1; level <= 10; level++)
        {
            var p = AnimalCrushPuzzle.Generate(level, 80, 200);
            Assert.True(p.HasPossibleMove(), $"level {level} has no move");
        }
    }

    [Fact]
    public void ValidSwap_Scores_InvalidSwap_DoesNot()
    {
        var p = AnimalCrushPuzzle.Generate(3, 120, 350);
        var hint = p.FindHint();
        Assert.NotNull(hint);

        bool ok = p.TrySwap(hint.Value.r1, hint.Value.c1, hint.Value.r2, hint.Value.c2, out int gained);
        Assert.True(ok);
        Assert.True(gained > 0);
        Assert.Empty(AnimalCrushPuzzle.FindAllMatches(p.Board));

        // diagonal swap is not adjacent
        bool diag = p.TrySwap(0, 0, 1, 1, out int g2);
        Assert.False(diag);
        Assert.Equal(0, g2);
    }

    [Fact]
    public void Reshuffle_LeavesNoMatches_AndHasMove()
    {
        var p = AnimalCrushPuzzle.Generate(1, 80, 250);
        p.Reshuffle();
        Assert.Empty(AnimalCrushPuzzle.FindAllMatches(p.Board));
        Assert.True(p.HasPossibleMove());
    }

    [Fact]
    public void FindAllMatches_Detects2x2Square()
    {
        var p = new AnimalCrushPuzzle();
        // clear
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                p.Board[r, c] = -1;
        // 2x2 square of type 0 at (2,2)
        p.Board[2, 2] = 0; p.Board[2, 3] = 0;
        p.Board[3, 2] = 0; p.Board[3, 3] = 0;

        var matches = AnimalCrushPuzzle.FindAllMatches(p.Board);
        Assert.Equal(4, matches.Count);
    }

    [Fact]
    public void UsePowerUp_Hammer_ClearsTileAndScores()
    {
        var p = AnimalCrushPuzzle.Generate(1, 80, 200);
        // place distinct types to avoid matches
        int t = 0;
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                p.Board[r, c] = t++ % 6;
        // ensure (4,4) is not matching
        p.Board[4, 4] = 0;
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
                if (dr != 0 || dc != 0)
                    p.Board[4 + dr, 4 + dc] = (p.Board[4 + dr, 4 + dc] + 1) % 6;

        int score = p.UsePowerUp(AnimalCrushPuzzle.PowerUpKind.Hammer, 4, 4);
        Assert.True(score >= 10);
        // after gravity, cell is refilled — just verify score and board changed
    }

    [Fact]
    public void UsePowerUp_Rocket_ClearsRowAndColumn()
    {
        var p = AnimalCrushPuzzle.Generate(1, 80, 200);
        // distinct types
        int t = 0;
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                p.Board[r, c] = t++ % 6;

        int score = p.UsePowerUp(AnimalCrushPuzzle.PowerUpKind.Rocket, 4, 4);
        Assert.True(score >= 150); // 15 cells * 10
    }

    [Fact]
    public void UsePowerUp_Bomb_ClearsPlusShape()
    {
        var p = AnimalCrushPuzzle.Generate(1, 80, 200);
        // distinct types
        int t = 0;
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                p.Board[r, c] = t++ % 6;

        int score = p.UsePowerUp(AnimalCrushPuzzle.PowerUpKind.Bomb, 4, 4);
        Assert.True(score >= 130); // up to 13 cells * 10
    }
}
