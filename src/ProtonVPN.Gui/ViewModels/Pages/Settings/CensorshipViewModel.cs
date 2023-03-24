using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings;

public class CensorshipViewModel : PageViewModelBase
{
    public CensorshipViewModel(INavigationService navigationService)
        : base(navigationService, "Help us fight censorship", true)
    {
    }
}
