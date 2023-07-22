using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QTraining.Converter
{
    /// <summary>
    /// bool转换器（反向，如true对应collapsed）
    /// </summary>
    public class BoolReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueBool && targetType == typeof(Visibility))
            {
                return valueBool ? Visibility.Collapsed : Visibility.Visible;
            }
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility valueVisibility)
            {
                return valueVisibility != Visibility.Visible;
            }
            return !(bool)value;
        }
    }
}
