using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings;

namespace ProtonVPN.Gui.Views.Pages.Settings;

public sealed partial class ProtocolPage : Page
{
    public ProtocolViewModel ViewModel
    {
        get;
    }

    public ProtocolPage()
    {
        ViewModel = App.GetService<ProtocolViewModel>();
        InitializeComponent();
    }
}
