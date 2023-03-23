using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings.AdvancedSettings;

namespace ProtonVPN.Gui.Views.Pages.Settings.AdvancedSettings;

public sealed partial class VpnLogsPage : Page
{
    public VpnLogsViewModel ViewModel
    {
        get;
    }

    public VpnLogsPage()
    {
        ViewModel = App.GetService<VpnLogsViewModel>();
        InitializeComponent();
    }
}
