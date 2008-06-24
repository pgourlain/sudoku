using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace SilverlightSudoku
{
    
    public class SudokuConfiguration : NotifyPropertyChangedClass
    {
        public SudokuConfiguration(IWpfView view)// : base(view)
        {
            _ShowReminder = true;
        }

        private bool _ShowError;
        public bool ShowErrors
        {
            get { return _ShowError; }
            set
            {
                if (value != _ShowError)
                {
                    _ShowError = value;
                    NotifyPropertyChanged("ShowErrors");
                }
            }
        }

        private bool _ShowHint;
        public bool ShowHint
        {
            get { return _ShowHint; }
            set
            {
                if (value != _ShowHint)
                {
                    _ShowHint = value;
                    NotifyPropertyChanged("ShowHint");
                }
            }
        }


        private bool _ShowReminder;
        public bool ShowReminder
        {
            get { return _ShowReminder; }
            set
            {
                if (value != _ShowReminder)
                {
                    _ShowReminder = value;
                    NotifyPropertyChanged("ShowReminder");
                }
            }
        }
    }
}
