using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings;

namespace ProtonVPN.Gui.Views.Pages.Settings;

public sealed partial class NetShieldPage : Page
{
    public NetShieldViewModel ViewModel
    {
        get;
    }

    public NetShieldPage()
    {
        ViewModel = App.GetService<NetShieldViewModel>();
        InitializeComponent();
    }
}
