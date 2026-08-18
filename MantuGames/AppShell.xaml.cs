namespace MantuGames;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("levelmap", typeof(Views.LevelMapPage));
        Routing.RegisterRoute("sudoku", typeof(Views.SudokuPage));
        Routing.RegisterRoute("wordfinder", typeof(Views.WordFinderPage));
        Routing.RegisterRoute("mathchallenge", typeof(Views.MathGamePage));
        Routing.RegisterRoute("towerofhanoi", typeof(Views.TowerOfHanoiPage));
        Routing.RegisterRoute("cardmemory", typeof(Views.CardMemoryPage));
        Routing.RegisterRoute("mazerunner", typeof(Views.MazeRunnerPage));
        Routing.RegisterRoute("puzzlepets", typeof(Views.PuzzlePetsPage));
        Routing.RegisterRoute("blockpuzzle", typeof(Views.BlockPuzzlePage));
        // Routing.RegisterRoute("animalcrush", typeof(Views.AnimalCrushPage)); // Paused: not in 8-game release
    }
}