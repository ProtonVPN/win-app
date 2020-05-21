using System;
using System.Globalization;
using System.Windows.Data;
using ProtonVPN.Resources;

namespace ProtonVPN.Core.MVVM.Converters
{
    public class PluralConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string)value;
            double.TryParse(parameter.ToString(), out var number);

            return StringResources.GetPluralFormat(str, (decimal) number);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
