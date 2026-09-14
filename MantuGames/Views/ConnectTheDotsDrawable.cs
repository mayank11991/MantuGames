using System.Collections.Generic;
using MantuGames.ViewModels;
using Microsoft.Maui.Graphics;

namespace MantuGames.Views;

public class ConnectTheDotsDrawable : IDrawable
{
    private readonly ConnectTheDotsViewModel _vm;
    private float _cellSize;
    private float _gridOffsetX;
    private float _gridOffsetY;

    public ConnectTheDotsDrawable(ConnectTheDotsViewModel vm)
    {
        _vm = vm;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[CTD-Draw] Draw called: {dirtyRect.Width}x{dirtyRect.Height}, pairs={_vm.Pairs.Count}");
            if (_vm.Pairs.Count == 0 || dirtyRect.Width <= 0 || dirtyRect.Height <= 0) return;

            int size = _vm.GridSize;
            if (size <= 0) return;

            float padding = 8f;
            float availableWidth = dirtyRect.Width - padding * 2;
            float availableHeight = dirtyRect.Height - padding * 2;
            _cellSize = Math.Min(availableWidth, availableHeight) / size;

            if (_cellSize <= 0) return;

            float gridWidth = _cellSize * size;
            float gridHeight = _cellSize * size;
            _gridOffsetX = (dirtyRect.Width - gridWidth) / 2;
            _gridOffsetY = (dirtyRect.Height - gridHeight) / 2;

            DrawBackground(canvas, dirtyRect);
            DrawGridCells(canvas, size);
            DrawCompletedPaths(canvas, size);
            DrawActivePath(canvas, size);
            DrawDots(canvas, size);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CTD-Draw] ERROR: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void DrawBackground(ICanvas canvas, RectF rect)
    {
        canvas.FillColor = Color.FromArgb("#0D1117");
        canvas.FillRectangle(rect);
    }

    private void DrawGridCells(ICanvas canvas, int size)
    {
        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                float x = _gridOffsetX + c * _cellSize;
                float y = _gridOffsetY + r * _cellSize;

                // Subtle cell background
                canvas.FillColor = Color.FromArgb("#161B22");
                canvas.FillRectangle(x + 0.5f, y + 0.5f, _cellSize - 1, _cellSize - 1);
            }
        }

        // Subtle grid lines
        canvas.StrokeColor = Color.FromArgb("#21262D");
        canvas.StrokeSize = 0.5f;

        for (int i = 0; i <= size; i++)
        {
            float x = _gridOffsetX + i * _cellSize;
            canvas.DrawLine(x, _gridOffsetY, x, _gridOffsetY + gridHeight(size));
        }
        for (int i = 0; i <= size; i++)
        {
            float y = _gridOffsetY + i * _cellSize;
            canvas.DrawLine(_gridOffsetX, y, _gridOffsetX + gridWidth(size), y);
        }
    }

    private float gridWidth(int size) => _cellSize * size;
    private float gridHeight(int size) => _cellSize * size;

    private void DrawCompletedPaths(ICanvas canvas, int size)
    {
        foreach (var kvp in _vm.CompletedPaths)
        {
            if (kvp.Value.Count < 2) continue;

            var color = _vm.GetPairColor(kvp.Key);
            var points = CellIndicesToPoints(kvp.Value, size);

            // Glow layer
            DrawSmoothPath(canvas, points, color.WithAlpha(0.25f), _cellSize * 0.6f);

            // Main path
            DrawSmoothPath(canvas, points, color, _cellSize * 0.32f);

            // Highlight center
            DrawSmoothPath(canvas, points, color.WithAlpha(0.4f), _cellSize * 0.12f);
        }
    }

    private void DrawActivePath(ICanvas canvas, int size)
    {
        if (!_vm.ActivePairId.HasValue || _vm.CurrentPath.Count < 1) return;

        var color = _vm.GetPairColor(_vm.ActivePairId.Value);

        if (_vm.CurrentPath.Count >= 2)
        {
            var points = CellIndicesToPoints(_vm.CurrentPath, size);

            // Glow
            DrawSmoothPath(canvas, points, color.WithAlpha(0.3f), _cellSize * 0.7f);

            // Main path
            DrawSmoothPath(canvas, points, color, _cellSize * 0.38f);

            // Highlight
            DrawSmoothPath(canvas, points, Colors.White.WithAlpha(0.2f), _cellSize * 0.1f);
        }

        // Head indicator
        int headCell = _vm.CurrentPath[^1];
        int hr = headCell / size, hc = headCell % size;
        float hx = CellCenterX(hc);
        float hy = CellCenterY(hr);
        canvas.FillColor = color.WithAlpha(0.6f);
        canvas.FillCircle(hx, hy, _cellSize * 0.18f);
    }

    private void DrawDots(ICanvas canvas, int size)
    {
        foreach (var pair in _vm.Pairs)
        {
            var color = pair.Color;
            float radius = _cellSize * 0.28f;

            DrawEndpoint(canvas, pair.StartRow, pair.StartCol, color, radius);
            DrawEndpoint(canvas, pair.EndRow, pair.EndCol, color, radius);
        }
    }

    private void DrawEndpoint(ICanvas canvas, int row, int col, Color color, float radius)
    {
        float cx = CellCenterX(col);
        float cy = CellCenterY(row);

        // Outer glow
        canvas.FillColor = color.WithAlpha(0.2f);
        canvas.FillCircle(cx, cy, radius * 1.6f);

        // Main dot
        canvas.FillColor = color;
        canvas.FillCircle(cx, cy, radius);

        // Inner highlight
        canvas.FillColor = Colors.White.WithAlpha(0.35f);
        canvas.FillCircle(cx - radius * 0.25f, cy - radius * 0.25f, radius * 0.35f);

        // White ring
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 2f;
        canvas.DrawCircle(cx, cy, radius + 1);
    }

    private void DrawSmoothPath(ICanvas canvas, List<PointF> points, Color color, float strokeSize)
    {
        if (points.Count < 2) return;

        canvas.StrokeColor = color;
        canvas.StrokeSize = strokeSize;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        var path = new PathF();
        path.MoveTo(points[0].X, points[0].Y);

        if (points.Count == 2)
        {
            path.LineTo(points[1].X, points[1].Y);
        }
        else
        {
            // Use quadratic curves for smoother corners
            for (int i = 1; i < points.Count - 1; i++)
            {
                float midX = (points[i].X + points[i + 1].X) / 2;
                float midY = (points[i].Y + points[i + 1].Y) / 2;
                path.QuadTo(points[i].X, points[i].Y, midX, midY);
            }
            // Line to the last point
            path.LineTo(points[^1].X, points[^1].Y);
        }

        canvas.DrawPath(path);
    }

    private float CellCenterX(int col) => _gridOffsetX + col * _cellSize + _cellSize / 2;
    private float CellCenterY(int row) => _gridOffsetY + row * _cellSize + _cellSize / 2;

    public int HitTest(float tapX, float tapY, int gridSize)
    {
        if (_cellSize <= 0) return -1;

        int col = (int)((tapX - _gridOffsetX) / _cellSize);
        int row = (int)((tapY - _gridOffsetY) / _cellSize);

        if (col < 0 || col >= gridSize || row < 0 || row >= gridSize)
            return -1;

        return row * gridSize + col;
    }

    private List<PointF> CellIndicesToPoints(IReadOnlyList<int> cells, int size)
    {
        var points = new List<PointF>(cells.Count);
        foreach (int cell in cells)
        {
            int row = cell / size, col = cell % size;
            points.Add(new PointF(CellCenterX(col), CellCenterY(row)));
        }
        return points;
    }
}
