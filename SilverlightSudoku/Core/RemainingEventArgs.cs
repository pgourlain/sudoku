using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusSudoku.Core
{
    public class RemainingEventArgs : EventArgs
    {
        private int _Number;
        private int _Remaining;
        
        public RemainingEventArgs(int number, int remaining)
        {
            _Number = number;
            _Remaining = remaining;
        }

        public int Number
        {
            get
            {
                return _Number;
            }
        }

        public int Remining
        {
            get
            {
                return _Remaining;
            }
        }
    }
}
