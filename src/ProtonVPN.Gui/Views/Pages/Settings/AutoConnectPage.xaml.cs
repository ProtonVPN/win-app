using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings;

namespace ProtonVPN.Gui.Views.Pages.Settings;

public sealed partial class AutoConnectPage : Page
{
    public AutoConnectViewModel ViewModel
    {
        get;
    }

    public AutoConnectPage()
    {
        ViewModel = App.GetService<AutoConnectViewModel>();
        InitializeComponent();
    }
}
