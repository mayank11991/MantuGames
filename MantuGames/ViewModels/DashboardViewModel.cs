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
        new GameInfo { Id = "sudoku",           Title = "Sudoku",           ImageName = "sudoku.png",      CardColor = "#7C5CFF", Route = "sudoku",           Description = "Classic sudoku logic",              IconEmoji = "🔢", IconColor = "#2F9E7F" },
        new GameInfo { Id = "wordfinder",       Title = "Word Search",      ImageName = "wordfinder.png",         CardColor = "#FF8FB1", Route = "wordfinder",       Description = "Hunt hidden words",                 IconEmoji = "Aa", IconColor = "#3D8BFD" },
        new GameInfo { Id = "mathchallenge",    Title = "Math Challenge",   ImageName = "mathchallenge.png",      CardColor = "#FFC857", Route = "mathchallenge",    Description = "Rapid-fire arithmetic",             IconEmoji = "🧮", IconColor = "#7C5CFF" },
        new GameInfo { Id = "towerofhanoi",     Title = "Tower of Hanoi",    ImageName = "towerofhanoi.png",       CardColor = "#FF9F7A", Route = "towerofhanoi",     Description = "Puzzle of strategy",                IconEmoji = "🔺", IconColor = "#F09A2E" },
        new GameInfo { Id = "cardmemory",       Title = "Memory Match",     ImageName = "cardmemory.png",         CardColor = "#4FD1B5", Route = "cardmemory",       Description = "Flip, match and train memory",      IconEmoji = "🃏", IconColor = "#E0568F" },
        new GameInfo { Id = "puzzlepets",       Title = "Puzzle Pets",    ImageName = "puzzlepets.png",         CardColor = "#FF9FCF", Route = "puzzlepets",       Description = "Adorable pet puzzles",              IconEmoji = "🧩", IconColor = "#9B7BFF" },
        new GameInfo { Id = "blockpuzzle",      Title = "Block Stack",ImageName = "blockpuzzle.png",        CardColor = "#7FD8FF", Route = "blockpuzzle",      Description = "Slide, stack and clear",            IconEmoji = "🟦", IconColor = "#1FA8C9" },
        new GameInfo { Id = "mazerunner",       Title = "Maze Adventure",   ImageName = "mazerunner.png",         CardColor = "#A78BFA", Route = "mazerunner",       Description = "Blaze through twisting mazes",      IconEmoji = "🗺️", IconColor = "#2F9E7F" },
        new GameInfo { Id = "connectthedots",   Title = "Connect Dots",     ImageName = "connectdots.png",        CardColor = "#5EEAD4", Route = "connectthedots",   Description = "Draw paths to connect pairs",       IconEmoji = "🎨", IconColor = "#E0568F" },
        new GameInfo { Id = "numbermatch",      Title = "Number Matching",     ImageName = "numbermatch.png",        CardColor = "#FFD166", Route = "numbermatch",      Description = "Match numbers that add to 10",      IconEmoji = "⭐", IconColor = "#3D8BFD" },
        new GameInfo { Id = "arrowlines",       Title = "Arrow Lines",       ImageName = "arrowlines.png",         CardColor = "#FF9F7A", Route = "arrowlines",       Description = "Clear the board with arrows",       IconEmoji = "🎮", IconColor = "#9B7BFF" },
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
