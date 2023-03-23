using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Countries;

namespace ProtonVPN.Gui.Views.Pages.Countries;

public sealed partial class CountryPage : Page
{
    public CountryViewModel ViewModel
    {
        get;
    }

    public CountryPage()
    {
        ViewModel = App.GetService<CountryViewModel>();
        InitializeComponent();
    }
}
