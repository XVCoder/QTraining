using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QTraining.Converter
{
    /// <summary>
    /// 空值转换
    /// </summary>
    public class NullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
