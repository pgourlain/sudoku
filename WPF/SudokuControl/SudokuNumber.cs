using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace WPFGeniusSudoku.SudokuControl
{
    public class SudokuNumber : INotifyPropertyChanged
    {
        private int _Number;
        private int _NumberRestant;

        public SudokuNumber(int aNumber, int aNumberRestant)
        {
            _Number = aNumber;
            _NumberRestant = aNumberRestant;
        }

        public int Number
        {
            get
            {
                return _Number;
            }
        }

        public int Remaining
        {
            get
            {
                return _NumberRestant;
            }
            internal set
            {
                _NumberRestant = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Remaining"));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
