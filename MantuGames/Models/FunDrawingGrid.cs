namespace MantuGames.Models;

/// <summary>
/// A simple grid for the Fun Drawing game. Each cell stores its color index.
/// </summary>
public class FunDrawingGrid
{
    public int Rows { get; }
    public int Cols { get; }
    public int[,] CellColors { get; }

    private static readonly string[] Palette =
    {
        "#FF6B6B", "#FF8E53", "#FFC93C", "#6BCB77",
        "#4D96FF", "#9B59B6", "#FF69B4", "#00D2FF",
        "#FF4757", "#FFA502", "#2ED573", "#1E90FF",
    };

    public FunDrawingGrid(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        CellColors = new int[rows, cols];
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            CellColors[r, c] = -1; // -1 = uncolored
    }

    public void ColorCell(int row, int col, int colorIndex)
    {
        if (row >= 0 && row < Rows && col >= 0 && col < Cols)
            CellColors[row, col] = colorIndex;
    }

    public string GetColor(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Cols) return "#0F1420";
        int idx = CellColors[row, col];
        return idx < 0 ? "#0F1420" : Palette[idx % Palette.Length];
    }

    public string GetRandomColor(Random rng) => Palette[rng.Next(Palette.Length)];
    public int GetRandomColorIndex(Random rng) => rng.Next(Palette.Length);

    public static FunDrawingGrid ForLevel(int level)
    {
        int size = level <= 3 ? 8 : level <= 7 ? 10 : level <= 12 ? 12 : 14;
        return new FunDrawingGrid(size, size);
    }
}
