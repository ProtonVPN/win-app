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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Geographical;

namespace ProtonVPN.Client.UI.Main.Home.Details.Location;

public partial class LocationDetailsPageViewModel : PageViewModelBase<IDetailsViewNavigator>,
    IEventMessageReceiver<DeviceLocationChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Country))]
    [NotifyPropertyChangedFor(nameof(IpAddress))]
    [NotifyPropertyChangedFor(nameof(Isp))]
    private DeviceLocation _deviceLocation = DeviceLocation.Unknown;

    public string Country => EmptyValueExtensions.TransformValueOrDefault(DeviceLocation.CountryCode, cc => Localizer.GetCountryName(cc));
    public string IpAddress => EmptyValueExtensions.GetValueOrDefault(DeviceLocation.IpAddress);
    public string Isp => EmptyValueExtensions.GetValueOrDefault(DeviceLocation.Isp);

    public bool IsDisconnected => _connectionManager.IsDisconnected;
    public bool IsConnecting => _connectionManager.IsConnecting;

    public LocationDetailsPageViewModel(
        ISettings settings,
        IDetailsViewNavigator viewNavigator,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(viewNavigator, viewModelHelper)
    {
        _settings = settings;
        _connectionManager = connectionManager;
    }

    public void Receive(DeviceLocationChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateLocation);
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateConnectionStatus);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateLocation();
        InvalidateConnectionStatus();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Country));
    }

    private void InvalidateLocation()
    {
        DeviceLocation = _settings.DeviceLocation ?? DeviceLocation.Unknown;
    }

    private void InvalidateConnectionStatus()
    {
        OnPropertyChanged(nameof(IsDisconnected));
        OnPropertyChanged(nameof(IsConnecting));
    }
}