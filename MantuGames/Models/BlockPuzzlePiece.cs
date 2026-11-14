namespace MantuGames.Models;

public class BlockPiece
{
    public int[,]  Shape { get; init; }
    public Color   PieceColor  { get; init; }
    public string  Name  { get; init; }

    // Rotate shape 90 degrees clockwise
    public BlockPiece RotateClockwise()
    {
        int rows = Shape.GetLength(0);
        int cols = Shape.GetLength(1);
        var rotated = new int[cols, rows];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                rotated[c, rows - 1 - r] = Shape[r, c];

        return new BlockPiece { Shape = rotated, PieceColor = PieceColor, Name = Name };
    }

    // All standard Tetromino definitions
    public static readonly List<BlockPiece> All = new()
    {
        // I — cyan
        new BlockPiece
        {
            Name = "I",
            PieceColor = Color.FromArgb("#7FD8FF"),
            Shape = new int[,]
            {
                { 1, 1, 1, 1 }
            }
        },
        // O — yellow
        new BlockPiece
        {
            Name = "O",
            PieceColor = Color.FromArgb("#FFD166"),
            Shape = new int[,]
            {
                { 1, 1 },
                { 1, 1 }
            }
        },
        // T — purple
        new BlockPiece
        {
            Name = "T",
            PieceColor = Color.FromArgb("#B39DFF"),
            Shape = new int[,]
            {
                { 0, 1, 0 },
                { 1, 1, 1 }
            }
        },
        // S — green
        new BlockPiece
        {
            Name = "S",
            PieceColor = Color.FromArgb("#7BDCB5"),
            Shape = new int[,]
            {
                { 0, 1, 1 },
                { 1, 1, 0 }
            }
        },
        // Z — red
        new BlockPiece
        {
            Name = "Z",
            PieceColor = Color.FromArgb("#FF8FA3"),
            Shape = new int[,]
            {
                { 1, 1, 0 },
                { 0, 1, 1 }
            }
        },
        // L — orange
        new BlockPiece
        {
            Name = "L",
            PieceColor = Color.FromArgb("#FFB36B"),
            Shape = new int[,]
            {
                { 1, 0 },
                { 1, 0 },
                { 1, 1 }
            }
        },
        // J — blue
        new BlockPiece
        {
            Name = "J",
            PieceColor = Color.FromArgb("#6D9BFF"),
            Shape = new int[,]
            {
                { 0, 1 },
                { 0, 1 },
                { 1, 1 }
            }
        },
    };
}
