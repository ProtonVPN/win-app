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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Legacy.UI.Home.Status;

public partial class VpnStatusViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<DeviceLocationChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    [NotifyPropertyChangedFor(nameof(IsConnecting))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(IsNetShieldStatVisible))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(IsTitleVisible))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    private ConnectionStatus _currentConnectionStatus;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLocationUnknown))]
    [NotifyPropertyChangedFor(nameof(Country))]
    private string? _countryCode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLocationUnknown))]
    private string? _ipAddress;

    [ObservableProperty]
    private bool _isSecureCoreConnection;

    public string Country => Localizer.GetCountryName(CountryCode);

    public bool IsConnected => CurrentConnectionStatus == ConnectionStatus.Connected;

    public bool IsConnecting => CurrentConnectionStatus == ConnectionStatus.Connecting;

    public bool IsDisconnected => CurrentConnectionStatus == ConnectionStatus.Disconnected;

    public bool IsNetShieldStatVisible => IsConnected && _settings.IsNetShieldEnabled;

    public bool IsTitleVisible => !string.IsNullOrEmpty(Title);

    public bool IsLocationUnknown => string.IsNullOrEmpty(CountryCode) && string.IsNullOrEmpty(IpAddress);

    public string Subtitle =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected or
            ConnectionStatus.Connecting => Localizer.Get("Home_VpnStatus_Subtitle_Connecting"),
            ConnectionStatus.Disconnected => Localizer.Get("Home_VpnStatus_Subtitle_Disconnected"),
            _ => string.Empty
        };

    public string Title =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => Localizer.Get("Home_VpnStatus_Title_Connected"),
            ConnectionStatus.Connecting or
            ConnectionStatus.Disconnected => string.Empty,
            _ => string.Empty
        };

    public VpnStatusViewModel(
        IConnectionManager connectionManager,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider, logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _settings = settings;

        InvalidateCurrentConnectionStatus();
        InvalidateDeviceLocation();
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(InvalidateCurrentConnectionStatus);
    }

    public void Receive(DeviceLocationChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateDeviceLocation);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsNetShieldEnabled))
        {
            OnPropertyChanged(nameof(IsNetShieldStatVisible));
        }
    }

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(Country));
    }

    private void InvalidateCurrentConnectionStatus()
    {
        CurrentConnectionStatus = _connectionManager.ConnectionStatus;

        IsSecureCoreConnection = _connectionManager.IsConnected
                              && (_connectionManager.CurrentConnectionDetails?.IsSecureCore ?? false);
    }

    private void InvalidateDeviceLocation()
    {
        DeviceLocation? currentLocation = _settings.DeviceLocation;

        IpAddress = currentLocation?.IpAddress;
        CountryCode = currentLocation?.CountryCode;
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsNetShieldStatVisible)));
    }
}