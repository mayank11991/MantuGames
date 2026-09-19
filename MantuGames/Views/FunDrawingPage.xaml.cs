using MantuGames.Models;
using MantuGames.Services;
using Microsoft.Maui.Graphics;

namespace MantuGames.Views;

public partial class FunDrawingPage : ContentPage
{
    private FunDrawingGrid _grid;
    private FunDrawingDrawable _drawable;
    private bool _isDrawing;
    private int _currentColorIndex;
    private Random _rng = new();

    public FunDrawingPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartLevel(1);
        this.Opacity = 0;
        this.FadeTo(1, 350);
    }

    private void StartLevel(int level)
    {
        _grid = FunDrawingGrid.ForLevel(level);
        _currentColorIndex = _rng.Next(12);
        _drawable = new FunDrawingDrawable { Grid = _grid };
        DrawingCanvas.Drawable = _drawable;
        LevelLabel.Text = $"Level {level}";
    }

    private void OnPointerPressed(object sender, PointerEventArgs e)
    {
        _isDrawing = true;
        _currentColorIndex = _rng.Next(12);
        ColorAtPosition(e);
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (!_isDrawing) return;
        ColorAtPosition(e);
    }

    private void OnPointerReleased(object sender, PointerEventArgs e)
    {
        _isDrawing = false;
    }

    private void ColorAtPosition(PointerEventArgs e)
    {
        var pos = e.GetPosition(DrawingCanvas);
        if (pos == null) return;

        float cs = _drawable.CellSize;
        if (cs <= 0) return;

        int col = (int)((pos.Value.X - _drawable.OffsetX) / cs);
        int row = (int)((pos.Value.Y - _drawable.OffsetY) / cs);

        if (row >= 0 && row < _grid.Rows && col >= 0 && col < _grid.Cols)
        {
            _grid.ColorCell(row, col, _currentColorIndex);
            DrawingCanvas.Invalidate();
            AudioService.Instance.Play("pop");
        }
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        for (int r = 0; r < _grid.Rows; r++)
        for (int c = 0; c < _grid.Cols; c++)
            _grid.CellColors[r, c] = -1;
        DrawingCanvas.Invalidate();
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("..");
    }
}

internal sealed class FunDrawingDrawable : IDrawable
{
    public FunDrawingGrid? Grid { get; set; }
    public float CellSize { get; private set; }
    public float OffsetX { get; private set; }
    public float OffsetY { get; private set; }

    public void Draw(ICanvas canvas, RectF bounds)
    {
        if (Grid == null) return;

        var (ox, oy, cs) = Layout(bounds);

        // Draw grid cells
        for (int r = 0; r < Grid.Rows; r++)
        for (int c = 0; c < Grid.Cols; c++)
        {
            float x = ox + c * cs;
            float y = oy + r * cs;
            string color = Grid.GetColor(r, c);
            canvas.FillColor = Color.FromArgb(color);
            canvas.FillRoundedRectangle(x + 1, y + 1, cs - 2, cs - 2, cs * 0.15f);
        }

        // Draw grid lines
        canvas.StrokeColor = Color.FromArgb("#1AFFFFFF");
        canvas.StrokeSize = 0.5f;
        for (int r = 0; r <= Grid.Rows; r++)
            canvas.DrawLine(ox, oy + r * cs, ox + Grid.Cols * cs, oy + r * cs);
        for (int c = 0; c <= Grid.Cols; c++)
            canvas.DrawLine(ox + c * cs, oy, ox + c * cs, oy + Grid.Rows * cs);
    }

    private (float ox, float oy, float cs) Layout(RectF bounds)
    {
        if (Grid == null) return (0, 0, 0);
        CellSize = Math.Min(bounds.Width / Grid.Cols, bounds.Height / Grid.Rows);
        OffsetX = (bounds.Width - CellSize * Grid.Cols) / 2f;
        OffsetY = (bounds.Height - CellSize * Grid.Rows) / 2f;
        return (OffsetX, OffsetY, CellSize);
    }
}
