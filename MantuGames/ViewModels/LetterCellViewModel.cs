using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantuGames.Models;

namespace MantuGames.ViewModels;

public class LetterCellViewModel : INotifyPropertyChanged
    {
        private bool _isSelected, _isFound, _isHinted;
        public int  Row { get; set; }
        public int  Col { get; set; }
        public char Letter { get; set; }
 
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
        public bool IsFound    { get => _isFound;    set { _isFound = value;    OnPropertyChanged(); } }
        public bool IsHinted   { get => _isHinted;   set { _isHinted = value;   OnPropertyChanged(); } }
 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
 
    public class WordViewModel : INotifyPropertyChanged
    {
        private bool _isFound;
        public string Word     { get => _word;    set { _word = value;    OnPropertyChanged(); } }
        public bool   IsFound  { get => _isFound; set { _isFound = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayWord)); } }
        private string _word;
        public string DisplayWord => IsFound ? $"✅ {Word}" : Word;
 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
 
    public class WordFinderViewModel : INotifyPropertyChanged
    {
        private WordPuzzle _puzzle;
        private System.Threading.Timer _timer;
        private bool _isGameOver, _isWin;
        private int _timeRemainingSec = 210;
        private int _currentLevel;
        private readonly List<(int Row, int Col)> _selection = new();
 
        public ObservableCollection<LetterCellViewModel> Cells { get; } = new();
        public ObservableCollection<WordViewModel>       Words { get; } = new();
 
        public int GridSize { get; private set; }
 
        public int TimeRemainingSec
        {
            get => _timeRemainingSec;
            set { _timeRemainingSec = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimerDisplay)); OnPropertyChanged(nameof(TimerProgress)); }
        }
        public string TimerDisplay  => $"{TimeRemainingSec / 60}:{TimeRemainingSec % 60:D2}";
        public double TimerProgress => TimeRemainingSec / 210.0;
 
        public bool IsGameOver { get => _isGameOver; set { _isGameOver = value; OnPropertyChanged(); } }
        public bool IsWin      { get => _isWin;      set { _isWin = value;      OnPropertyChanged(); } }
        public bool SolutionWasShown { get; private set; }
 
        public int CurrentLevel
        {
            get => _currentLevel;
            set { _currentLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(LevelDisplay)); }
        }
        public string LevelDisplay => $"Level {CurrentLevel}";
 
        public ICommand TapCellCommand      { get; }
        public ICommand ShowSolutionCommand { get; }
 
        public event Action<bool> GameEnded;
 
        public WordFinderViewModel(int level = 1)
        {
            TapCellCommand      = new Command<LetterCellViewModel>(TapCell);
            ShowSolutionCommand = new Command(ShowSolution);
            StartLevel(level);
        }
 
        private void StartLevel(int level)
        {
            CurrentLevel      = level;
            TimeRemainingSec  = 210;
            IsGameOver        = false;
            IsWin             = false;
            SolutionWasShown  = false;
            _selection.Clear();
 
            _puzzle  = WordPuzzle.Generate(level);
            GridSize = _puzzle.GridSize;
 
            Cells.Clear();
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    Cells.Add(new LetterCellViewModel
                        { Row = r, Col = c, Letter = _puzzle.Grid[r, c] });
 
            Words.Clear();
            foreach (var pw in _puzzle.Words)
                Words.Add(new WordViewModel { Word = pw.Word });
 
            StopTimer();
            StartTimer();
        }
 
        private void TapCell(LetterCellViewModel cell)
        {
            if (IsGameOver || cell == null) return;
 
            var pos = (cell.Row, cell.Col);
 
            // Already in selection — if it's the last one, deselect it
            if (_selection.Contains(pos))
            {
                if (_selection.Last() == pos)
                {
                    _selection.Remove(pos);
                    cell.IsSelected = false;
                }
                return;
            }
 
            // Validate continuity — must be adjacent to last selected cell
            if (_selection.Count > 0)
            {
                var last = _selection.Last();
                int dr = Math.Abs(cell.Row - last.Row);
                int dc = Math.Abs(cell.Col - last.Col);
                bool adjacent = dr <= 1 && dc <= 1 && (dr + dc > 0);
 
                // Also must be in same direction as existing selection
                if (_selection.Count > 1)
                {
                    var first = _selection[0];
                    var second = _selection[1];
                    int edir = Math.Sign(second.Row - first.Row);
                    int edic = Math.Sign(second.Col - first.Col);
                    int ndir = Math.Sign(cell.Row - last.Row);
                    int ndic = Math.Sign(cell.Col - last.Col);
                    if (ndir != edir || ndic != edic) { ClearSelection(); return; }
                }
 
                if (!adjacent) { ClearSelection(); return; }
            }
 
            _selection.Add(pos);
            cell.IsSelected = true;
 
            // Check if selection forms a word
            var found = _puzzle.CheckSelection(_selection);
            if (found != null)
            {
                // Mark cells as found
                foreach (var p in _selection)
                    GetCell(p.Row, p.Col).IsFound = true;
 
                // Update word list
                var wvm = Words.FirstOrDefault(w => w.Word == found.Word);
                if (wvm != null) wvm.IsFound = true;
 
                ClearSelection();
 
                if (_puzzle.AllFound()) EndGame(true);
            }
        }
 
        public void ClearSelection()
        {
            foreach (var p in _selection)
            {
                var c = GetCell(p.Row, p.Col);
                if (c != null && !c.IsFound) c.IsSelected = false;
            }
            _selection.Clear();
        }

        public void AddToSelection(LetterCellViewModel cell)
        {
            if (IsGameOver || cell == null) return;
            var pos = (cell.Row, cell.Col);
            if (_selection.Contains(pos)) return;
            if (_selection.Count == 0)
            {
                _selection.Add(pos);
                cell.IsSelected = true;
                return;
            }

            var last = _selection.Last();
            int dr = cell.Row - last.Row;
            int dc = cell.Col - last.Col;
            int adr = Math.Abs(dr);
            int adc = Math.Abs(dc);

            bool straight = adr == adc || dr == 0 || dc == 0;
            if (!straight || (adr + adc == 0)) return;

            if (_selection.Count > 1)
            {
                var first = _selection[0];
                var second = _selection[1];
                int edir = Math.Sign(second.Row - first.Row);
                int edic = Math.Sign(second.Col - first.Col);
                int ndir = Math.Sign(dr);
                int ndic = Math.Sign(dc);
                if (ndir != edir || ndic != edic) return;
            }

            int stepR = dr == 0 ? 0 : dr / adr;
            int stepC = dc == 0 ? 0 : dc / adc;
            int r = last.Row + stepR;
            int c = last.Col + stepC;
            while (r != cell.Row || c != cell.Col)
            {
                if (!_selection.Contains((r, c)))
                {
                    _selection.Add((r, c));
                    var midCell = GetCell(r, c);
                    if (midCell != null) midCell.IsSelected = true;
                }
                r += stepR;
                c += stepC;
            }

            _selection.Add(pos);
            cell.IsSelected = true;

            var found = _puzzle.CheckSelection(_selection);
            if (found != null)
            {
                foreach (var p in _selection)
                    GetCell(p.Row, p.Col).IsFound = true;
                var wvm = Words.FirstOrDefault(w => w.Word == found.Word);
                if (wvm != null) wvm.IsFound = true;
                ClearSelection();
                if (_puzzle.AllFound()) EndGame(true);
            }
        }
 
        private async void ShowSolution()
        {
            StopTimer();
            IsGameOver       = true;
            SolutionWasShown = true;
 
            foreach (var pw in _puzzle.Words.Where(w => !w.Found))
            {
                foreach (var pos in _puzzle.GetWordCells(pw))
                    GetCell(pos.Row, pos.Col).IsHinted = true;
 
                var wvm = Words.FirstOrDefault(w => w.Word == pw.Word);
                if (wvm != null) wvm.IsFound = true;
            }
 
            IsWin = false;
            await Task.Delay(2000);
            GameEnded?.Invoke(false);
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
        private void StopTimer() { _timer?.Dispose(); _timer = null; }
 
        private void EndGame(bool win)
        {
            StopTimer();
            IsWin = win; IsGameOver = true;
            GameEnded?.Invoke(win);
        }
 
        private LetterCellViewModel GetCell(int r, int c) =>
            Cells.FirstOrDefault(x => x.Row == r && x.Col == c);
 
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }