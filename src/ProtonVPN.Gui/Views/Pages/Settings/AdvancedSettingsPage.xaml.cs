using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings;

namespace ProtonVPN.Gui.Views.Pages.Settings;

public sealed partial class AdvancedSettingsPage : Page
{
    public AdvancedSettingsViewModel ViewModel
    {
        get;
    }

    public AdvancedSettingsPage()
    {
        ViewModel = App.GetService<AdvancedSettingsViewModel>();
        InitializeComponent();
    }
}
