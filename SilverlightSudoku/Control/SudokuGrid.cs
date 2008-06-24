using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GeniusSudoku.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Elite.Silverlight;

namespace SilverlightSudoku
{

    using TControlOfCell = System.Windows.Controls.ContentControl;

    /// <summary>
    /// 
    /// Gestion du plateau visuel
    /// ========================================
    /// WinFX Custom Control
    /// ========================================
    ///
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:testSudokuGrid.SudokuControl"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:testSudokuGrid.SudokuControl;assembly=testSudokuGrid.SudokuControl"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file. Note that Intellisense in the
    /// XML editor does not currently work on custom controls and its child elements.
    ///
    ///     <MyNamespace:SudokuGrid/>
    ///
    /// </summary>
    public class SudokuGrid : UserControl, INotifyPropertyChanged, IWpfView
    {
        /// <summary>
        /// le jeu
        /// </summary>
        private Sudoku _Sudoku;
        private SudokuConfiguration _Conf;

        /// <summary>
        /// la grille
        /// </summary>
        Grid _Grid;
        /// <summary>
        /// liste des minicellules
        /// </summary>
        List<TControlOfCell> _MiniCellControls;
        /// <summary>
        /// liste des grandes cellules
        /// </summary>
        Dictionary<Point, TControlOfCell> _CellControls;
        /// <summary>
        /// liste indexée des mini cellules
        /// </summary>
        Dictionary<MiniCellKey, SudokuCellView> _MiniCells;

        List<SudokuNumber> _RemainningNumbers;

        //for double clic management (minicell)
        MouseClickManager _miniCellClickMgr;
        //for double management (big cell)
        MouseClickManager _bigCellClickMgr;

        Timer _Clock;
        DateTime _Seconds;

        public SudokuGrid()
        {
            this.Loaded += new RoutedEventHandler(SudokuGrid_Loaded);
            _miniCellClickMgr = new MouseClickManager(this, 300);
            _bigCellClickMgr = new MouseClickManager(this, 300);

            _MiniCellControls = new List<TControlOfCell>();
            _CellControls = new Dictionary<Point, TControlOfCell>();
            _MiniCells = new Dictionary<MiniCellKey, SudokuCellView>();

        }

