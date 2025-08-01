/*
 * Copyright (c) 2024 Proton AG
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
    [NotifyCanExecuteChangedFor(nameof(AddDnsServerCommand))]
    [NotifyPropertyChangedFor(nameof(IsCurrentAddressValid))]
    private string _currentIpAddress;

    [ObservableProperty]
    private bool _isCustomDnsServersEnabled;

    public override string Title => Localizer.Get("Settings_Connection_Advanced_CustomDnsServers");

    [property: SettingName(nameof(ISettings.CustomDnsServersList))]
    public ObservableCollection<DnsServerViewModel> CustomDnsServers { get; }

    public int ActiveCustomDnsServersCount => CustomDnsServers.Count(s => s.IsActive);

    public bool HasCustomDnsServers => CustomDnsServers.Count > 0;

    public bool HasActiveCustomDnsServers => ActiveCustomDnsServersCount > 0;

    public bool IsCurrentAddressValid => string.IsNullOrWhiteSpace(CurrentIpAddress)
                                      || CanAddDnsServer();

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

    [RelayCommand(CanExecute = nameof(CanAddDnsServer))]
    public void AddDnsServer()
    {
        if (!NetworkAddress.TryParse(CurrentIpAddress, out NetworkAddress address))
        {
            return;
        }

        DnsServerViewModel? dnsServer = CustomDnsServers.FirstOrDefault(s => s.IpAddress == address.FormattedAddress);
        if (dnsServer != null)
        {
            dnsServer.IsActive = true;
        }
        else
        {
            CustomDnsServers.Add(new(this, ViewModelHelper, address.FormattedAddress));
        }

        CurrentIpAddress = string.Empty;
    }

    public bool CanAddDnsServer()
    {
        return NetworkAddress.TryParse(CurrentIpAddress, out NetworkAddress address)
            && address.IsIpV4 
            && address.IsSingleIp;
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
            CustomDnsServers.Add(new(this, ViewModelHelper, server.IpAddress, server.IsActive));
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

    public void OnIpAddressKeyDownHandler(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && CanAddDnsServer())
        {
            AddDnsServerCommand.Execute(null);
        }
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
}