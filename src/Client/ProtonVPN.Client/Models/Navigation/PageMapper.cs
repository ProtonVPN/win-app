/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Common.UI.Gallery.Pages;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Countries.Pages;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Client.UI.Settings.Pages.Advanced;

namespace ProtonVPN.Client.Models.Navigation;

public class PageMapper : IPageMapper
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageMapper()
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
        Configure<CustomDnsServersViewModel, CustomDnsServersPage>();
        Configure<AutoConnectViewModel, AutoConnectPage>();
        Configure<CensorshipViewModel, CensorshipPage>();
        Configure<SettingsViewModel, SettingsPage>();

        ConfigureDebugPages();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageMapper.Configure?");
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
            string key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageMapper");
            }

            Type type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }

    private void Configure<V>()
        where V : Page
    {
        lock (_pages)
        {
            string key = typeof(V).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageMapper");
            }

            Type type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }

    [Conditional("DEBUG")]
    private void ConfigureDebugPages()
    {
        Configure<GalleryViewModel, GalleryPage>();
        Configure<GalleryItemViewModel, GalleryItemPage>();
    }
}