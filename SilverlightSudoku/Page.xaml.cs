using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;

namespace SilverlightSudoku
{
    public partial class Page : UserControl
    {
        SudokuGridPresenter _gridPresenter;
        public Page()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Page_Loaded);
            _sudoku.OnShowBigNumber += new RoutedEventHandler(_sudoku_OnShowBigNumber);
            _sudoku.OnWon += new RoutedEventHandler(_sudoku_OnWon);
            _gridPresenter = new SudokuGridPresenter(_sudoku);
        }

        void OnGameWon(object sender, RoutedEventArgs e)
        {
            _sudoku_OnWon(sender, e);
        }

        void _sudoku_OnWon(object sender, RoutedEventArgs e)
        {
            Won dlg = new Won();
            dlg.OnClose += delegate
            {
                this.LayoutRoot.Children.Remove(dlg);
                dlg = null;
            };
            Grid.SetColumnSpan(dlg, 5);
            Grid.SetRowSpan(dlg, 5);
            this.LayoutRoot.Children.Add(dlg);
        }

        void _sudoku_OnShowBigNumber(object sender, RoutedEventArgs e)
        {
            //FrameworkElement elem = sender as FrameworkElement;
            //if (elem != null)
            //{
            //    object anim = elem.FindName("xCellAnimation");
            //    _sb = anim as Storyboard;
            //    _sb.Begin();

            //}
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _gridPresenter.Configuration = _sudoku.Configuration;
            this.leftPanel.DataContext = _gridPresenter;
            this.pnlOptions.DataContext = _gridPresenter;
            this.bSave.DataContext = _gridPresenter;
        }

        private void OnNewGame(object sender, RoutedEventArgs e)
        {
            int level = (bool)rbEasy.IsChecked ? 0 : 101;
            _sudoku.NewGame(level);
        }

        private void OnAbout(object sender, RoutedEventArgs e)
        {
            about dlg = new about();
            dlg.OnClose += delegate
            {
                this.LayoutRoot.Children.Remove(dlg);
                dlg = null;
            };
            //dlg.VerticalAlignment = VerticalAlignment.Center;
            //dlg.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetColumnSpan(dlg, 5);
            Grid.SetRowSpan(dlg, 5);
            this.LayoutRoot.Children.Add(dlg);
        }

        private void ClearNotes(object sender, RoutedEventArgs e)
        {
            _sudoku.ClearNotes();
        }

        private void OnLoadGame(object sender, RoutedEventArgs e)
        {
            LoadDlg dlg = new LoadDlg();
            dlg.OnClose += delegate
            {
                this.LayoutRoot.Children.Remove(dlg);
                if (!dlg.IsCanceled)
                    _sudoku.LoadGame(LoadDlgPresenter.FILENAME, dlg.Presenter.Selected.ID);
                dlg = null;
            };
            //dlg.VerticalAlignment = VerticalAlignment.Center;
            //dlg.HorizontalAlignment = HorizontalAlignment.Center;
            Grid.SetColumnSpan(dlg, 5);
            Grid.SetRowSpan(dlg, 5);
            this.LayoutRoot.Children.Add(dlg);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnLoadGame(sender, e);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //string fileName = _sudoku.BoardData.IDPartie + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmm") + "sudokuPart.xml"; 
            _sudoku.BoardData.Save(LoadDlgPresenter.FILENAME, _sudoku.BoardData.IDPartie);
        }
    }
}
