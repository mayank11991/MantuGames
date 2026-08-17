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
        new GameInfo { Id = "sudoku",        Title = "Sudoku",         ImageName = "sudoku.png",        CardColor = "#22D3EE", Route = "sudoku",        Description = "Classic number logic with daily grids and progressive difficulty." },
        new GameInfo { Id = "wordfinder",    Title = "Word Finder",    ImageName = "wordfinder.png",    CardColor = "#A855F7", Route = "wordfinder",    Description = "Hunt hidden words, expand your vocabulary, race the clock." },
        new GameInfo { Id = "mathchallenge", Title = "Math Challenge", ImageName = "mathchallenge.png", CardColor = "#F59E0B", Route = "mathchallenge", Description = "Rapid-fire arithmetic that turns numbers into superpowers." },
        new GameInfo { Id = "towerofhanoi",  Title = "Tower of Hanoi", ImageName = "towerofhanoi.png",  CardColor = "#EF4444", Route = "towerofhanoi",  Description = "The timeless puzzle of strategy, patience and precision." },
        new GameInfo { Id = "cardmemory",    Title = "Card Memory",    ImageName = "cardmemory.png",    CardColor = "#34D399", Route = "cardmemory",    Description = "Flip, match and train your memory one card at a time." },
        new GameInfo { Id = "puzzlepets",    Title = "Puzzle Pets",    ImageName = "puzzlegame.png",    CardColor = "#F472B6", Route = "puzzlepets",    Description = "Adorable pet puzzles that grow with you through every level." },
        new GameInfo { Id = "blockpuzzle",   Title = "Block Puzzle",   ImageName = "tetris.png",        CardColor = "#F97316", Route = "blockpuzzle",   Description = "Slide, stack and clear blocks in a fast-paced spatial challenge." },
        new GameInfo { Id = "mazerunner",    Title = "Maze Runner",    ImageName = "mazerunner.png",    CardColor = "#3B82F6", Route = "mazerunner",    Description = "Blaze through twisting mazes — speed, logic and precision." },
        new GameInfo { Id = "animalcrush",   Title = "Animal Crush",   ImageName = "animalcrush.png",   CardColor = "#F43F5E", Route = "animalcrush",   Description = "Match adorable critters in a colorful, satisfying combo rush." },
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
            System.Diagnostics.Debug.WriteLine($"NavigateCommand: {g.Id} -> {g.Route}");
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            await Shell.Current.GoToAsync($"levelmap?gameId={g.Id}");
            System.Diagnostics.Debug.WriteLine($"NavigateCommand completed");
            _isNavigating = false;
        });
    }
}