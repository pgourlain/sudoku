using System;
using System.Collections.Generic;
using System.Text;

namespace SilverlightSudoku
{
    /// <summary>
    /// structure servant de clé dans au dictionnaire des minicellules
    /// </summary>
    struct MiniCellKey
    {
        public int X;
        public int Y;
        public byte MiniCellNumber;

        public MiniCellKey(int ax, int aY, byte aValue)
        {
            X = ax;
            Y = aY;
            MiniCellNumber = aValue;
        }
    }
}
