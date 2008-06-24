using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Threading;

namespace SilverlightSudoku
{
    /// <summary>
    /// interface of view in MVP pattern
    /// </summary>
    public interface IWpfView
    {
        Dispatcher Dispatcher {get;}
    }

    /// <summary>
    /// Base presenter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasePresenter<T> : NotifyPropertyChangedClass where T : IWpfView
    {

        public BasePresenter(T aView)
        {
            View = aView;
        }

        public T View { get; private set; }
    }

    /// <summary>
    /// Presenter of sudoku
    /// </summary>
    public class SudokuGridPresenter : BasePresenter<SudokuGrid>
    {

        public SudokuGridPresenter(SudokuGrid grid) : base(grid)
        {
            Configuration = grid.Configuration;
            grid.OnNewGame += new RoutedEventHandler(grid_OnNewGame);
            grid.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(grid_PropertyChanged);
        }

        void grid_OnNewGame(object sender, RoutedEventArgs e)
        {
            IsGameStarted = true;
        }

        void grid_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(e.PropertyName);
            //System.Diagnostics.Debug.WriteLine("grid_PropertyChanged : "+ e.PropertyName);
        }


        public List<SudokuNumber> Remainings
        {
            get
            {
                return View.Remainings;
            }
        }

        SudokuConfiguration _Configuration;
        public SudokuConfiguration Configuration 
        {
            set
            {
                if (_Configuration != null)
                    _Configuration.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(_Configuration_PropertyChanged);
                _Configuration = value;
                if (_Configuration != null)
                    _Configuration.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_Configuration_PropertyChanged);
            }
        }

        void _Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("_Configuration_PropertyChanged : " + e.PropertyName);
            NotifyPropertyChanged(e.PropertyName);
        }


        public bool ShowHint
        {
            get { return View.Configuration.ShowHint; }
            set
            {
                View.Configuration.ShowHint = value;
            }
        }


        public bool ShowReminder
        {
            get 
            { 
                return View.Configuration.ShowReminder; 
            }
            set
            {
                View.Configuration.ShowReminder = value;
            }
        }


        public bool ShowErrors
        {
            get { return View.Configuration.ShowErrors; }
            set
            {
                View.Configuration.ShowErrors = value;
            }
        }

        public string Time
        {
            get
            {
                return View.Time;
            }
        }

        bool _IsGameStarted;
        public bool IsGameStarted
        {
            get
            {
                return _IsGameStarted;
            }
            set
            {
                if (_IsGameStarted != value)
                {
                    _IsGameStarted = value;
                    NotifyPropertyChanged("IsGameStarted");
                }
            }
        }
    }
}
