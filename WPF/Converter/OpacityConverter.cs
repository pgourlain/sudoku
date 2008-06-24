using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using WPFGeniusSudoku.SudokuControl;
using System.Windows;

namespace GeniusSudokuConverter
{
    public class OpacityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return ((bool)value) ? 1.0f : parameter;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
