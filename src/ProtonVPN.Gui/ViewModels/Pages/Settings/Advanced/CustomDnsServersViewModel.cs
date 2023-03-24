using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Settings.Advanced;

public class CustomDnsServersViewModel : PageViewModelBase
{
    public CustomDnsServersViewModel(INavigationService navigationService)
        : base(navigationService, "Custom DNS servers", true)
    {
    }
}
