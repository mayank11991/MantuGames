using MantuGames.Models;
using Xunit;

namespace MantuGames.Tests;

public class MazeTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
    [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)] [InlineData(11)] [InlineData(17)]
    [InlineData(30)] [InlineData(50)]
    public void EveryShapedLevel_IsAPerfectMazeFromStartToExit(int level)
    {
        var m = MazePuzzle.ForLevel(level);
        int active = 0;
        for (int r = 0; r < m.Rows; r++)
        for (int c = 0; c < m.Cols; c++)
            if (m.Active[r, c]) active++;

        Assert.True(active >= 40, $"{m.ShapeName} has only {active} cells");
        Assert.True(m.IsActive(m.Start.r, m.Start.c));
        Assert.True(m.IsActive(m.End.r, m.End.c));
        Assert.NotEqual(m.Start, m.End);

        // Every cell of the picture is reachable from the entrance.
        for (int r = 0; r < m.Rows; r++)
        for (int c = 0; c < m.Cols; c++)
            if (m.Active[r, c])
                Assert.NotNull(m.PathTo(m.Start, (r, c)));

        // No passage leads outside the shape.
        for (int r = 0; r < m.Rows; r++)
        for (int c = 0; c < m.Cols; c++)
            if (!m.Active[r, c])
                Assert.False(m.CanMove(r, c, 0, 1) || m.CanMove(r, c, 1, 0) || m.CanMove(r, c, -1, 0) || m.CanMove(r, c, 0, -1));
    }

    [Fact]
    public void ConsecutiveLevels_HaveDifferentShapes()
    {
        for (int l = 1; l < 20; l++)
            Assert.NotEqual(MazePuzzle.ForLevel(l).ShapeName, MazePuzzle.ForLevel(l + 1).ShapeName);
    }
}
