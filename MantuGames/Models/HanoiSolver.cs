namespace MantuGames.Models;

/// <summary>
/// Computes the exact move sequence that solves a Tower of Hanoi puzzle
/// from its current (possibly scattered) configuration onto the goal pole.
/// Pure logic — no UI, no state mutation of the caller's puzzle.
/// Supports 3, 4, or 5 poles.
/// </summary>
public static class HanoiSolver
{
    /// <summary>Returns the ordered list of (from, to) moves that solves the puzzle.</summary>
    public static List<(int From, int To)> Solve(HanoiPuzzle puzzle)
    {
        var sim = puzzle.Clone();
        var moves = new List<(int, int)>();
        int poleCount = sim.Poles.Count;
        Move(sim, sim.DiscCount, sim.GoalPole, moves, poleCount);
        return moves;
    }

    private static void Move(HanoiPuzzle sim, int n, int target, List<(int, int)> moves, int poleCount)
    {
        if (n == 0) return;

        int current = FindPole(sim, n);
        if (current == target)
        {
            // Disc already in place — settle the smaller discs on top of it.
            Move(sim, n - 1, target, moves, poleCount);
            return;
        }

        // Find a spare pole that is neither current nor target
        int spare = FindSparePole(sim, current, target, poleCount);

        // Move every smaller disc off this disc, onto the spare pole.
        Move(sim, n - 1, spare, moves, poleCount);

        // Disc n is now free on top — move it to its target.
        if (sim.TryMove(current, target))
            moves.Add((current, target));

        // Finally stack the smaller discs on top of it.
        Move(sim, n - 1, target, moves, poleCount);
    }

    private static int FindSparePole(HanoiPuzzle sim, int current, int target, int poleCount)
    {
        for (int p = 0; p < poleCount; p++)
        {
            if (p != current && p != target)
                return p;
        }
        return 0; // fallback
    }

    private static int FindPole(HanoiPuzzle sim, int disc)
    {
        for (int p = 0; p < sim.Poles.Count; p++)
            if (sim.Poles[p].Contains(disc))
                return p;
        return 0;
    }
}