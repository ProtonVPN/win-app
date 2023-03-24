using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings;

public class PortForwardingViewModel : PageViewModelBase
{
    public PortForwardingViewModel(INavigationService navigationService)
        : base(navigationService, "Port forwarding", true)
    {
    }
}
