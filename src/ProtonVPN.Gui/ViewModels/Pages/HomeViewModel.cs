using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages;

public class HomeViewModel : PageViewModelBase
{
    public HomeViewModel(INavigationService navigationService)
        : base(navigationService, "Home")
    {
    }
}
