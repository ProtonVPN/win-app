using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings;

public class NetShieldViewModel : PageViewModelBase
{
    public NetShieldViewModel(INavigationService navigationService)
        : base(navigationService, "NetShield", true)
    {
    }
}
