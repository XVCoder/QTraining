using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QTraining.Converter
{
    /// <summary>
    /// visibility转换器（反向，如collapsed对应true）
    /// </summary>
    public class VisibilityReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility valueVisibility)
            {
                if (targetType == typeof(bool))
                    return valueVisibility == Visibility.Visible;
                if (targetType == typeof(Visibility))
                    return valueVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueBool)
            {
                return valueBool ? Visibility.Collapsed : Visibility.Visible;
            }
            return value;
        }
    }
}
