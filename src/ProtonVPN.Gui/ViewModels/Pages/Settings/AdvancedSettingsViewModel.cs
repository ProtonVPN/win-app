using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings;

public class AdvancedSettingsViewModel : PageViewModelBase
{
    public AdvancedSettingsViewModel(INavigationService navigationService)
        : base(navigationService, "Advanced settings", true)
    {
    }
}
