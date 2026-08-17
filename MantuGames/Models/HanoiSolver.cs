namespace MantuGames.Models;

/// <summary>
/// Computes the exact move sequence that solves a Tower of Hanoi puzzle
/// from its current (possibly scattered) configuration onto the goal pole.
/// Pure logic — no UI, no state mutation of the caller's puzzle.
/// </summary>
public static class HanoiSolver
{
    /// <summary>Returns the ordered list of (from, to) moves that solves the puzzle.</summary>
    public static List<(int From, int To)> Solve(HanoiPuzzle puzzle)
    {
        var sim = puzzle.Clone();
        var moves = new List<(int, int)>();
        Move(sim, sim.DiscCount, sim.GoalPole, moves);
        return moves;
    }

    private static void Move(HanoiPuzzle sim, int n, int target, List<(int, int)> moves)
    {
        if (n == 0) return;

        int current = FindPole(sim, n);
        if (current == target)
        {
            // Disc already in place — settle the smaller discs on top of it.
            Move(sim, n - 1, target, moves);
            return;
        }

        int spare = 3 - current - target;

        // Move every smaller disc off this disc, onto the spare pole.
        Move(sim, n - 1, spare, moves);

        // Disc n is now free on top — move it to its target.
        if (sim.TryMove(current, target))
            moves.Add((current, target));

        // Finally stack the smaller discs on top of it.
        Move(sim, n - 1, target, moves);
    }

    private static int FindPole(HanoiPuzzle puzzle, int disc)
    {
        for (int p = 0; p < 3; p++)
            if (puzzle.Poles[p].Contains(disc))
                return p;
        return 0;
    }
}