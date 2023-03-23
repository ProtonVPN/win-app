using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings;

namespace ProtonVPN.Gui.Views.Pages.Settings;

public sealed partial class KillSwitchPage : Page
{
    public KillSwitchViewModel ViewModel
    {
        get;
    }

    public KillSwitchPage()
    {
        ViewModel = App.GetService<KillSwitchViewModel>();
        InitializeComponent();
    }
}
