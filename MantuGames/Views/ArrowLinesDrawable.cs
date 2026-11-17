using MantuGames.Services;
using MantuGames.Models;

namespace MantuGames.Views;

/// <summary>
/// Draws the Arrow Lines board: a dotted grid, every line as a thick rounded stroke
/// with an arrowhead at its head, and the line currently moving (blue when escaping,
/// red when it bumps) at a fractional offset along its own path.
/// </summary>
public class ArrowLinesDrawable : IDrawable
{
    private static readonly Color Ink = Color.FromArgb("#1F2A44");
    private static Color Move => ThemeColors.Get(0, "#2F8CFF");
    private static readonly Color Bump = Color.FromArgb("#FF5C7A");
    private static readonly Color Dot = Color.FromArgb("#D6E4F0");
    private static readonly Color Hint = Color.FromArgb("#3DC86A");

    public ArrowLinesPuzzle? Puzzle { get; set; }

    /// <summary>Line being animated (drawn separately from the static ones).</summary>
    public ArrowLine? Moving { get; set; }
    /// <summary>How many cells the moving line has advanced along its path.</summary>
    public float Offset { get; set; }
    public bool MovingIsBump { get; set; }
    /// <summary>Line to pulse green (Show Solution hint).</summary>
    public ArrowLine? Hinted { get; set; }

    public float CellSize { get; private set; }
    public PointF Origin { get; private set; }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var p = Puzzle;
        if (p == null) return;

        float pad = 14;
        CellSize = Math.Min((rect.Width - pad * 2) / p.Cols, (rect.Height - pad * 2) / p.Rows);
        float bw = CellSize * p.Cols, bh = CellSize * p.Rows;
        Origin = new PointF(rect.X + (rect.Width - bw) / 2, rect.Y + (rect.Height - bh) / 2);

        // Board card with a soft lip
        canvas.FillColor = Color.FromArgb("#332A6FB8");
        canvas.FillRoundedRectangle(Origin.X - 10, Origin.Y - 4, bw + 20, bh + 20, 22);
        canvas.FillColor = Colors.White;
        canvas.FillRoundedRectangle(Origin.X - 10, Origin.Y - 10, bw + 20, bh + 20, 22);

        canvas.FillColor = Dot;
        for (int r = 0; r < p.Rows; r++)
            for (int c = 0; c < p.Cols; c++)
                canvas.FillCircle(Center(r, c), Math.Max(1.6f, CellSize * 0.05f));

        float sw = Math.Max(4f, CellSize * 0.17f);
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        foreach (var line in p.Lines)
        {
            if (line == Moving || line.IsCleared) continue;
            var pts = line.Cells.Select(c => Center(c.Row, c.Col)).ToList();
            DrawLine(canvas, pts, line == Hinted ? Hint : Ink, sw);
        }

        if (Moving != null)
            DrawLine(canvas, MovingPoints(Moving, Offset), MovingIsBump ? Bump : Move, sw);
    }

    public PointF Center(int r, int c) =>
        new(Origin.X + (c + 0.5f) * CellSize, Origin.Y + (r + 0.5f) * CellSize);

    /// <summary>The line's own path continued straight past its head far enough to leave the screen.</summary>
    private List<PointF> ExtendedPath(ArrowLine line)
    {
        var pts = line.Cells.Select(c => Center(c.Row, c.Col)).ToList();
        var (dr, dc) = ArrowLine.Step(line.Direction);
        var (hr, hc) = line.Head;
        int extra = Math.Max(Puzzle!.Rows, Puzzle.Cols) + line.Cells.Count + 2;
        for (int i = 1; i <= extra; i++) pts.Add(Center(hr + dr * i, hc + dc * i));
        return pts;
    }

    private List<PointF> MovingPoints(ArrowLine line, float offset)
    {
        var path = ExtendedPath(line);
        int len = line.Cells.Count;
        float start = Math.Clamp(offset, 0, path.Count - len);
        float end = start + len - 1;
        var result = new List<PointF> { Sample(path, start) };
        for (int i = (int)Math.Floor(start) + 1; i < end; i++) result.Add(path[i]);
        result.Add(Sample(path, end));
        return result;
    }

    private static PointF Sample(List<PointF> path, float s)
    {
        int i = Math.Clamp((int)Math.Floor(s), 0, path.Count - 1);
        int j = Math.Min(i + 1, path.Count - 1);
        float f = s - i;
        return new PointF(path[i].X + (path[j].X - path[i].X) * f, path[i].Y + (path[j].Y - path[i].Y) * f);
    }

    private void DrawLine(ICanvas canvas, List<PointF> pts, Color color, float sw)
    {
        if (pts.Count < 2) return;
        canvas.StrokeColor = color;
        canvas.StrokeSize = sw;

        var path = new PathF();
        path.MoveTo(pts[0]);
        for (int i = 1; i < pts.Count; i++) path.LineTo(pts[i]);
        canvas.DrawPath(path);

        // Arrowhead: a filled triangle at the head, aligned with the last segment.
        var tip = pts[^1];
        var prev = pts[^2];
        float dx = tip.X - prev.X, dy = tip.Y - prev.Y;
        float l = MathF.Sqrt(dx * dx + dy * dy);
        if (l < 0.01f) return;
        dx /= l; dy /= l;
        float hl = CellSize * 0.32f, hw = CellSize * 0.23f;
        var apex = new PointF(tip.X + dx * hl * 0.9f, tip.Y + dy * hl * 0.9f);
        var b1 = new PointF(tip.X - dx * hl * 0.15f - dy * hw, tip.Y - dy * hl * 0.15f + dx * hw);
        var b2 = new PointF(tip.X - dx * hl * 0.15f + dy * hw, tip.Y - dy * hl * 0.15f - dx * hw);
        var head = new PathF();
        head.MoveTo(apex); head.LineTo(b1); head.LineTo(b2); head.Close();
        canvas.FillColor = color;
        canvas.FillPath(head);
        canvas.StrokeSize = sw * 0.5f;
        canvas.DrawPath(head);
    }
}
