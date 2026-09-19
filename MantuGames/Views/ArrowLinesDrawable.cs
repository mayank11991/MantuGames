using MantuGames.Models;
using Microsoft.Maui.Graphics;

namespace MantuGames.Views;

public class ArrowLinesDrawable : IDrawable
{
    public List<ArrowCell> Arrows { get; set; } = new();
    public ArrowCell HighlightedArrow { get; set; }
    public ArrowCell SlideArrow { get; set; }
    public float SlideProgress { get; set; } = 0;
    public int Rows { get; set; } = 9;
    public int Cols { get; set; } = 9;
    public float CellSize { get; set; } = 40;
    public float Padding { get; set; } = 8;
    public float WallThickness { get; set; } = 6f;
    public float PathThickness { get; set; } = 8f;

    private readonly Dictionary<int, Color> _arrowColors = new()
    {
        { 0, Colors.Cyan },       // Up
        { 1, Colors.Orange },     // Down
        { 2, Color.FromArgb("#A855F7") }, // Left
        { 3, Color.FromArgb("#34D399") }, // Right
    };

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float startX = (dirtyRect.Width - Cols * CellSize) / 2;
        float startY = (dirtyRect.Height - Rows * CellSize) / 2;

        canvas.SaveState();
        canvas.Translate(startX, startY);

        DrawGridBackground(canvas);
        DrawWalls(canvas);
        DrawArrows(canvas);
        DrawSlidingArrow(canvas);

        canvas.RestoreState();
    }

    private void DrawGridBackground(ICanvas canvas)
    {
        canvas.FillColor = Color.FromArgb("#0A0D14");
        canvas.FillRoundedRectangle(0, 0, Cols * CellSize, Rows * CellSize, 12);
    }

    private void DrawWalls(ICanvas canvas)
    {
        canvas.StrokeColor = Color.FromArgb("#1E293B");
        canvas.StrokeSize = WallThickness;
        canvas.StrokeLineCap = LineCap.Round;

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                float x = c * CellSize;
                float y = r * CellSize;

                // Draw right wall (connects to next cell)
                if (c < Cols - 1)
                {
                    canvas.DrawLine(
                        x + CellSize, y + CellSize * 0.3f,
                        x + CellSize, y + CellSize * 0.7f);
                }

                // Draw bottom wall (connects to next row)
                if (r < Rows - 1)
                {
                    canvas.DrawLine(
                        x + CellSize * 0.3f, y + CellSize,
                        x + CellSize * 0.7f, y + CellSize);
                }
            }
        }

        // Outer border
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = WallThickness + 2;
        canvas.DrawRoundedRectangle(
            -WallThickness / 2, -WallThickness / 2,
            Cols * CellSize + WallThickness, Rows * CellSize + WallThickness,
            14);
    }

    private void DrawSlidingArrow(ICanvas canvas)
    {
        if (SlideArrow == null) return;

        var color = _arrowColors.ContainsKey((int)SlideArrow.Direction)
            ? _arrowColors[(int)SlideArrow.Direction]
            : Colors.Cyan;

        float cx = SlideArrow.Col * CellSize + CellSize / 2;
        float cy = SlideArrow.Row * CellSize + CellSize / 2;
        float size = CellSize * 0.25f;

        // Calculate slide offset
        float slideDist = CellSize * 2.5f * SlideProgress;
        float sx = cx + SlideArrow.Direction switch
        {
            ArrowDirection.Left => -slideDist,
            ArrowDirection.Right => slideDist,
            _ => 0
        };
        float sy = cy + SlideArrow.Direction switch
            {
            ArrowDirection.Up => -slideDist,
            ArrowDirection.Down => slideDist,
            _ => 0
        };

        // Fade out
        float alpha = 1f - SlideProgress;

        // Glow
        for (int i = 2; i >= 1; i--)
        {
            canvas.FillColor = color.WithAlpha(0.1f * i * alpha);
            canvas.FillCircle(sx, sy, size + i * 5);
        }

        // Circle
        canvas.FillColor = Color.FromArgb("#0F172A").WithAlpha(alpha);
        canvas.FillCircle(sx, sy, size + 2);
        canvas.StrokeColor = color.WithAlpha(alpha);
        canvas.StrokeSize = 2;
        canvas.DrawCircle(sx, sy, size + 2);

        // Arrow text
        canvas.FontColor = color.WithAlpha(alpha);
        canvas.FontSize = CellSize * 0.45f;
        canvas.DrawString(SlideArrow.Symbol.ToString(), sx - CellSize * 0.2f, sy - CellSize * 0.25f, CellSize * 0.4f, CellSize * 0.5f, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawArrows(ICanvas canvas)
    {
        foreach (var arrow in Arrows)
        {
            if (arrow.IsCleared) continue;

            float cx = arrow.Col * CellSize + CellSize / 2;
            float cy = arrow.Row * CellSize + CellSize / 2;
            float size = CellSize * 0.25f;

            var color = _arrowColors.ContainsKey((int)arrow.Direction)
                ? _arrowColors[(int)arrow.Direction]
                : Colors.White;

            bool isHighlighted = arrow == HighlightedArrow;

            // Outer glow
            if (isHighlighted)
            {
                for (int i = 3; i >= 1; i--)
                {
                    canvas.FillColor = color.WithAlpha(0.12f * i);
                    canvas.FillCircle(cx, cy, size + i * 4);
                }
            }

            // Cell background circle
            canvas.FillColor = Color.FromArgb("#0F172A");
            canvas.FillCircle(cx, cy, size + 2);

            // Border circle
            canvas.StrokeColor = isHighlighted ? color : color.WithAlpha(0.6f);
            canvas.StrokeSize = 2;
            canvas.DrawCircle(cx, cy, size + 2);

            // Arrow character
            canvas.FontColor = color;
            canvas.FontSize = CellSize * 0.45f;
            canvas.DrawString(arrow.Symbol.ToString(), cx - CellSize * 0.2f, cy - CellSize * 0.25f, CellSize * 0.4f, CellSize * 0.5f, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
