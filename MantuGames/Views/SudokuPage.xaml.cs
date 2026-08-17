using MantuGames.Helpers;
using MantuGames.ViewModels;
using MantuGames.Services;
using MantuGames.Views.Controls;

namespace MantuGames.Views
{
    [QueryProperty(nameof(Level), "level")]
    public partial class SudokuPage : ContentPage
    {
        private static readonly string[] AnimalImages =
        {
            "cat.png", "elephant.png", "lion.png", "owl.png", "butterfly.png"
        };

        private GameViewModel _vm;
        private int _startLevel = 1;

        public string Level
        {
            set
            {
                if (int.TryParse(value, out int l)) _startLevel = l;
            }
        }

        public SudokuPage()
        {
            InitializeComponent();
            this.AddBannerAd();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AudioService.Instance.StartMusic();

            // (Re)create VM with the level from the map
            _vm = new GameViewModel(_startLevel);
            TimerView.TotalSeconds = ProgressService.GetTimerSeconds(_startLevel);
            BindingContext = _vm;
            _vm.GameEnded += OnGameEnded;

            this.Loaded += (s, e) => BuildSudokuGrid();

            this.Opacity = 0;
            this.FadeTo(1, 400);
            PauseOverlay.Resumed += OnResumeGame;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            PauseOverlay.Resumed -= OnResumeGame;
            if (_vm != null)
            {
                _vm.GameEnded -= OnGameEnded;
                _vm.Cleanup();
            }
        }

        // ── Pause / Resume ──────────────────────────────────────────
        private void OnPause(object sender, EventArgs e)
        {
            try { _vm?.PauseTimer(); } catch { }
            PauseOverlay.Show();
        }

        private void OnResumeGame(object sender, EventArgs e)
        {
            try { _vm?.ResumeTimer(); } catch { }
        }

        private void OnShowSolutionClicked(object sender, EventArgs e)
        {
            if (CoinService.GetCoins("sudoku") < CoinService.SolutionCost)
            {
                CoinShopPopup.ShowForGame("sudoku");
                return;
            }
            CoinService.SpendCoins("sudoku", CoinService.SolutionCost);
            _vm?.ShowSolutionCommand.Execute(null);
        }

        // ── BACK ────────────────────────────────────────────────────
        private async void OnBackClicked(object sender, EventArgs e)
        {
            try
            {
                bool leave = await ConfirmPopup.Show("Leave Game?", "Your progress will be lost if you leave.", "Leave", "Stay");
                if (!leave) return;
                _vm?.Cleanup();
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnBackClicked: {ex.Message}");
            }
        }

        // ── GRID ────────────────────────────────────────────────────
        private void BuildSudokuGrid()
        {
            SudokuGrid.Children.Clear();
            SudokuGrid.RowDefinitions.Clear();
            SudokuGrid.ColumnDefinitions.Clear();

            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            double cellSize = Math.Min((screenWidth - 60) / 5.0, 62);

            for (int i = 0; i < 5; i++)
            {
                SudokuGrid.RowDefinitions.Add(new RowDefinition { Height = cellSize });
                SudokuGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = cellSize });
            }

            foreach (var cell in _vm.Cells)
            {
                var border = CreateCellFrame(cell, (int)cellSize);
                Grid.SetRow(border, cell.Row);
                Grid.SetColumn(border, cell.Col);
                SudokuGrid.Children.Add(border);
            }
        }

        private Border CreateCellFrame(CellViewModel cell, int size)
        {
            var image = new Image
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = size * 0.6,
                HeightRequest = size * 0.6,
                InputTransparent = true,
                Aspect = Aspect.AspectFit
            };

            void UpdateImage()
            {
                if (cell.Value > 0 && cell.Value <= 5)
                    image.Source = ImageSource.FromFile(AnimalImages[cell.Value - 1]);
                else
                    image.Source = null;
            }
            UpdateImage();

            var border = new Border
            {
                Padding = new Thickness(0),
                Margin = new Thickness(1),
                StrokeThickness = 0,
                HeightRequest = size,
                WidthRequest = size,
                Content = image,
                BindingContext = cell
            };
            border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 };

            void UpdateBackground()
            {
                if (cell.IsSelected) border.BackgroundColor = Color.FromArgb("#FFE066");
                else if (cell.IsError) border.BackgroundColor = Color.FromArgb("#FF6B6B");
                else if (cell.IsRevealed) border.BackgroundColor = Color.FromArgb("#EDE7F6");
                else if (cell.IsFixed) border.BackgroundColor = Color.FromArgb("#B8E4FF");
                else if (cell.IsHighlighted) border.BackgroundColor = Color.FromArgb("#EEF6FF");
                else if (cell.Value == 0) border.BackgroundColor = Color.FromArgb("#FFF8E7");
                else border.BackgroundColor = Color.FromArgb("#F0FFF0");
            }

            UpdateBackground();

            cell.PropertyChanged += (s, e) =>
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateBackground();
                    UpdateImage();
                });

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) =>
            {
                if (!cell.IsFixed && !cell.IsRevealed && !_vm.IsGameOver)
                {
                    _vm.SelectCellCommand.Execute(cell);
                    AudioService.Instance.Play("tap");
                    await border.ScaleTo(1.1, 70);
                    await border.ScaleTo(1.0, 70);
                }
            };
            border.GestureRecognizers.Add(tap);

            cell.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(CellViewModel.Value) && cell.Value != 0 && !cell.IsRevealed)
                {
                    await image.ScaleTo(1.35, 90);
                    await image.ScaleTo(1.0, 90);
                }

                if (e.PropertyName == nameof(CellViewModel.IsError) && cell.IsError)
                {
                    AudioService.Instance.Play("wobble");
                    await border.TranslateTo(-5, 0, 50);
                    await border.TranslateTo(5, 0, 50);
                    await border.TranslateTo(0, 0, 50);
                }
            };

            return border;
        }

        // ── GAME EVENTS ─────────────────────────────────────────────
        private async void OnGameEnded(bool isWin)
        {
            try
            {
                int elapsed = 210 - _vm.TimeRemainingSec;
                string reason = null;

                if (!isWin && _vm.SolutionWasShown)
                    reason = "Solution Shown";

                int stars  = isWin && reason == null ? ProgressService.CalcStars(elapsed, 210) : 0;
                int coins = isWin && reason == null ? stars switch { 3 => 5, 2 => 3, 1 => 1, _ => 0 } : 0;

                await ResultPopup.Show(isWin, _vm.CurrentLevel, elapsed, 210, stars, coins, reason, "sudoku");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnGameEnded: {ex.Message}");
            }
        }

        private void OnNextLevel(object sender, EventArgs e)
        {
            _startLevel = _vm.CurrentLevel + 1;
            _vm.GameEnded -= OnGameEnded;
            _vm = new GameViewModel(_startLevel);
            BindingContext = _vm;
            _vm.GameEnded += OnGameEnded;
            BuildSudokuGrid();
        }

        private void OnRetry(object sender, EventArgs e)
        {
            _vm.GameEnded -= OnGameEnded;
            _vm = new GameViewModel(_startLevel);
            BindingContext = _vm;
            _vm.GameEnded += OnGameEnded;
            BuildSudokuGrid();
        }
    }
}