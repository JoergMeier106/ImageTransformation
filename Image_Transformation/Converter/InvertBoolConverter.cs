using System;
using System.Globalization;
using System.Windows.Data;

namespace Image_Transformation
{
    /// <summary>
    /// Toggles a boolean in a view property.
    /// </summary>
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return ToogleBool(boolValue);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return ToogleBool(boolValue);
            }
            return value;
        }

        private bool ToogleBool(bool value)
        {
            return !value;
        }
    }
}