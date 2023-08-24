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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Models;

namespace ProtonVPN.Client.UI.Settings.Pages.Advanced;

public partial class CustomDnsServersViewModel : PageViewModelBase<IMainViewNavigator>
{
    private readonly ISettings _settings;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddDnsServerCommand))]
    private string _currentIpAddress;

    [ObservableProperty]
    private bool _isCustomDnsServersEnabled;

    public override string? Title => Localizer.Get("Settings_Connection_Advanced_CustomDnsServers");

    public ObservableCollection<DnsServerViewModel> CustomDnsServers { get; }

    public bool HasCustomDnsServers => CustomDnsServers.Any();

    public int ActiveCustomDnsServersCount => CustomDnsServers.Count(s => s.IsActive);

    public CustomDnsServersViewModel(IMainViewNavigator viewNavigator, ILocalizationProvider localizationProvider, ISettings settings)
        : base(viewNavigator, localizationProvider)
    {
        _settings = settings;
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

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        _settings.IsCustomDnsServersEnabled = IsCustomDnsServersEnabled;
        _settings.CustomDnsServersList = CustomDnsServers.Select(s => new CustomDnsServer(s.IpAddress, s.IsActive)).ToList();
    }

    public override void OnNavigatedTo(object parameter)
    {
        base.OnNavigatedTo(parameter);

        IsCustomDnsServersEnabled = _settings.IsCustomDnsServersEnabled;

        CustomDnsServers.Clear();
        foreach (CustomDnsServer server in _settings.CustomDnsServersList)
        {
            CustomDnsServers.Add(new(Localizer, this, server.IpAddress, server.IsActive));
        }
    }

    private void OnCustomDnsServersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasCustomDnsServers));

        InvalidateCustomDnsServersCount();
    }
}