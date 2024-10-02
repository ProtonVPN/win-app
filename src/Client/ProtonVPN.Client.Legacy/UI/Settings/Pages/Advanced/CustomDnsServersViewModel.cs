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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Legacy.Models.Activation;
using ProtonVPN.Client.Legacy.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.Legacy.UI.Settings.Pages.Entities;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using Windows.System;

namespace ProtonVPN.Client.Legacy.UI.Settings.Pages.Advanced;

public partial class CustomDnsServersViewModel : SettingsPageViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddDnsServerCommand))]
    private string _currentIpAddress;

    [ObservableProperty]
    private bool _isCustomDnsServersEnabled;

    public override string? Title => Localizer.Get("Settings_Connection_Advanced_CustomDnsServers");

    [property: SettingName(nameof(ISettings.CustomDnsServersList))]
    public ObservableCollection<DnsServerViewModel> CustomDnsServers { get; }

    public bool HasCustomDnsServers => CustomDnsServers.Any();

    public int ActiveCustomDnsServersCount => CustomDnsServers.Count(s => s.IsActive);

    public CustomDnsServersViewModel(
        IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        IOverlayActivator overlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, 
               localizationProvider, 
               overlayActivator, 
               settings, 
               settingsConflictResolver, 
               connectionManager,
               logger,
               issueReporter)
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