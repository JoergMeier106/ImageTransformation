using System;
using System.Globalization;
using System.Windows.Data;

namespace Image_Transformation
{
    public class ToNegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number && number > 0)
            {
                return ToggleNumber(number);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number && number < 0)
            {
                return ToggleNumber(number);
            }
            return value;
        }

        private double ToggleNumber(double number)
        {
            return number * -1;
        }
    }
}