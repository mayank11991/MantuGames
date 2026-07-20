using System.Collections.ObjectModel;
using System.Windows.Input;
using MantuGames.Helpers;
using MantuGames.Models;
using MantuGames.Services;

namespace MantuGames.ViewModels;

public class DashboardViewModel
{
    // Candy palette: 8 evenly-spaced hues so no two cards read as the same color.
    // Small fixed tilts so the grid looks hand-placed (like the reference screenshot).
    // Alternating left/right keeps it visually balanced.
    public static readonly ObservableCollection<GameInfo> Games = new()
    {
        new GameInfo { Id = "sudoku",        Title = "Sudoku",         ImageName = "sudoku",        CardColor = "#7C4DFF", Route = "sudoku"       },
        new GameInfo { Id = "wordfinder",    Title = "Word Finder",    ImageName = "wordfinder",    CardColor = "#00C2A8", Route = "wordfinder"   },
        new GameInfo { Id = "mathchallenge", Title = "Math Challenge", ImageName = "mathchallenge", CardColor = "#FF6B35", Route = "mathchallenge"},
        new GameInfo { Id = "towerofhanoi",  Title = "Tower of Hanoi", ImageName = "towerofhanoi",  CardColor = "#FF3D9A", Route = "towerofhanoi" },
        new GameInfo { Id = "cardmemory",    Title = "Card Memory",    ImageName = "cardmemory",    CardColor = "#2F8FFF", Route = "cardmemory"   },
        new GameInfo { Id = "puzzlepets",    Title = "Puzzle Pets",    ImageName = "puzzlegame",    CardColor = "#FFC145", Route = "puzzlepets"    },
        new GameInfo { Id = "blockpuzzle",   Title = "Block Puzzle",   ImageName = "tetris",   CardColor = "#4CAF50", Route = "blockpuzzle"  },
        new GameInfo { Id = "mazerunner",    Title = "Maze Runner",    ImageName = "mazerunner",    CardColor = "#FF4B5C", Route = "mazerunner"   },
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
            AudioService.Instance.Play("tap");
            VibrationHelper.Click();
            await Shell.Current.GoToAsync($"levelmap?gameId={g.Id}");
            _isNavigating = false;
        });
    }
}