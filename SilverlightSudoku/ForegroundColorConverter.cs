using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;

namespace SilverlightSudoku
{
    public class ForegroundColorConverter : IValueConverter
    {
        private Brush lightgray = new SolidColorBrush(Colors.LightGray);
        private Brush gray = new SolidColorBrush(Colors.Gray);
        private Brush green = new SolidColorBrush(Colors.Green);


        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var cell = value as SudokuCellView;
            if (cell != null)
            {
                if (cell.Configuration.ShowHint)
                {
                    if (cell.IsNumberPossible)
                    {
                        return green;
                    }
                }
                if (cell.IsChecked)
                {
                    return gray;
                }
            }
            return lightgray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
