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
    /// Object反转转换器（如：false→true；visible→collapse）
    /// </summary>
    public class ObjectReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool valueBool && targetType == typeof(bool))
            {
                return !valueBool;
            }
            if (value is Visibility valueVisibility && targetType == typeof(Visibility))
            {
                return valueVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
