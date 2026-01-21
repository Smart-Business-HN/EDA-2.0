using Microsoft.UI.Xaml.Data;
using System;

namespace EDA_2._0.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal decimalValue)
            {
                return $"L. {decimalValue:N2}";
            }
            if (value is double doubleValue)
            {
                return $"L. {doubleValue:N2}";
            }
            if (value is int intValue)
            {
                return $"L. {intValue:N2}";
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
