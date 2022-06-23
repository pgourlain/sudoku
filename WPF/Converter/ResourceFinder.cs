using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace GeniusSudokuConverter
{
    public class ResourceFinder : IValueConverter
    {
        #region IValueConverter Members

        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string? aKey = value.ToString();
            if (parameter != null)
                aKey = parameter.ToString()+aKey;
            if (Application.Current.Resources.Contains(aKey))
                return Application.Current.Resources[aKey];
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("todo");
        }

        #endregion
    }
}
