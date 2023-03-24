using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ProtonVPN.Gui.Helpers;

public class SliderValueToMarginConverter : IValueConverter
{
    public double MinimumValue
    {
        get;
        set;
    }
    public double MaximumValue
    {
        get;
        set;
    }

    public double GripSize
    {
        get;
        set;
    } = 4.0;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var currentValue = System.Convert.ToDouble(value);
        var coercedValue = Math.Max(MinimumValue, Math.Min(MaximumValue, currentValue));

        return new Thickness(coercedValue - MaximumValue + GripSize, 0, MinimumValue - GripSize, 0);
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
