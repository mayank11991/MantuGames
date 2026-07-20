namespace MantuGames.Models;

public class HanoiPuzzle
    {
        private static readonly Random _rng = new();

        public int DiscCount { get; private set; }
        public int StartPole { get; private set; }
        public int GoalPole  { get; private set; }
        // Stack top = top of visual tower = smallest disc currently on pole
        public List<Stack<int>> Poles { get; private set; }
        public int MinMoves  => (int)Math.Pow(2, DiscCount) - 1;
        public int MoveCount { get; private set; }
  
        public static HanoiPuzzle Generate(int level)
        {
            var p = new HanoiPuzzle();
            p.Build(level);
            return p;
        }
  
        private void Build(int level)
        {
            DiscCount = level <= 4 ? 3 : level <= 8 ? 4 : 5;
            MoveCount = 0;

            // Vary start and goal poles based on level
            int[] arrangements = {
                0, 2,  // level 1-3: pole 0 → pole 2
                1, 0,  // level 4-6: pole 1 → pole 0
                2, 1,  // level 7-9: pole 2 → pole 1
                0, 1,  // level 10+: pole 0 → pole 1
                1, 2,  // level 13-15: pole 1 → pole 2
                2, 0,  // level 16+: pole 2 → pole 0
            };
            int idx = Math.Clamp((level - 1) / 3 * 2, 0, arrangements.Length - 2);
            StartPole = arrangements[idx];
            GoalPole  = arrangements[idx + 1];

            // Randomly scatter disks across all 3 poles while maintaining validity.
            // Place disks largest → smallest: each subsequent disk is smaller than
            // everything already placed, so it can safely go on any pole.
            int maxAttempts = 20;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Poles = new List<Stack<int>> { new(), new(), new() };
                for (int d = DiscCount; d >= 1; d--)
                {
                    int pole = _rng.Next(3);
                    Poles[pole].Push(d);
                }
                // Ensure not already solved and at least 2 poles have disks
                if (!IsSolved() && Poles.Count(p => p.Count > 0) >= 2)
                    return;
            }
        }
  
        public bool TryMove(int from, int to)
        {
            if (from == to) return false;
            if (Poles[from].Count == 0) return false;
  
            int disc = Poles[from].Peek();
  
            if (Poles[to].Count > 0 && Poles[to].Peek() < disc)
                return false;
  
            Poles[from].Pop();
            Poles[to].Push(disc);
            MoveCount++;
            return true;
        }
  
        public bool IsSolved()
        {
            for (int p = 0; p < 3; p++)
                if (p == GoalPole && Poles[p].Count == DiscCount) return true;
            return false;
        }
  
        public int? TopDisc(int pole) =>
            Poles[pole].Count > 0 ? Poles[pole].Peek() : null;
 
        /// <summary>
        /// Returns discs bottom-to-top: index 0 = largest (bottom), last = smallest (top).
        /// Use this order for rendering — draw index 0 first (at bottom of pole visual).
        /// </summary>
        public List<int> GetPileBottomToTop(int pole)
        {
            // Stack.ToList() gives top-first (smallest first).
            // Reverse to get bottom-first (largest first).
            var list = Poles[pole].ToList();
            list.Reverse();
            return list; // index 0 = bottom disc (largest), last = top disc (smallest)
        }
    }