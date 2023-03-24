using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages;

public partial class HomeViewModel : PageViewModelBase
{
    [ObservableProperty]
    private bool _isDetailsPaneOpen;

    public HomeViewModel(INavigationService navigationService)
        : base(navigationService)
    {
    }

    [RelayCommand]
    public void CloseDetailsPane()
    {
        IsDetailsPaneOpen = false;
    }

    [RelayCommand]
    public void OpenDetailsPane()
    {
        IsDetailsPaneOpen = true;
    }
}