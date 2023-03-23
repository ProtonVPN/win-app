using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings;

namespace ProtonVPN.Gui.Views.Pages.Settings;

public sealed partial class SplitTunnelingPage : Page
{
    public SplitTunnelingViewModel ViewModel
    {
        get;
    }

    public SplitTunnelingPage()
    {
        ViewModel = App.GetService<SplitTunnelingViewModel>();
        InitializeComponent();
    }
}
