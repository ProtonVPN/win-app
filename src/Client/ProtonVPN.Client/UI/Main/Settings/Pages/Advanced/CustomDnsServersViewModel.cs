/*
 * Copyright (c) 2025 Proton AG
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

using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Models;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Client.UI.Overlays.Selection.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;

public partial class CustomDnsServersViewModel : SettingsPageViewModelBase
{
    private readonly IIpSelector _ipSelector;

    private bool _wasIpv6WarningDisplayed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasIpv6DnsServersWhileIpv6Disabled))]
    private bool _isIpv6Enabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasIpv6DnsServersWhileIpv6Disabled))]
    private bool _isCustomDnsServersEnabled;

    public override string Title => Localizer.Get("Settings_Connection_Advanced_CustomDnsServers");

    [property: SettingName(nameof(ISettings.CustomDnsServersList))]
    public SmartObservableCollection<SelectableNetworkAddress> DnsServers { get; } = [];

    public IEnumerable<NetworkAddress> SelectedDnsServers
        => DnsServers.Where(ip => ip.IsSelected).Select(ip => ip.Value);

    public bool HasSelectedDnsServers => SelectedDnsServers.Any();

    public string DnsServersHeader => Localizer.GetFormat("Settings_Connection_Advanced_CustomDnsServers_FormattedHeader", SelectedDnsServers.Count());

    public bool HasIpv6DnsServersWhileIpv6Disabled
        => !IsIpv6Enabled
        && IsCustomDnsServersEnabled
        && SelectedDnsServers.Any(ip => ip.IsIpV6);

    public CustomDnsServersViewModel(
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper,
        IIpSelector ipSelector)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _ipSelector = ipSelector;

        DnsServers.CollectionChanged += OnDnsServersCollectionChanged;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.IsCustomDnsServersEnabled, () => IsCustomDnsServersEnabled),
            ChangedSettingArgs.Create(() => Settings.CustomDnsServersList, () => GetSettingsCustomDnsServers()),
            ChangedSettingArgs.Create(() => Settings.IsIpv6Enabled, () => IsIpv6Enabled)
        ];
    }

    [RelayCommand]
    public async Task TriggerIpv6DisabledWarningAsync()
    {
        if (await ShowIpv6DisabledWarningAsync())
        {
            IsIpv6Enabled = true;
        }

        // Show this warning only once per app launch
        _wasIpv6WarningDisplayed = true;
    }

    protected override void OnRetrieveSettings()
    {
        IsIpv6Enabled = Settings.IsIpv6Enabled;
        IsCustomDnsServersEnabled = Settings.IsCustomDnsServersEnabled;

        DnsServers.Reset(GetObservableCustomDnsServers());
    }

    protected override bool IsReconnectionRequiredDueToChanges(IEnumerable<ChangedSettingArgs> changedSettings)
    {
        bool isReconnectionRequired = base.IsReconnectionRequiredDueToChanges(changedSettings);
        if (isReconnectionRequired)
        {
            // Check if there was any active DNS servers from the settings
            // then check if there is any active DNS servers now.
            // If there is none in both case, no need to reconnect.
            bool hadAnyActiveDnsServers = Settings.IsCustomDnsServersEnabled
                                       && Settings.CustomDnsServersList.Any(s => s.IsActive);
            bool hasAnyActiveDnsServers = IsCustomDnsServersEnabled
                                       && HasSelectedDnsServers;
            if (!hadAnyActiveDnsServers && !hasAnyActiveDnsServers)
            {
                return false;
            }
        }

        return isReconnectionRequired;
    }

    [RelayCommand]
    private async Task SelectCustomDnsServersAsync()
    {
        _ipSelector.Title = Localizer.Get("Settings_Connection_Advanced_CustomDnsServers_Header");
        _ipSelector.Description = Localizer.Get("Settings_Connection_Advanced_CustomDnsServers_Footer");
        _ipSelector.Caption = Localizer.Get("Settings_Connection_Advanced_CustomDnsServers_AddNew");
        _ipSelector.CanReorder = true;
        _ipSelector.IsAddressRangeAuthorized = false;

        List<SelectableNetworkAddress>? result = await _ipSelector.SelectAsync(DnsServers.Select(ip => ip.Clone()).ToList());
        if (result != null)
        {
            DnsServers.Reset(result);

            if (HasIpv6DnsServersWhileIpv6Disabled && !_wasIpv6WarningDisplayed)
            {
                await TriggerIpv6DisabledWarningAsync();
            }
        }
    }

    private List<CustomDnsServer> GetSettingsCustomDnsServers()
    {
        return DnsServers.Select(ip => new CustomDnsServer(ip.Value.ToString(), ip.IsSelected)).ToList();
    }

    private List<SelectableNetworkAddress> GetObservableCustomDnsServers()
    {
        List<SelectableNetworkAddress> addresses = [];

        foreach (CustomDnsServer ip in Settings.CustomDnsServersList)
        {
            if (NetworkAddress.TryParse(ip.IpAddress, out NetworkAddress address))
            {
                addresses.Add(new SelectableNetworkAddress(address, ip.IsActive));
            }
        }

        return addresses;
    }

    private void OnDnsServersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(DnsServersHeader));
        OnPropertyChanged(nameof(SelectedDnsServers));
        OnPropertyChanged(nameof(HasSelectedDnsServers));
        OnPropertyChanged(nameof(HasIpv6DnsServersWhileIpv6Disabled));
    }
}