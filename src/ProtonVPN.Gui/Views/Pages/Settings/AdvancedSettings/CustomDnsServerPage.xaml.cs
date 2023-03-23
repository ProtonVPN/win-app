using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings.AdvancedSettings;

namespace ProtonVPN.Gui.Views.Pages.Settings.AdvancedSettings;

public sealed partial class CustomDnsServerPage : Page
{
    public CustomDnsServerViewModel ViewModel
    {
        get;
    }

    public CustomDnsServerPage()
    {
        ViewModel = App.GetService<CustomDnsServerViewModel>();
        InitializeComponent();
    }
}
