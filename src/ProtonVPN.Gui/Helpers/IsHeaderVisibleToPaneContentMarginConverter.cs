using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ProtonVPN.Gui.Helpers;

public class IsHeaderVisibleToPaneContentMarginConverter : IValueConverter
{
    public Thickness DefaultPaneContentMargin
    {
        get;
        set;
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isHeaderVisible = System.Convert.ToBoolean(value);

        return isHeaderVisible ? DefaultPaneContentMargin : new Thickness(0);
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}