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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using Windows.System;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;

public partial class CustomDnsServersViewModel : SettingsPageViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddDnsServerCommand))]
    private string _currentIpAddress;

    [ObservableProperty]
    private bool _isCustomDnsServersEnabled;

    public override string Title => Localizer.Get("Settings_Connection_Advanced_CustomDnsServers");

    [property: SettingName(nameof(ISettings.CustomDnsServersList))]
    public ObservableCollection<DnsServerViewModel> CustomDnsServers { get; }

    public bool HasCustomDnsServers => CustomDnsServers.Any();

    public int ActiveCustomDnsServersCount => CustomDnsServers.Count(s => s.IsActive);

    public CustomDnsServersViewModel(
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               localizer,
               logger,
               issueReporter,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager)
    {
        _currentIpAddress = string.Empty;

        CustomDnsServers = new();
        CustomDnsServers.CollectionChanged += OnCustomDnsServersCollectionChanged;
    }

    [RelayCommand(CanExecute = nameof(CanAddDnsServer))]
    public void AddDnsServer()
    {
        DnsServerViewModel? dnsServer = CustomDnsServers.FirstOrDefault(s => s.IpAddress == CurrentIpAddress);
        if (dnsServer != null)
        {
            dnsServer.IsActive = true;
        }
        else
        {
            CustomDnsServers.Add(new(Localizer, Logger, IssueReporter, this, CurrentIpAddress));
        }

        CurrentIpAddress = string.Empty;
    }

    public bool CanAddDnsServer()
    {
        return !string.IsNullOrEmpty(CurrentIpAddress)
            && CurrentIpAddress.IsValidIpAddress();
    }

    [RelayCommand]
    public void RemoveDnsServer(DnsServerViewModel server)
    {
        CustomDnsServers.Remove(server);
    }

    public void InvalidateCustomDnsServersCount()
    {
        OnPropertyChanged(nameof(ActiveCustomDnsServersCount));
    }

    protected override void OnSaveSettings()
    {
        Settings.IsCustomDnsServersEnabled = IsCustomDnsServersEnabled;
        Settings.CustomDnsServersList = GetCustomDnsServersList();
    }

    protected override void OnRetrieveSettings()
    {
        IsCustomDnsServersEnabled = Settings.IsCustomDnsServersEnabled;

        CustomDnsServers.Clear();
        foreach (CustomDnsServer server in Settings.CustomDnsServersList)
        {
            CustomDnsServers.Add(new(Localizer, Logger, IssueReporter, this, server.IpAddress, server.IsActive));
        }
    }

    protected override IEnumerable<ChangedSettingArgs> GetSettings()
    {
        yield return new(nameof(ISettings.IsCustomDnsServersEnabled), IsCustomDnsServersEnabled,
            Settings.IsCustomDnsServersEnabled != IsCustomDnsServersEnabled);

        yield return new(nameof(ISettings.CustomDnsServersList), GetCustomDnsServersList(),
            !Settings.CustomDnsServersList.SequenceEqual(GetCustomDnsServersList()));
    }

    private void OnCustomDnsServersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasCustomDnsServers));

        InvalidateCustomDnsServersCount();
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
}