using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using GeniusSudoku.Core;
using System.Windows;

namespace SilverlightSudoku
{
    public class SudokuCellView : NotifyPropertyChangedClass
    {
        private SudokuConfiguration _Conf;
        private Sudoku _Board;
        int _X, _Y;
        int _MiniCellNumber;
        int _xCell, _yCell;
        bool _IsMiniCell = true;
        bool _GameBegin = false;

        static SudokuCellView()
        {
        }

        public SudokuCellView(SudokuConfiguration aConfiguration, Sudoku aBoard, int aX, int aY, int aMiniCellNumber)
        {
            _Conf = aConfiguration;
            _Board = aBoard;
            _Y = aY;
            _X = aX;
            _MiniCellNumber = aMiniCellNumber;
            _yCell = _MiniCellNumber / 3;
            _xCell = _MiniCellNumber - _yCell * 3;
            _Board.OnFlagChanged += new EventHandler<EventArgs>(_Board_OnFlagChanged);
            _Board.OnCheckChanged += new EventHandler<EventArgs>(_Board_OnCheckChanged);
            _Board.OnGameBegin += new EventHandler<EventArgs>(_Board_OnGameBegin);
            _Board.OnCoupAdded += new EventHandler<EventArgs>(_Board_OnCoupAdded);
            _Conf.PropertyChanged += new PropertyChangedEventHandler(_Conf_PropertyChanged);
        }

        void _Conf_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName))
            {
                DoNumberIsPossibleChanged(); 
                DoErrorChanged();
            }
            else 
                switch (e.PropertyName)
                {
                    case "ShowErrors" :
                        DoErrorChanged();
                        break;
                    case "ShowHint" :
                        DoNumberIsPossibleChanged();
                        break;
                }
        }

        void _Board_OnCoupAdded(object sender, EventArgs e)
        {
            DoNumberIsPossibleChanged();
            DoErrorChanged();
        }

        void _Board_OnGameBegin(object sender, EventArgs e)
        {
            _GameBegin = true;
            DoNumberIsPossibleChanged();
            NotifyPropertyChanged("IsFixed");
        }

        void _Board_OnCheckChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged("IsChecked");
            DoErrorChanged();
            NotifyPropertyChanged("THIS");
        }

        private void DoNumberIsPossibleChanged()
        {
            NotifyPropertyChanged("IsNumberPossible");
            NotifyPropertyChanged("THIS");
        }

        private void DoErrorChanged()
        {
            NotifyPropertyChanged("IsError");
        }

        void _Board_OnFlagChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged("IsFlagged");
            NotifyPropertyChanged("THIS");
        }

        /// <summary>
        /// le nombre 1 -> 9
        /// </summary>
        public int Number
        {
            get
            {                
                return _MiniCellNumber;
            }
        }

        /// <summary>
        /// suis-je en mode minicell ? -> Isflagged et Ischecked ont un sens, sinon IsFixed
        /// </summary>
        public bool IsMiniCell
        {
            get { return _IsMiniCell; }
            set 
            {
                if (_IsMiniCell != value)
                {
                    _IsMiniCell = value;
                    DoNumberIsPossibleChanged();
                    DoErrorChanged();
                }
            }
        }

        /// <summary>
        /// coordonnées logique de la cellule, (pas de la mini cellule) point(0,0) -> point(8,8) 
        /// </summary>
        public Point Position
        {
            get
            {
                return new Point(_X, _Y);
            }
        }

        /// <summary>
        /// la case est marqué par l'utilisateur comme n'étant pas une possibilité (par défaut affiche une croix)
        /// </summary>
        public bool IsFlagged
        {
            get
            {
               return _Board.IsFlagged(_X, _Y, _xCell, _yCell);
            }
        }

        /// <summary>
        /// correspond au clic gauche de l'utilisateur
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return _Board.IsCheckedCell(_X, _Y, _xCell, _yCell);
            }
        }

        public bool IsError
        {
            get
            {
                if (_Conf.ShowErrors)
                {
                    if (IsChecked && _IsMiniCell)
                        return !_Board.IsNumberPossible(_X, _Y, (byte)_MiniCellNumber);
                    else if (!_IsMiniCell)
                        return _Board.IsError(_X, _Y);
                }
                return false;
            }
        }

        public bool IsNumberPossible
        {
            get
            {
                if (_GameBegin)
                    return _Board.IsNumberPossible(_X, _Y, (byte)_MiniCellNumber);
                return false;
            }
        }

        /// <summary>
        /// true quand c'est un nombre fixe (la case ne peut être modifiée)
        /// </summary>
        public bool IsFixed
        {
            get
            {
                return _Board.IsFixedNumber(_X, _Y);
            }
        }

        internal void CheckUnCheck()
        {
            bool check = _Board.IsCheckedCell(_X,_Y,_xCell,_yCell);
            _Board.CheckCell(_X, _Y, _xCell, _yCell, !check);
        }

        internal void FlagUnFlag()
        {
            bool flag = _Board.IsFlagged(_X, _Y, _xCell, _yCell);
            _Board.FlagCell(_X, _Y, _xCell, _yCell, !flag);
        }

        public int X
        {
            get
            {
                return _X;
            }
        }

        public int Y
        {
            get
            {
                return _Y;
            }
        }

        public SudokuConfiguration Configuration
        {
            get
            {
                return _Conf;
            }
        }

        public SudokuCellView THIS
        {
            get
            {
                return this;
            }
        }
    }
}
