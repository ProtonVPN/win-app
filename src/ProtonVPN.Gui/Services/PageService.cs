using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;

using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.ViewModels.Pages;
using ProtonVPN.Gui.ViewModels.Pages.Countries;
using ProtonVPN.Gui.ViewModels.Pages.Settings;
using ProtonVPN.Gui.ViewModels.Pages.Settings.AdvancedSettings;
using ProtonVPN.Gui.Views.Pages;
using ProtonVPN.Gui.Views.Pages.Countries;
using ProtonVPN.Gui.Views.Pages.Settings;
using ProtonVPN.Gui.Views.Pages.Settings.AdvancedSettings;

namespace ProtonVPN.Gui.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<HomeViewModel, HomePage>();
        Configure<CountriesViewModel, CountriesPage>();
        Configure<CountryViewModel, CountryPage>();
        Configure<NetShieldViewModel, NetShieldPage>();
        Configure<KillSwitchViewModel, KillSwitchPage>();
        Configure<PortForwardingViewModel, PortForwardingPage>();
        Configure<SplitTunnelingViewModel, SplitTunnelingPage>();
        Configure<ProtocolViewModel, ProtocolPage>();
        Configure<VpnAcceleratorViewModel, VpnAcceleratorPage>();
        Configure<AdvancedSettingsViewModel, AdvancedSettingsPage>();
        Configure<VpnLogsViewModel, VpnLogsPage>();
        Configure<CustomDnsServerViewModel, CustomDnsServerPage>();
        Configure<AutoConnectViewModel, AutoConnectPage>();
        Configure<CensorshipViewModel, CensorshipPage>();
        Configure<SettingsViewModel, SettingsPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