        void SudokuGrid_Loaded(object sender, RoutedEventArgs e)
        {
            _Sudoku = new Sudoku();
            _Conf = new SudokuConfiguration(this);
            _RemainningNumbers = new List<SudokuNumber>();
            for (int i = 1; i <= 9; i++)
            {
                _RemainningNumbers.Add(new SudokuNumber(i, 9));
            }
            _Sudoku.OnWin += new EventHandler<EventArgs>(_Sudoku_OnWin);
            _Sudoku.OnGameBegin += new EventHandler<EventArgs>(_Sudoku_OnGameBegin);
            _Sudoku.OnRemainingChanged += new EventHandler<RemainingEventArgs>(_Sudoku_OnRemainingChanged);
            _Grid = new Grid();
            Binding binding = new Binding("Background");
            binding.Source = this;
            binding.Mode = BindingMode.OneWay;
            _Grid.SetBinding(Grid.BackgroundProperty, binding);

            _Clock = new Timer(_Clock_Elapsed);

            this.Content = _Grid;
            //27 *2 lignes
            for (int i = 0; i < 27 * 2 + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = CreateGridLength(i);

                _Grid.RowDefinitions.Add(row);
            }
            //27*2 colonnes
            for (int i = 0; i < 27 * 2 + 1; i++)
            {
                ColumnDefinition col = new ColumnDefinition();

                col.Width = CreateGridLength(i);
                _Grid.ColumnDefinitions.Add(col);
            }
            SolidColorBrush lightGray = new SolidColorBrush(Colors.LightGray);
            //le fond de la grille est défini pa l'utilisateur, ce fond sera utilisé pour afficher les lignes
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Rectangle r = new Rectangle();
                    r.Opacity = 0.8;
                    r.Fill = lightGray;
                    Grid.SetColumn(r, j * 6 + 1);
                    Grid.SetColumnSpan(r, 5);
                    Grid.SetRow(r, i * 6 + 1);
                    Grid.SetRowSpan(r, 5);
                    _Grid.Children.Add(r);
                }
            }
            _Grid.ShowGridLines = false;
            //Ajout des mini cellules
            for (int i = 0; i < 27; i++)
            {
                for (int j = 0; j < 27; j++)
                {
                    TControlOfCell b = new TControlOfCell();
                    b.Style = this.MiniCellStyle;
                    b.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(b_MouseLeftButtonDown);
                    b.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(b_MouseLeftButtonUp);
                    _MiniCellControls.Add(b);
                    int x = j % 3;
                    int y = i % 3;
                    b.Content = (y * 3 + x + 1);
                    SudokuCellView scv = new SudokuCellView(_Conf, _Sudoku, (int)j / 3, (int)i / 3, y * 3 + x + 1);
                    b.DataContext = scv;
                    _MiniCells.Add(new MiniCellKey((int)scv.Position.X, (int)scv.Position.Y, (byte)scv.Number), scv);
                    _Grid.Children.Add(b);
                    Grid.SetColumn(b, j * 2 + 1);
                    Grid.SetRow(b, i * 2 + 1);
                    b.VerticalAlignment = VerticalAlignment.Stretch;
                    b.HorizontalAlignment = HorizontalAlignment.Stretch;
                }
            }
            _miniCellClickMgr.Click += new System.Windows.Input.MouseButtonEventHandler(clickMgr_Click);
            _miniCellClickMgr.DoubleClick += new System.Windows.Input.MouseButtonEventHandler(clickMgr_DoubleClick);
            _miniCellClickMgr.LongClick += new System.Windows.Input.MouseButtonEventHandler(_miniCellClickMgr_LongClick);
            _bigCellClickMgr.DoubleClick += new System.Windows.Input.MouseButtonEventHandler(_bigCellClickMgr_DoubleClick);
            Time = "00:00";
        }

        void _miniCellClickMgr_LongClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnCellFlagClick(sender, e);
        }

        void clickMgr_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnMiniCellDoubleClick(sender, e);
        }

        void clickMgr_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnCellClick(sender, e);
        }

        void b_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _miniCellClickMgr.HandleButtonDown(sender, e);
        }

        void b_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _miniCellClickMgr.HandleButtonUp(sender, e);
        }

        void _bigCellClickMgr_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnCellDoubleClick(sender, e);
        }

        void _Sudoku_OnRemainingChanged(object sender, RemainingEventArgs e)
        {
            int index = e.Number - 1;
            _RemainningNumbers[index].Remaining = e.Remining;
        }

        void _Sudoku_OnGameBegin(object sender, EventArgs e)
        {
            if (OnNewGame != null)
                OnNewGame(this, new RoutedEventArgs());
        }

        void _Sudoku_OnWin(object sender, EventArgs e)
        {
            if (_Clock != null)
            {
                _Clock.Dispose();
                _Clock = null;
            }
            if (OnWon != null)
                OnWon(this, new RoutedEventArgs());
        }

        delegate void sansparametre();
        void _Clock_Elapsed(object sender)
        {
            _Seconds = _Seconds.AddSeconds(1);
            Dispatcher.BeginInvoke(new sansparametre(OnTimeChanged));
        }

        void OnCellFlagClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TControlOfCell b = (sender as TControlOfCell);
            SudokuCellView cellview = b.DataContext as SudokuCellView;
            if (cellview.IsMiniCell)
                cellview.FlagUnFlag();
        }

        void OnCellClick(object sender, RoutedEventArgs e)
        {
            TControlOfCell b = (sender as TControlOfCell);
            SudokuCellView cellview = b.DataContext as SudokuCellView;
            if (cellview.IsMiniCell && !cellview.IsFlagged)
            {
                cellview.CheckUnCheck();
            }
        }

        void OnMiniCellDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TControlOfCell b = (sender as TControlOfCell);
            SudokuCellView cellview = b.DataContext as SudokuCellView;
            if (cellview.IsMiniCell && !cellview.IsFlagged)
            {
                //cellview.CheckUnCheck();
                cellview.IsMiniCell = !cellview.IsMiniCell;
                _Sudoku.AddCoup(cellview.X, cellview.Y, (byte)cellview.Number);
                ShowBigNumber(cellview, false);
                if (OnShowBigNumber != null)
                {
                    OnShowBigNumber(_CellControls[cellview.Position], new RoutedEventArgs());
                }
            }
        }

        void OnCellDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TControlOfCell b = (sender as TControlOfCell);
            SudokuCellView cellview = b.DataContext as SudokuCellView;
            ///inversion
            cellview.IsMiniCell = !cellview.IsMiniCell;
            if (cellview.IsMiniCell)
            {
                b.Visibility = Visibility.Collapsed;
                _Sudoku.AddCoup(cellview.X, cellview.Y, 0);
            }
        }

        private void ShowBigNumber(SudokuCellView cellview, bool IsFixed)
        {
            if (_CellControls.ContainsKey(cellview.Position))
            {
                _CellControls[cellview.Position].DataContext = cellview;
                _CellControls[cellview.Position].Visibility = Visibility.Visible;
            }
            else
            {
                var big = new TControlOfCell();
                big.Visibility = Visibility.Collapsed;
                //permet de gérer les nombres non "clickable"
                if (!IsFixed)
                    big.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(big_MouseLeftButtonUp);

                big.DataContext = cellview;
                big.Style = this.BigCellStyle;
                _Grid.Children.Add(big);
                Grid.SetColumn(big, (int)(cellview.Position.X * 6 + 1));
                Grid.SetRow(big, (int)(cellview.Position.Y * 6 + 1));
                Grid.SetColumnSpan(big, 5);
                Grid.SetRowSpan(big, 5);
                big.VerticalAlignment = VerticalAlignment.Stretch;
                big.HorizontalAlignment = HorizontalAlignment.Stretch;
                _CellControls.Add(cellview.Position, big);
                big.Visibility = Visibility.Visible;

                Binding binding = new Binding("Number");
                binding.Mode = BindingMode.OneWay;
                big.SetBinding(ContentControl.ContentProperty, binding);

            }
        }

        void big_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _bigCellClickMgr.HandleClick(sender, e);
        }

        private GridLength CreateGridLength(int index)
        {
            double colWidth = 10;
            if (index % 18 == 0)
                colWidth = 6;
            else if (index % 6 == 0)
                colWidth = 3;
            else if (index % 2 == 0)
                colWidth = 1;
            return new GridLength(colWidth, colWidth == 10 ? GridUnitType.Star : GridUnitType.Pixel);
        }

        public void NewGame(int level)
        {
            if (_Clock != null)
            {
                _Clock.Dispose();
                _Clock = null;
            }
            _Seconds = new DateTime(0);
            ClearBoard();
            for (int i = 0; i < 9; i++)
                _RemainningNumbers[i].Remaining = 9;
            if (level == 0)
                _Sudoku.New(10);
            else
                _Sudoku.New(101);
            _Sudoku.ClearNotes();
            //switch number
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (_Sudoku.IsFixedNumber(i, j))
                    {
                        byte value = _Sudoku[i, j];
                        SudokuCellView cellview = _MiniCells[new MiniCellKey(i, j, value)];
                        ShowBigNumber(cellview, true);
                    }
                }
            }
            Time = "00:00";
            _Clock = new Timer(_Clock_Elapsed, null, 0, 1000);
        }

        public void LoadGame(string fileName, string IDPartie)
        {
            if (_Clock != null)
            {
                _Clock.Dispose();
                _Clock = null;
            }
            _Seconds = new DateTime(0);
            ClearBoard();
            for (int i = 0; i < 9; i++)
                _RemainningNumbers[i].Remaining = 9;
            _Sudoku.Restore(fileName, IDPartie);
            //switch number
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (_Sudoku.IsFixedNumber(i, j))
                    {
                        byte value = _Sudoku[i, j];
                        SudokuCellView cellview = _MiniCells[new MiniCellKey(i, j, value)];
                        ShowBigNumber(cellview, true);
                    }
                }
            }
            Time = "00:00";
            _Clock = new Timer(_Clock_Elapsed, null, 0, 1000);
        }

        public void ClearNotes()
        {
            _Sudoku.ClearNotes();
        }

        private void ChangeBigCellStyle()
        {
            Style newStyle = this.BigCellStyle;
            foreach (var b in _CellControls.Values)
                b.Style = newStyle;
        }

        private void ChangeMiniCellStyle()
        {
            Style newStyle = this.MiniCellStyle;
            foreach (var b in _MiniCellControls)
                b.Style = newStyle;            
        }
        
        private void ClearBoard()
        {
            foreach (var b in _CellControls.Values)
                _Grid.Children.Remove(b);

            _CellControls.Clear();
        }

        public Sudoku BoardData
        {
            get
            {
                return _Sudoku;
            }
        }

        public Style BigCellStyle
        {
            get
            {
                return (Style)base.GetValue(CellTemplateProperty);
            }
            set
            {
                base.SetValue(CellTemplateProperty, value);
            }
        }

        public Style MiniCellStyle
        {
            get
            {
                return (Style)base.GetValue(MiniCellStyleProperty);
            }
            set
            {
                base.SetValue(MiniCellStyleProperty, value);
            }

        }

        public string Time 
        { 
            get; 
            private set; 
        }

        public List<SudokuNumber> Remainings
        {
            get
            {
                return _RemainningNumbers;
            }
        }

        public SudokuConfiguration Configuration
        {
            get
            {
                return _Conf;
            }
        }

        [Category("Behavior")]
        public event RoutedEventHandler OnWon;

        [Category("Behavior")]
        public event RoutedEventHandler OnNewGame;

        public event RoutedEventHandler OnShowBigNumber;

        private void OnTimeChanged()
        {
            Time = _Seconds.ToString("mm:ss");
            NotifyPropertyChanged("Time");
        }

        private static void OnMiniCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SudokuGrid grid = (SudokuGrid)d;
            grid.ChangeMiniCellStyle();
        }
        
        private static void OnBigCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SudokuGrid grid = (SudokuGrid)d;
            grid.ChangeBigCellStyle();
        }


        public static readonly DependencyProperty CellTemplateProperty =
                                    DependencyProperty.Register("BigCellStyle",
                                    typeof(Style),
                                    typeof(SudokuGrid), new PropertyMetadata(OnBigCellStyleChanged));


        public static readonly DependencyProperty MiniCellStyleProperty =
                                    DependencyProperty.Register("MiniCellStyle",
                                    typeof(Style),
                                    typeof(SudokuGrid),new PropertyMetadata(OnMiniCellStyleChanged));

        //public static readonly DependencyProperty TimeProperty =
        //                            DependencyProperty.Register("Time",
        //                            typeof(string),
        //                            typeof(SudokuGrid), null);


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                Dispatcher.BeginInvoke(delegate
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propName));
                });
            }
        }
        #endregion
    }
}
