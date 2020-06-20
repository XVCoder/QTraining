using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace QTraining.Converter
{
    /// <summary>
    /// visibility转换器（正向，如visible对应true）
    /// </summary>
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility valueVisibility && targetType == typeof(bool))
            {
                return valueVisibility == Visibility.Visible;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueBool)
            {
                return valueBool ? Visibility.Visible : Visibility.Collapsed;
            }
            return value;
        }
    }
}
