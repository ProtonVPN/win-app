using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings;

public class SplitTunnelingViewModel : PageViewModelBase
{
    public SplitTunnelingViewModel(INavigationService navigationService)
        : base(navigationService, "Split tunneling", true)
    {
    }
}
