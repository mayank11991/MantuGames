using System.Collections.ObjectModel;
using System.Windows.Input;
using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.ViewModels;

public class DashboardViewModel
{
    // Matches the website game grid (mayank11991.github.io): per-game accent colors,
    // image icons and taglines from the site's game cards.
    public static readonly ObservableCollection<GameInfo> Games = new()
    {
        new GameInfo { Id = "sudoku",        Title = "Sudoku",         ImageName = "sudoku.png",        CardColor = "#FFC704", Route = "sudoku",        Description = "Classic sudoku logic, jungle touch" },
        new GameInfo { Id = "wordfinder",    Title = "Word Finder",    ImageName = "wordfinder.png",    CardColor = "#B3DA5D", Route = "wordfinder",    Description = "Hunt hidden words, expand your vocabulary" },
        new GameInfo { Id = "mathchallenge", Title = "Math Challenge", ImageName = "mathchallenge.png", CardColor = "#188C8A", Route = "mathchallenge", Description = "Rapid-fire arithmetic" },
        new GameInfo { Id = "towerofhanoi",  Title = "Tower of Hanoi", ImageName = "towerofhanoi.png",  CardColor = "#FFC704", Route = "towerofhanoi",  Description = "Puzzle of strategy, patience and precision." },
        new GameInfo { Id = "cardmemory",    Title = "Card Memory",    ImageName = "cardmemory.png",    CardColor = "#B3DA5D", Route = "cardmemory",    Description = "Flip, match and train your memory" },
        new GameInfo { Id = "puzzlepets",    Title = "Puzzle Pets",    ImageName = "puzzlegame.png",    CardColor = "#EC6C28", Route = "puzzlepets",    Description = "Adorable pet puzzles" },
        new GameInfo { Id = "blockpuzzle",   Title = "Block Puzzle",   ImageName = "tetris.png",        CardColor = "#FFC704", Route = "blockpuzzle",   Description = "Slide, stack and clear" },
        new GameInfo { Id = "mazerunner",       Title = "Maze Runner",       ImageName = "mazerunner.png",    CardColor = "#EC6C28", Route = "mazerunner",       Description = "Blaze through twisting mazes" },
        new GameInfo { Id = "connectthedots",  Title = "Connect the Dots",  ImageName = "connectthedots.png", CardColor = "#188C8A", Route = "connectthedots",  Description = "Draw paths to connect matching pairs" },
        new GameInfo { Id = "numbermatch",     Title = "Number Match",     ImageName = "numbermatch.png",    CardColor = "#B3DA5D", Route = "numbermatch",     Description = "Match identical numbers or pairs that add up to 10" },
        new GameInfo { Id = "arrowlines",      Title = "Arrow Lines",      ImageName = "arrow_lines.png",     CardColor = "#3B82F6", Route = "arrowlines",      Description = "Clear the board by tapping arrows in their direction" },
        // new GameInfo { Id = "animalcrush",   Title = "Animal Crush",   ImageName = "animalcrush.png",   CardColor = "#F43F5E", Route = "animalcrush",   Description = "Match adorable critters in a colorful, satisfying combo rush." }, // Paused: not in 8-game release
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