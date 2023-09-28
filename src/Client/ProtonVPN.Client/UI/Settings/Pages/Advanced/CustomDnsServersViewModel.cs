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
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.UI.Settings.Pages.Advanced;

public partial class CustomDnsServersViewModel : ConnectionSettingsPageViewModelBase
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
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager)
        : base(viewNavigator, localizationProvider, settings, settingsConflictResolver, connectionManager)
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
            CustomDnsServers.Add(new(Localizer, this, CurrentIpAddress));
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

    protected override bool HasConfigurationChanged()
    {
        return Settings.IsCustomDnsServersEnabled != IsCustomDnsServersEnabled
            || !Settings.CustomDnsServersList.SequenceEqual(GetCustomDnsServersList());
    }

    protected override void SaveSettings()
    {
        Settings.IsCustomDnsServersEnabled = IsCustomDnsServersEnabled;
        Settings.CustomDnsServersList = GetCustomDnsServersList();
    }

    protected override void RetrieveSettings()
    {
        IsCustomDnsServersEnabled = Settings.IsCustomDnsServersEnabled;

        CustomDnsServers.Clear();
        foreach (CustomDnsServer server in Settings.CustomDnsServersList)
        {
            CustomDnsServers.Add(new(Localizer, this, server.IpAddress, server.IsActive));
        }
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
}