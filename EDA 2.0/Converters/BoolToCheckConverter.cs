using Microsoft.UI.Xaml.Data;
using System;

namespace EDA_2._0.Converters
{
    public class BoolToCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isActive)
            {
                return isActive ? "\uE73E" : "\uE711"; // Checkmark or X
            }
            return "\uE711";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
