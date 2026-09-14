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

        DrawCells(canvas, rows, cols);
        DrawPaths(canvas, rows, cols);
        DrawDots(canvas, rows, cols);
    }

    private void DrawCells(ICanvas canvas, int rows, int cols)
    {
        float radius = _cellSize * 0.16f;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = _offsetX + c * _cellSize;
                float y = _offsetY + r * _cellSize;
                canvas.FillColor = Color.FromArgb("#1A2332");
                canvas.FillRoundedRectangle(x + 2, y + 2, _cellSize - 4, _cellSize - 4, radius);
            }
        }
    }

    private void DrawPaths(ICanvas canvas, int rows, int cols)
    {
        float lineRadius = _cellSize * 0.22f;

        foreach (var kvp in _vm.CompletedPaths)
        {
            if (kvp.Value.Count < 1) continue;
            DrawPathLine(canvas, kvp.Value, _vm.GetPairColor(kvp.Key), lineRadius);
        }

        if (_vm.ActivePair.HasValue && _vm.CurrentPath.Count >= 1)
        {
            DrawPathLine(canvas, _vm.CurrentPath, _vm.GetPairColor(_vm.ActivePair.Value), lineRadius);
        }
    }

    private void DrawPathLine(ICanvas canvas, List<(int r, int c)> path, Color color, float radius)
    {
        canvas.FillColor = color;
        canvas.StrokeColor = color;
        canvas.StrokeSize = radius * 2;
        canvas.StrokeLineCap = LineCap.Round;

        if (path.Count == 1)
        {
            float cx = _offsetX + path[0].c * _cellSize + _cellSize / 2;
            float cy = _offsetY + path[0].r * _cellSize + _cellSize / 2;
            canvas.FillCircle(cx, cy, radius);
            return;
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            float x1 = _offsetX + path[i].c * _cellSize + _cellSize / 2;
            float y1 = _offsetY + path[i].r * _cellSize + _cellSize / 2;
            float x2 = _offsetX + path[i + 1].c * _cellSize + _cellSize / 2;
            float y2 = _offsetY + path[i + 1].r * _cellSize + _cellSize / 2;
            canvas.DrawLine(x1, y1, x2, y2);
        }

        foreach (var (r, c) in path)
        {
            float cx = _offsetX + c * _cellSize + _cellSize / 2;
            float cy = _offsetY + r * _cellSize + _cellSize / 2;
            canvas.FillCircle(cx, cy, radius);
        }
    }

    private void DrawDots(ICanvas canvas, int rows, int cols)
    {
        foreach (var pair in _vm.Pairs)
        {
            float radius = _cellSize * 0.32f;
            DrawDot(canvas, pair.R1, pair.C1, pair.Color, radius);
            DrawDot(canvas, pair.R2, pair.C2, pair.Color, radius);
        }
    }

    private void DrawDot(ICanvas canvas, int r, int c, Color color, float radius)
    {
        float cx = _offsetX + c * _cellSize + _cellSize / 2;
        float cy = _offsetY + r * _cellSize + _cellSize / 2;

        canvas.FillColor = color;
        canvas.FillCircle(cx, cy, radius);

        canvas.FillColor = Colors.White.WithAlpha(0.3f);
        canvas.FillCircle(cx - radius * 0.2f, cy - radius * 0.2f, radius * 0.3f);
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
