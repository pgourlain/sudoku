using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Reflection;
using System.Windows;
using System.IO;

namespace GeniusSudokuConverter
{
    public class ImagePathFinder : IValueConverter
    {
        private static string pathImages;
        static ImagePathFinder()
        {
            pathImages = Assembly.GetExecutingAssembly().Location;
            pathImages = System.IO.Path.GetDirectoryName(pathImages) + @"\images\";
        }

        #region IValueConverter Members

        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue ||value == null)
                return null;
            else
            {
                string aFileName = System.IO.Path.Combine(pathImages, value.ToString() + ".jpg");
                if (File.Exists(aFileName))
                    return aFileName;
                return null;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
