using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings;

public class VpnAcceleratorViewModel : PageViewModelBase
{
    public VpnAcceleratorViewModel(INavigationService navigationService)
        : base(navigationService, "VPN Accelerator", true)
    {
    }
}
