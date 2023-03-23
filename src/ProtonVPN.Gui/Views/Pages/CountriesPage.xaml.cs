using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages;

namespace ProtonVPN.Gui.Views.Pages;

public sealed partial class CountriesPage : Page
{
    public CountriesViewModel ViewModel
    {
        get;
    }

    public CountriesPage()
    {
        ViewModel = App.GetService<CountriesViewModel>();
        InitializeComponent();
    }
}
