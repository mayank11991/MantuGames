using MantuGames.ViewModels;
using Microsoft.Maui.Graphics;

namespace MantuGames.Views;

public class CtdDrawable : IDrawable
{
    private readonly CtdViewModel _vm;
    private float _cellSize;
    private float _offsetX;
    private float _offsetY;

    public CtdDrawable(CtdViewModel vm) => _vm = vm;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_vm.Pairs.Count == 0 || dirtyRect.Width <= 0 || dirtyRect.Height <= 0) return;

        int rows = _vm.Rows, cols = _vm.Cols;
        float padding = 12f;
        float availW = dirtyRect.Width - padding * 2;
        float availH = dirtyRect.Height - padding * 2;
        _cellSize = Math.Min(availW / cols, availH / rows);
        if (_cellSize <= 0) return;

        _offsetX = (dirtyRect.Width - _cellSize * cols) / 2;
        _offsetY = (dirtyRect.Height - _cellSize * rows) / 2;

        DrawGrid(rows, cols, canvas);
        DrawCells(canvas, rows, cols);
        DrawPaths(canvas, rows, cols);
        DrawDots(canvas, rows, cols);
    }

    private void DrawGrid(int rows, int cols, ICanvas canvas)
    {
        // Night board background
        canvas.FillColor = Color.FromArgb("#EAF2FB");
        canvas.FillRectangle(_offsetX, _offsetY, _cellSize * cols, _cellSize * rows);

        // Subtle grid lines
        canvas.StrokeColor = Color.FromArgb("#FFFFFF");
        canvas.StrokeSize = 1f;

        for (int r = 0; r <= rows; r++)
            canvas.DrawLine(_offsetX, _offsetY + r * _cellSize, _offsetX + cols * _cellSize, _offsetY + r * _cellSize);
        for (int c = 0; c <= cols; c++)
            canvas.DrawLine(_offsetX + c * _cellSize, _offsetY, _offsetX + c * _cellSize, _offsetY + rows * _cellSize);
    }

    private void DrawCells(ICanvas canvas, int rows, int cols)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = _offsetX + c * _cellSize;
                float y = _offsetY + r * _cellSize;
                canvas.FillColor = Color.FromArgb("#8ED3FF");
                canvas.FillRectangle(x + 1, y + 1, _cellSize - 2, _cellSize - 2);
            }
        }
    }

    private void DrawPaths(ICanvas canvas, int rows, int cols)
    {
        float lineThickness = _cellSize * 0.24f;

        foreach (var kvp in _vm.CompletedPaths)
        {
            if (kvp.Value.Count < 1) continue;
            DrawPathLine(canvas, kvp.Value, _vm.GetPairColor(kvp.Key), lineThickness);
        }

        if (_vm.ActivePair.HasValue && _vm.CurrentPath.Count >= 1)
        {
            DrawPathLine(canvas, _vm.CurrentPath, _vm.GetPairColor(_vm.ActivePair.Value), lineThickness);
        }
    }

    private void DrawPathLine(ICanvas canvas, List<(int r, int c)> path, Color color, float thickness)
    {
        canvas.StrokeColor = color;
        canvas.StrokeSize = thickness;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        if (path.Count == 1)
        {
            float cx = _offsetX + path[0].c * _cellSize + _cellSize / 2;
            float cy = _offsetY + path[0].r * _cellSize + _cellSize / 2;
            canvas.FillColor = color;
            canvas.FillCircle(cx, cy, thickness / 2);
            return;
        }

        // Draw solid line segments
        for (int i = 0; i < path.Count - 1; i++)
        {
            float x1 = _offsetX + path[i].c * _cellSize + _cellSize / 2;
            float y1 = _offsetY + path[i].r * _cellSize + _cellSize / 2;
            float x2 = _offsetX + path[i + 1].c * _cellSize + _cellSize / 2;
            float y2 = _offsetY + path[i + 1].r * _cellSize + _cellSize / 2;
            canvas.DrawLine(x1, y1, x2, y2);
        }

        // Solid circles at start and end
        canvas.FillColor = color;
        canvas.FillCircle(
            _offsetX + path[0].c * _cellSize + _cellSize / 2,
            _offsetY + path[0].r * _cellSize + _cellSize / 2, thickness / 2);
        canvas.FillCircle(
            _offsetX + path[^1].c * _cellSize + _cellSize / 2,
            _offsetY + path[^1].r * _cellSize + _cellSize / 2, thickness / 2);
    }

    private void DrawDots(ICanvas canvas, int rows, int cols)
    {
        float radius = Math.Max(4, _cellSize * 0.33f);
        foreach (var pair in _vm.Pairs)
        {
            DrawDot(canvas, pair.R1, pair.C1, pair.Color, radius);
            DrawDot(canvas, pair.R2, pair.C2, pair.Color, radius);
        }
    }

    private void DrawDot(ICanvas canvas, int r, int c, Color color, float radius)
    {
        float cx = _offsetX + c * _cellSize + _cellSize / 2;
        float cy = _offsetY + r * _cellSize + _cellSize / 2;

        // Glossy candy marble: soft drop shadow, darker lip, white ring, bright face with a
        // radial glow, and a white shine in the top-left (same look as the CTA buttons).
        float r0 = radius * 0.92f;
        var dark = color.WithLuminosity(color.GetLuminosity() * 0.7f);
        var light = color.WithLuminosity(Math.Min(0.92f, color.GetLuminosity() * 1.3f));

        canvas.FillColor = Color.FromArgb("#2A1F2A44");
        canvas.FillEllipse(cx - r0 * 0.9f, cy + r0 * 0.7f, r0 * 1.8f, r0 * 0.5f);

        canvas.FillColor = dark;
        canvas.FillCircle(cx, cy + r0 * 0.12f, r0);
        canvas.FillColor = Colors.White;
        canvas.FillCircle(cx, cy, r0);

        float face = r0 * 0.84f;
        // Fake radial glow with stacked circles (a gradient paint leaks into later fills on Android).
        canvas.FillColor = color;
        canvas.FillCircle(cx, cy, face);
        canvas.FillColor = light.WithAlpha(0.45f);
        canvas.FillCircle(cx - face * 0.1f, cy - face * 0.12f, face * 0.7f);

        // Rim light along the bottom edge, then a big glossy highlight and a sparkle.
        canvas.FillColor = Colors.White.WithAlpha(0.35f);
        canvas.FillEllipse(cx - face * 0.5f, cy + face * 0.5f, face * 1.0f, face * 0.3f);
        canvas.FillColor = color;
        canvas.FillEllipse(cx - face * 0.58f, cy + face * 0.38f, face * 1.16f, face * 0.3f);
        canvas.FillColor = Colors.White.WithAlpha(0.9f);
        canvas.FillEllipse(cx - face * 0.66f, cy - face * 0.74f, face * 0.86f, face * 0.5f);
        canvas.FillColor = Colors.White;
        canvas.FillCircle(cx + face * 0.4f, cy - face * 0.36f, Math.Max(1.4f, face * 0.12f));
    }

    public (int r, int c) HitTest(float x, float y)
    {
        if (_cellSize <= 0 || _vm.Rows <= 0 || _vm.Cols <= 0) return (-1, -1);

        int c = (int)((x - _offsetX) / _cellSize);
        int r = (int)((y - _offsetY) / _cellSize);

        if (r < 0 || r >= _vm.Rows || c < 0 || c >= _vm.Cols) return (-1, -1);
        return (r, c);
    }
}
