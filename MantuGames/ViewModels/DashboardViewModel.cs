using System.Collections.ObjectModel;
using System.Windows.Input;
using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.ViewModels;

public class DashboardViewModel
{
    // Matches the reference dashboard: 2-column grid, each card has
    // a large thumbnail on top and a bottom bar with colored icon + bold name.
    public static readonly ObservableCollection<GameInfo> Games = new()
    {
        new GameInfo { Id = "sudoku",           Title = "Sudoku",           ImageName = "gi_sudoku.png",      CardColor = "#5AA6FF", Route = "sudoku",           Description = "Classic sudoku logic",              IconEmoji = "🔢", IconColor = "#2F9E7F" },
        new GameInfo { Id = "wordfinder",       Title = "Word Search",      ImageName = "gi_wordfinder.png",         CardColor = "#FF8FC0", Route = "wordfinder",       Description = "Hunt hidden words",                 IconEmoji = "Aa", IconColor = "#3D8BFD" },
        new GameInfo { Id = "mathchallenge",    Title = "Math Challenge",   ImageName = "gi_mathchallenge.png",      CardColor = "#F6C93C", Route = "mathchallenge",    Description = "Rapid-fire arithmetic",             IconEmoji = "🧮", IconColor = "#7C5CFF" },
        new GameInfo { Id = "towerofhanoi",     Title = "Tower of Hanoi",    ImageName = "gi_towerofhanoi.png",       CardColor = "#FF9A3D", Route = "towerofhanoi",     Description = "Puzzle of strategy",                IconEmoji = "🔺", IconColor = "#F09A2E" },
        new GameInfo { Id = "cardmemory",       Title = "Memory Match",     ImageName = "gi_cardmemory.png",         CardColor = "#7BE8B0", Route = "cardmemory",       Description = "Flip, match and train memory",      IconEmoji = "🃏", IconColor = "#E0568F" },
        new GameInfo { Id = "puzzlepets",       Title = "Puzzle Pets",    ImageName = "gi_puzzlepets.png",         CardColor = "#C79BFF", Route = "puzzlepets",       Description = "Adorable pet puzzles",              IconEmoji = "🧩", IconColor = "#9B7BFF" },
        new GameInfo { Id = "blockpuzzle",      Title = "Block Stack",ImageName = "gi_blockpuzzle.png",        CardColor = "#4FD2F5", Route = "blockpuzzle",      Description = "Slide, stack and clear",            IconEmoji = "🟦", IconColor = "#1FA8C9" },
        new GameInfo { Id = "mazerunner",       Title = "Maze Adventure",   ImageName = "gi_mazerunner.png",         CardColor = "#9AE04A", Route = "mazerunner",       Description = "Blaze through twisting mazes",      IconEmoji = "🗺️", IconColor = "#2F9E7F" },
        new GameInfo { Id = "connectthedots",   Title = "Connect Dots",     ImageName = "gi_connectdots.png",        CardColor = "#3FE0C5", Route = "connectthedots",   Description = "Draw paths to connect pairs",       IconEmoji = "🎨", IconColor = "#E0568F" },
        new GameInfo { Id = "numbermatch",      Title = "Number Matching",     ImageName = "gi_numbermatch.png",        CardColor = "#FFB547", Route = "numbermatch",      Description = "Match numbers that add to 10",      IconEmoji = "⭐", IconColor = "#3D8BFD" },
        new GameInfo { Id = "arrowlines",       Title = "Arrow Lines",       ImageName = "gi_arrowlines.png",         CardColor = "#FF7A59", Route = "arrowlines",       Description = "Clear the board with arrows",       IconEmoji = "🎮", IconColor = "#9B7BFF" },
    };

    // Instance copy for binding
    public ObservableCollection<GameInfo> GamesList => Games;

    private bool _isNavigating;

    public ICommand NavigateCommand { get; }

    public DashboardViewModel()
    {
        NavigateCommand = new Command<GameInfo>(async g =>
        {
            if (_isNavigating || g == null) return;
            _isNavigating = true;
            Console.WriteLine($"NavigateCommand: {g.Id} -> {g.Route}");
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            await Shell.Current.GoToAsync($"levelmap?gameId={g.Id}");
            Console.WriteLine($"NavigateCommand completed");
            _isNavigating = false;
        });
    }
}
