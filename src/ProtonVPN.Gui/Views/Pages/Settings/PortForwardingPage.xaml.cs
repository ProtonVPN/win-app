using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings;

namespace ProtonVPN.Gui.Views.Pages.Settings;

public sealed partial class PortForwardingPage : Page
{
    public PortForwardingViewModel ViewModel
    {
        get;
    }

    public PortForwardingPage()
    {
        ViewModel = App.GetService<PortForwardingViewModel>();
        InitializeComponent();
    }
}
