using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.Contracts.ViewModels;
using ProtonVPN.Gui.Models;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages.Countries;

public partial class CountryViewModel : PageViewModelBase, INavigationAware
{
    [ObservableProperty]
    private Country? _currentCountry;

    public CountryViewModel(INavigationService navigationService)
        : base(navigationService, "Country", true)
    {
    }

    public void OnNavigatedFrom()
    {
    }
    public void OnNavigatedTo(object parameter)
    {
        CurrentCountry = parameter as Country;
        if(CurrentCountry != null)
        {
            Title = CurrentCountry.CountryName;
        }
    }
}
