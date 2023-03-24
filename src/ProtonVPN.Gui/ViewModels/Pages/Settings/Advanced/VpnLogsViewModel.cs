using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings.Advanced;

public class VpnLogsViewModel : PageViewModelBase
{
    public VpnLogsViewModel(INavigationService navigationService)
        : base(navigationService, "VPN logs", true)
    {
    }
}
