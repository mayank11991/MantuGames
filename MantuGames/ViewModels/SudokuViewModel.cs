using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Models;
using Microsoft.Maui.Controls;

namespace MantuGames.ViewModels
{
    public class CellViewModel : INotifyPropertyChanged
    {
        private int _value;
        private bool _isFixed, _isSelected, _isError, _isHighlighted, _isRevealed;

        public int Row { get; set; }
        public int Col { get; set; }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayValue));
            }
        }

        public string DisplayValue => Value == 0 ? "" : Value.ToString();

        public bool IsFixed
        {
            get => _isFixed;
            set
            {
                _isFixed = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsError
        {
            get => _isError;
            set
            {
                _isError = value;
                OnPropertyChanged();
            }
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                _isHighlighted = value;
                OnPropertyChanged();
            }
        }

        public bool IsRevealed
        {
            get => _isRevealed;
            set
            {
                _isRevealed = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class GameViewModel : INotifyPropertyChanged
    {
        private SudokuPuzzle _puzzle;
        private CellViewModel _selectedCell;
        private bool _isGameOver, _isWin;
        private int _errorsCount;
        private System.Threading.Timer _timer;

        public bool SolutionWasShown { get; private set; }

        public ObservableCollection<CellViewModel> Cells { get; } = new();

        private int _timeRemainingSec = 210;

        public int TimeRemainingSec
        {
            get => _timeRemainingSec;
            set
            {
                _timeRemainingSec = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimerDisplay));
                OnPropertyChanged(nameof(TimerProgress));
            }
        }

        public string TimerDisplay => $"{TimeRemainingSec / 60}:{TimeRemainingSec % 60:D2}";
        public double TimerProgress => TimeRemainingSec / 210.0;

        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                _isGameOver = value;
                OnPropertyChanged();
            }
        }

        public bool IsWin
        {
            get => _isWin;
            set
            {
                _isWin = value;
                OnPropertyChanged();
            }
        }

        private int _currentLevel;

        public int CurrentLevel
        {
            get => _currentLevel;
            set
            {
                _currentLevel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LevelDisplay));
            }
        }

        public string LevelDisplay => $"Level {CurrentLevel}";

        public int ErrorsCount
        {
            get => _errorsCount;
            set
            {
                _errorsCount = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectCellCommand { get; }
        public ICommand EnterNumberCommand { get; }
        public ICommand EraseCommand { get; }
        public ICommand ShowSolutionCommand { get; }

        public event Action<bool> GameEnded;

        public GameViewModel(int level = 1)
        {
            CurrentLevel = level;

            SelectCellCommand = new Command<CellViewModel>(SelectCell);
            EnterNumberCommand = new Command<string>(s =>
            {
                if (int.TryParse(s, out int n)) EnterNumber(n);
            });
            EraseCommand = new Command(Erase);
            ShowSolutionCommand = new Command(ShowSolution);

            StartLevel(level);
        }

        private void StartLevel(int level)
        {
            CurrentLevel = level;
            TimeRemainingSec = 210;
            IsGameOver = false;
            IsWin = false;
            ErrorsCount = 0;
            SolutionWasShown = false;
            _selectedCell = null;

            _puzzle = SudokuPuzzle.Generate(level);
            Cells.Clear();

            for (int r = 0; r < 5; r++)
            for (int c = 0; c < 5; c++)
                Cells.Add(new CellViewModel
                {
                    Row = r, Col = c,
                    Value = _puzzle.Board[r, c],
                    IsFixed = _puzzle.IsFixed[r, c]
                });

            StopTimer();
            StartTimer();
        }

        private void StartTimer()
        {
            _timer = new System.Threading.Timer(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (TimeRemainingSec > 0) TimeRemainingSec--;
                    else EndGame(false);
                });
            }, null, 1000, 1000);
        }

        public void Cleanup() => StopTimer();
        public void PauseTimer() => StopTimer();
        public void ResumeTimer() => StartTimer();

        private void StopTimer()
        {
            _timer?.Dispose();
            _timer = null;
        }

        private void SelectCell(CellViewModel cell)
        {
            if (cell == null || cell.IsFixed) return;
            foreach (var c in Cells)
            {
                c.IsSelected = false;
                c.IsHighlighted = false;
            }

            _selectedCell = cell;
            cell.IsSelected = true;
            foreach (var c in Cells)
                if (!c.IsSelected && (c.Row == cell.Row || c.Col == cell.Col))
                    c.IsHighlighted = true;
        }

        private void EnterNumber(int number)
        {
            if (_selectedCell == null || _selectedCell.IsFixed || IsGameOver) return;

            int row = _selectedCell.Row, col = _selectedCell.Col;
            _puzzle.Board[row, col] = number;
            _selectedCell.Value = number;
            _selectedCell.IsRevealed = false;

            // Validate against solution — only correct answer is accepted as correct
            bool correct = (number == _puzzle.Solution[row, col]);
            _selectedCell.IsError = !correct;
            if (!correct) ErrorsCount++;
            else RevalidateAllCells();

            if (_puzzle.IsSolved()) EndGame(true);
        }

        private void RevalidateAllCells()
        {
            foreach (var cell in Cells)
            {
                if (cell.IsFixed || cell.IsRevealed) continue;
                if (cell.Value == 0)
                {
                    cell.IsError = false;
                    continue;
                }

                cell.IsError = (cell.Value != _puzzle.Solution[cell.Row, cell.Col]);
            }
        }

        private void Erase()
        {
            if (_selectedCell == null || _selectedCell.IsFixed || IsGameOver) return;
            _puzzle.Board[_selectedCell.Row, _selectedCell.Col] = 0;
            _selectedCell.Value = 0;
            _selectedCell.IsError = false;
            _selectedCell.IsRevealed = false;
        }

        private async void ShowSolution()
        {
            StopTimer();
            IsGameOver = true;
            SolutionWasShown = true;

            foreach (var cell in Cells)
            {
                if (!cell.IsFixed)
                {
                    int val = _puzzle.Solution[cell.Row, cell.Col];
                    _puzzle.Board[cell.Row, cell.Col] = val;
                    cell.Value = val;
                    cell.IsError = false;
                    cell.IsSelected = false;
                    cell.IsHighlighted = false;
                    cell.IsRevealed = true;
                }
            }

            IsWin = false;
            await Task.Delay(6000);
            GameEnded?.Invoke(false);
        }

        private void EndGame(bool win)
        {
            StopTimer();
            IsWin = win;
            IsGameOver = true;
            GameEnded?.Invoke(win);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}