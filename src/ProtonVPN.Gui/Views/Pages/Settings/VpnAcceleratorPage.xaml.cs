using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings;

namespace ProtonVPN.Gui.Views.Pages.Settings;

public sealed partial class VpnAcceleratorPage : Page
{
    public VpnAcceleratorViewModel ViewModel
    {
        get;
    }

    public VpnAcceleratorPage()
    {
        ViewModel = App.GetService<VpnAcceleratorViewModel>();
        InitializeComponent();
    }
}
