using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.Models;
using ProtonVPN.Gui.ViewModels.Bases;

namespace ProtonVPN.Gui.ViewModels.Pages;

public partial class CountriesViewModel : PageViewModelBase
{
    public ObservableCollection<Country> Countries
    {
        get;
    }

    public CountriesViewModel(INavigationService navigationService)
        : base(navigationService, "Countries")
    {
        Countries = new ObservableCollection<Country>()
        {
            new Country("France"),
            new Country("Italy"),
            new Country("Lithuania"),
            new Country("Portugal"),
            new Country("Switzerland"),
            new Country("Spain"),
        };
    }

    [RelayCommand]
    public void NavigateToCountry(Country country)
    {
        var countryPageKey = "ProtonVPN.Gui.ViewModels.Pages.Countries.CountryViewModel";

        NavigationService.NavigateTo(countryPageKey, country);
    }
}
