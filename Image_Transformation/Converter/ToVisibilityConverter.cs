using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Image_Transformation
{
    public class ToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                if (isVisible)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }                
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                if (visibility == Visibility.Visible)
                {
                    return true;
                }
                else if (visibility == Visibility.Hidden)
                {
                    return false;
                }
            }
            return value;
        }
    }
}