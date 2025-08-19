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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Services.Bootstrapping.Helpers;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Common.Core.Networking;
using Windows.System;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;

public partial class CustomDnsServersViewModel : SettingsPageViewModelBase
{
    [ObservableProperty]
    private string _currentIpAddress;

    [ObservableProperty]
    private string? _ipAddressError;

    [ObservableProperty]
    private bool _isCustomDnsServersEnabled;

    private bool _wasIpv6WarningDisplayed;

    public override string Title => Localizer.Get("Settings_Connection_Advanced_CustomDnsServers");

    [property: SettingName(nameof(ISettings.CustomDnsServersList))]
    public ObservableCollection<DnsServerViewModel> CustomDnsServers { get; }

    public int ActiveCustomDnsServersCount => CustomDnsServers.Count(s => s.IsActive);

    public bool HasCustomDnsServers => CustomDnsServers.Count > 0;

    public bool HasActiveCustomDnsServers => ActiveCustomDnsServersCount > 0;

    public bool IsDragDropEnabled { get; }

    public CustomDnsServersViewModel(
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _currentIpAddress = string.Empty;

        CustomDnsServers = new();
        CustomDnsServers.CollectionChanged += OnCustomDnsServersCollectionChanged;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.IsCustomDnsServersEnabled, () => IsCustomDnsServersEnabled),
            ChangedSettingArgs.Create(() => Settings.CustomDnsServersList, () => GetCustomDnsServersList())
        ];

        IsDragDropEnabled = !AppInstanceHelper.IsRunningAsAdmin();
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        ResetCurrentIpAddress();
        ResetIpAddressError();
    }

    [RelayCommand]
    public async Task AddDnsServerAsync()
    {
        NetworkAddress? address = GetValidatedCurrentIpAddress();
        string? error = GetIpAddressError(address);

        if (error != null || address == null)
        {
            IpAddressError = error ?? Localizer.Get("Settings_Common_IpAddresses_Invalid");
            return;
        }

        CustomDnsServers.Add(new(Settings, this, ViewModelHelper, address.Value.FormattedAddress));
        ResetCurrentIpAddress();

        if (address?.IsIpV6 == true && !Settings.IsIpv6Enabled && !_wasIpv6WarningDisplayed)
        {
            await ShowIpv6DisabledWarningAsync();

            // Show this warning only once per app launch
            _wasIpv6WarningDisplayed = true;
        }
    }

    [RelayCommand]
    public Task TriggerIpv6DisabledWarningAsync()
    {
        return ShowIpv6DisabledWarningAsync();
    }

    protected override void OnIpv6WarningClosedWithPrimaryAction()
    {
        List<DnsServerViewModel> customDnsServers = CustomDnsServers.ToList();
        CustomDnsServers.Clear();

        foreach (DnsServerViewModel ip in customDnsServers)
        {
            CustomDnsServers.Add(new(Settings, this, ViewModelHelper, ip.IpAddress, ip.IsActive));
        }
    }

    private string? GetIpAddressError(NetworkAddress? address)
    {
        if (address == null || !address.Value.IsSingleIp)
        {
            return Localizer.Get("Settings_Common_IpAddresses_Invalid");
        }

        if (CustomDnsServers.FirstOrDefault(ip => ip.IpAddress == address.Value.FormattedAddress) != null)
        {
            return Localizer.Get("Settings_Common_IpAddresses_AlreadyExists");
        }

        return null;
    }

    private NetworkAddress? GetValidatedCurrentIpAddress()
    {
        return NetworkAddress.TryParse(CurrentIpAddress, out NetworkAddress address) ? address : null;
    }

    public void RemoveDnsServer(DnsServerViewModel server)
    {
        CustomDnsServers.Remove(server);
    }

    public void MoveDnsServerUp(DnsServerViewModel server)
    {
        int currentIndex = CustomDnsServers.IndexOf(server);
        if (currentIndex > 0)
        {
            CustomDnsServers.Move(currentIndex, currentIndex - 1);
        }
    }

    public void MoveDnsServerDown(DnsServerViewModel server)
    {
        int currentIndex = CustomDnsServers.IndexOf(server);
        if (currentIndex >= 0 && currentIndex < CustomDnsServers.Count - 1)
        {
            CustomDnsServers.Move(currentIndex, currentIndex + 1);
        }
    }

    public bool CanMoveDnsServerUp(DnsServerViewModel dnsServerViewModel)
    {
        int currentIndex = CustomDnsServers.IndexOf(dnsServerViewModel);
        return currentIndex > 0;
    }

    public bool CanMoveDnsServerDown(DnsServerViewModel dnsServerViewModel)
    {
        int currentIndex = CustomDnsServers.IndexOf(dnsServerViewModel);
        return currentIndex >= 0 && currentIndex < CustomDnsServers.Count - 1;
    }

    public void InvalidateCustomDnsServersCount()
    {
        OnPropertyChanged(nameof(ActiveCustomDnsServersCount));
    }

    public void OnIpAddressKeyDownHandler(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            AddDnsServerCommand.Execute(null);
        }
    }

    private void InvalidateAllMoveCommands()
    {
        foreach (DnsServerViewModel server in CustomDnsServers)
        {
            server.InvalidateCommands();
        }
    }

    protected override void OnRetrieveSettings()
    {
        IsCustomDnsServersEnabled = Settings.IsCustomDnsServersEnabled;

        CustomDnsServers.Clear();
        foreach (CustomDnsServer server in Settings.CustomDnsServersList)
        {
            CustomDnsServers.Add(new(Settings, this, ViewModelHelper, server.IpAddress, server.IsActive));
        }
    }

    private void OnCustomDnsServersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasCustomDnsServers));

        InvalidateCustomDnsServersCount();
        InvalidateAllMoveCommands();
    }

    private List<CustomDnsServer> GetCustomDnsServersList()
    {
        return CustomDnsServers.Select(s => new CustomDnsServer(s.IpAddress, s.IsActive)).ToList();
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
                                       && HasActiveCustomDnsServers;
            if (!hadAnyActiveDnsServers && !hasAnyActiveDnsServers)
            {
                return false;
            }
        }

        return isReconnectionRequired;
    }

    partial void OnCurrentIpAddressChanged(string value)
    {
        ResetIpAddressError();
    }

    private void ResetIpAddressError()
    {
        IpAddressError = null;
    }

    private void ResetCurrentIpAddress()
    {
        CurrentIpAddress = string.Empty;
    }
}