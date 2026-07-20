namespace MantuGames.Models;

public class PuzzlePetsPuzzle
{
    public int Rows { get; }
    public int Cols { get; }
    public int TotalPieces => Rows * Cols;
    public string AnimalEmoji { get; }
    public string AnimalName { get; }
    public int Moves { get; private set; }

    // pieceId → which grid position it belongs in (0..TotalPieces-1)
    public int[] PiecePositions { get; private set; }

    // grid position → pieceId placed there (-1 if empty)
    public int[] GridState { get; private set; }

    // pieces still in the tray
    public List<int> TrayPieces { get; private set; }

    public bool IsSolved
    {
        get
        {
            for (int i = 0; i < TotalPieces; i++)
                if (GridState[i] != i) return false;
            return true;
        }
    }

    private static readonly (string emoji, string name)[] Animals =
    {
        ("🐱", "Cat"), ("🐶", "Dog"), ("🐰", "Rabbit"),
        ("🦊", "Fox"), ("🐼", "Panda"), ("🐸", "Frog"),
        ("🦁", "Lion"), ("🐯", "Tiger"), ("🐵", "Monkey"),
        ("🐨", "Koala"), ("🐻", "Bear"), ("🦄", "Unicorn"),
    };

    public PuzzlePetsPuzzle(int level)
    {
        (Rows, Cols) = level switch
        {
            1 => (2, 2),
            2 => (2, 3),
            3 => (3, 3),
            4 => (3, 4),
            _ => (4, 4)
        };

        int animalIdx = (level - 1) % Animals.Length;
        AnimalEmoji = Animals[animalIdx].emoji;
        AnimalName = Animals[animalIdx].name;

        int n = TotalPieces;
        // Each piece i belongs at position i when solved
        PiecePositions = Enumerable.Range(0, n).ToArray();

        // Shuffle which piece id goes where for the initial tray state
        var rng = new Random();
        var shuffled = Enumerable.Range(0, n).OrderBy(_ => rng.Next()).ToArray();
        // Ensure not already solved
        if (shuffled.Select((v, i) => v == i).All(x => x))
        {
            (shuffled[0], shuffled[1]) = (shuffled[1], shuffled[0]);
        }

        GridState = Enumerable.Repeat(-1, n).ToArray();
        TrayPieces = new List<int>(shuffled);
        Moves = 0;
    }

    public bool TryPlace(int pieceId, int gridPos)
    {
        if (gridPos < 0 || gridPos >= TotalPieces) return false;
        if (GridState[gridPos] != -1) return false;
        if (PiecePositions[pieceId] != gridPos) return false;

        GridState[gridPos] = pieceId;
        TrayPieces.Remove(pieceId);
        Moves++;
        return true;
    }

    public int? GetPieceAt(int gridPos)
    {
        if (gridPos < 0 || gridPos >= TotalPieces) return null;
        int id = GridState[gridPos];
        return id >= 0 ? id : null;
    }
}
