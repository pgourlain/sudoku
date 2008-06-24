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
using System.Timers;

namespace WPFGeniusSudoku.SudokuControl
{
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
    public class SudokuGrid : ContentControl
    {
        /// <summary>
        /// le jeu
        /// </summary>
        private Sudoku _Sudoku;

        /// <summary>
        /// la grille
        /// </summary>
        Grid _Grid;
        /// <summary>
        /// liste des minicellules
        /// </summary>
        List<Button> _MiniCellControls;
        /// <summary>
        /// liste des grandes cellules
        /// </summary>
        Dictionary<Point,Button> _CellControls;
        /// <summary>
        /// liste indexée des mini cellules
        /// </summary>
        Dictionary<MiniCellKey, SudokuCellView> _MiniCells;

        List<SudokuNumber> _RemainningNumbers;

        Timer _Clock;
        DateTime _Seconds;

        public SudokuGrid()
        {
            _Sudoku = new Sudoku();
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

            _MiniCellControls = new List<Button>();
            _CellControls = new Dictionary<Point, Button>();
            _MiniCells = new Dictionary<MiniCellKey, SudokuCellView>();
            _Clock = new Timer(1000);
            //_Clock.
            _Clock.Elapsed += new ElapsedEventHandler(_Clock_Elapsed);

            this.Content = _Grid;
            //27 *2 lignes
            for (int i = 0; i < 27*2+1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = CreateGridLength(i);
                
                _Grid.RowDefinitions.Add(row);
            }
            //27*2 colonnes
            for (int i = 0; i < 27*2+1; i++)
            {
                ColumnDefinition col = new ColumnDefinition();

                col.Width = CreateGridLength(i);  
                _Grid.ColumnDefinitions.Add(col);
            }
            //le fond de la grille est défini pa l'utilisateur, ce fond sera utilisé pour afficher les lignes
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Rectangle r = new Rectangle();
                    r.Opacity = 0.8;
                    r.Fill = Brushes.LightGray;
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
                    Button b = new Button();
                    b.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(OnMiniCellDoubleClick);
                    b.Click += new RoutedEventHandler(OnCellClick);
                    b.MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(OnCellFlagClick);
                    _MiniCellControls.Add(b);
                    int x = j % 3;
                    int y = i % 3;
                    b.Content = (y * 3 + x + 1) ;
                    SudokuCellView scv = new SudokuCellView(_Sudoku, (int)j / 3, (int)i / 3, y * 3 + x + 1);
                    b.DataContext = scv;
                    _MiniCells.Add(new MiniCellKey((int)scv.Position.X, (int)scv.Position.Y, (byte)scv.Number), scv);
                    _Grid.Children.Add(b);
                    Grid.SetColumn(b, j*2+1);
                    Grid.SetRow(b, i*2+1);
                    b.VerticalAlignment = VerticalAlignment.Stretch;
                    b.HorizontalAlignment = HorizontalAlignment.Stretch;
                }
            }
            this.Content = _Grid;
        }

        void _Sudoku_OnRemainingChanged(object sender, RemainingEventArgs e)
        {
            int index = e.Number - 1;
            _RemainningNumbers[index].Remaining = e.Remining;
        }

        void _Sudoku_OnGameBegin(object sender, EventArgs e)
        {
            RoutedEventArgs eNewGame = new RoutedEventArgs(OnNewGameEvent);
            base.RaiseEvent(eNewGame);
        }

        void _Sudoku_OnWin(object sender, EventArgs e)
        {
            _Clock.Stop();
            RoutedEventArgs eWon = new RoutedEventArgs(OnWonEvent);
            base.RaiseEvent(eWon);
        }

        delegate void sansparametre();
        void _Clock_Elapsed(object sender, ElapsedEventArgs e)
        {
            _Seconds = _Seconds.AddSeconds(1);
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new sansparametre(OnThread));

        }

        void OnCellFlagClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Button b = (sender as Button);
            SudokuCellView cellview = b.DataContext as SudokuCellView;
            if (cellview.IsMiniCell)
                cellview.FlagUnFlag();
        }

        void OnCellClick(object sender, RoutedEventArgs e)
        {
            Button b = (sender as Button);
            SudokuCellView cellview = b.DataContext as SudokuCellView;
            if (cellview.IsMiniCell && !cellview.IsFlagged)
                cellview.CheckUnCheck();
        }

        void OnMiniCellDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Button b = (sender as Button);
            SudokuCellView cellview = b.DataContext as SudokuCellView;
            if (cellview.IsMiniCell)
            {
                cellview.CheckUnCheck();
                cellview.IsMiniCell = !cellview.IsMiniCell;
                _Sudoku.AddCoup(cellview.X, cellview.Y, (byte)cellview.Number);
                ShowBigNumber(cellview, false);
            }
        }

        void OnCellDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Button b = (sender as Button);
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
                Button big = new Button();
                big.Visibility = Visibility.Collapsed;
                //me permet de gérer les nombres non "clickable"
                if (!IsFixed)
                    big.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(OnCellDoubleClick);
                big.DataContext = cellview;
                big.Template = this.CellTemplate;
                _Grid.Children.Add(big);
                Grid.SetColumn(big, (int)(cellview.Position.X * 6 + 1));
                Grid.SetRow(big, (int)(cellview.Position.Y * 6 + 1));
                Grid.SetColumnSpan(big, 5);
                Grid.SetRowSpan(big, 5);
                big.VerticalAlignment = VerticalAlignment.Stretch;
                big.HorizontalAlignment = HorizontalAlignment.Stretch;
                _CellControls.Add(cellview.Position, big);
                big.Visibility = Visibility.Visible;
            }
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
            _Clock.Stop();
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
            _Clock.Interval = 1000;
            Time = "00:00";
            _Clock.Start();
        }

        public void ClearNotes()
        {
            _Sudoku.ClearNotes();
        }

        private void ChangeMiniCellTemplate()
        {
            ControlTemplate template = this.MiniCellTemplate;
            foreach (Button b in _MiniCellControls)
                b.Template = template;
        }

        private void ChangeCellTemplate()
        {
            ControlTemplate template = this.CellTemplate;
            foreach (Button b in _CellControls.Values)
                b.Template = template;
        }

        private void ChangeMiniCellStyle()
        {
            Style newStyle = this.MiniCellStyle;
            foreach (Button b in _MiniCellControls)
                b.Style = newStyle;            
        }
        
        private void ClearBoard()
        {
            foreach (Button b in _CellControls.Values)
                _Grid.Children.Remove(b);

            _CellControls.Clear();
        }

        public ControlTemplate CellTemplate
        {
            get
            {
                return (ControlTemplate)base.GetValue(CellTemplateProperty);
            }
            set
            {
                base.SetValue(CellTemplateProperty, value);
            }
        }

        public ControlTemplate MiniCellTemplate
        {
            get
            {
                return (ControlTemplate)base.GetValue(MiniCellTemplateProperty);
            }
            set
            {
                base.SetValue(MiniCellTemplateProperty, value);
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
            get
            {
                return (string)base.GetValue(TimeProperty);
            }
            private set
            {
                base.SetValue(TimeProperty, value);
            }
        }

        public List<SudokuNumber> Remainings
        {
            get
            {
                return _RemainningNumbers;
            }
        }

        [Category("Behavior")]
        public event RoutedEventHandler OnWon
        {
            add
            {
                base.AddHandler(OnWonEvent, value);
            }
            remove
            {
                base.RemoveHandler(OnWonEvent, value);
            }
        }
        
        [Category("Behavior")]
        public event RoutedEventHandler OnNewGame
        {
            add
            {
                base.AddHandler(OnNewGameEvent, value);
            }
            remove
            {
                base.RemoveHandler(OnNewGameEvent, value);
            }
        }

        private void OnThread()
        {
            Time = _Seconds.ToString("mm:ss");
        }

        private static void OnMiniCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SudokuGrid grid = (SudokuGrid)d;
            grid.ChangeMiniCellStyle();
        }

        private static void OnMiniCellTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SudokuGrid grid = (SudokuGrid)d;
            grid.ChangeMiniCellTemplate();
        }
        
        private static void OnCellTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SudokuGrid grid = (SudokuGrid)d;
            grid.ChangeCellTemplate();
        }


        public static readonly DependencyProperty CellTemplateProperty =
                                    DependencyProperty.Register("CellTemplate",
                                    typeof(ControlTemplate),
                                    typeof(SudokuGrid),
                                    new PropertyMetadata(OnCellTemplateChanged));

        public static readonly DependencyProperty MiniCellTemplateProperty =
                                    DependencyProperty.Register("MiniCellTemplate",
                                    typeof(ControlTemplate),
                                    typeof(SudokuGrid),
                                    new PropertyMetadata(OnMiniCellTemplateChanged));
        public static readonly DependencyProperty MiniCellStyleProperty =
                                    DependencyProperty.Register("MiniCellStyle",
                                    typeof(Style),
                                    typeof(SudokuGrid),
                                    new PropertyMetadata(OnMiniCellStyleChanged));

        public static readonly DependencyProperty TimeProperty =
                                    DependencyProperty.Register("Time",
                                    typeof(string),
                                    typeof(SudokuGrid));
        
        public static readonly RoutedEvent OnWonEvent = EventManager.RegisterRoutedEvent("OnWon", 
                                                                        RoutingStrategy.Direct, 
                                                                        typeof(RoutedEventHandler), 
                                                                        typeof(SudokuGrid));
        public static readonly RoutedEvent OnNewGameEvent = EventManager.RegisterRoutedEvent("OnNewGame",
                                                                        RoutingStrategy.Direct,
                                                                        typeof(RoutedEventHandler),
                                                                        typeof(SudokuGrid));
    }
}
