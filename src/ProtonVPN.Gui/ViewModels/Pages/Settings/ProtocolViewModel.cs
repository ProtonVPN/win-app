using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings;

public class ProtocolViewModel : PageViewModelBase
{
    public ProtocolViewModel(INavigationService navigationService)
        : base(navigationService, "Protocol", true)
    {
    }
}
