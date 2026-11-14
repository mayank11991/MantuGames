using MantuGames.Models;
using Xunit;

namespace MantuGames.Tests;

public class ArrowLinesTests
{
    [Theory]
    [InlineData(1)] [InlineData(5)] [InlineData(12)] [InlineData(25)] [InlineData(40)] [InlineData(60)]
    public void EveryGeneratedBoardCanBeCleared(int level)
    {
        for (int seed = 0; seed < 40; seed++)
        {
            var p = ArrowLinesPuzzle.Generate(level, new Random(seed * 7919 + level));
            Assert.NotEmpty(p.Lines);

            // Greedy: any line with a free exit can go; the backwards build guarantees one exists.
            while (!p.AllCleared)
            {
                var next = p.Lines.FirstOrDefault(p.CanEscape);
                Assert.NotNull(next);
                p.Remove(next!);
            }
        }
    }

    [Fact]
    public void LinesAreContiguousAndArrowFollowsTheLine()
    {
        var p = ArrowLinesPuzzle.Generate(20, new Random(3));
        foreach (var line in p.Lines)
        {
            Assert.True(line.Cells.Count >= 2);
            for (int i = 1; i < line.Cells.Count; i++)
            {
                var (r0, c0) = line.Cells[i - 1];
                var (r1, c1) = line.Cells[i];
                Assert.Equal(1, Math.Abs(r1 - r0) + Math.Abs(c1 - c0));
            }
            var (dr, dc) = ArrowLine.Step(line.Direction);
            var head = line.Head;
            var beforeHead = line.Cells[^2];
            Assert.Equal((head.Row - dr, head.Col - dc), beforeHead);
        }
    }

    [Fact]
    public void BlockedLineReportsDistanceToObstacle()
    {
        var p = ArrowLinesPuzzle.Generate(30, new Random(11));
        foreach (var line in p.Lines)
        {
            int run = p.FreeRunBeforeBlock(line);
            if (run < 0) continue;
            var ray = p.RayCells(line.Head.Row, line.Head.Col, line.Direction);
            Assert.NotNull(p.LineAt(ray[run].Row, ray[run].Col));
        }
    }
}
