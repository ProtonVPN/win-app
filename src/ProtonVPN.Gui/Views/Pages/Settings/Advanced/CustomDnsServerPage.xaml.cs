using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings.Advanced;

namespace ProtonVPN.Gui.Views.Pages.Settings.Advanced;

public sealed partial class CustomDnsServersPage : Page
{
    public CustomDnsServersViewModel ViewModel
    {
        get;
    }

    public CustomDnsServersPage()
    {
        ViewModel = App.GetService<CustomDnsServersViewModel>();
        InitializeComponent();
    }
}
