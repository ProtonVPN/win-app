using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings;

public class KillSwitchViewModel : PageViewModelBase
{
    public KillSwitchViewModel(INavigationService navigationService)
        : base(navigationService, "Kill switch", true)
    {
    }
}
