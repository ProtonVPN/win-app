using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings;

public class AutoConnectViewModel : PageViewModelBase
{
    public AutoConnectViewModel(INavigationService navigationService)
        : base(navigationService, "Auto connect", true)
    {
    }
}
