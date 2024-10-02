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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Navigation;

namespace ProtonVPN.Client.UI.Main.Home.Details.Location;

public partial class LocationDetailsPageViewModel : PageViewModelBase<IDetailsViewNavigator>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChanged>
{
    private readonly ISettings _settings;

    [ObservableProperty]
    private string _country;

    [ObservableProperty]
    private string _ipAddress;

    [ObservableProperty]
    private string _isp;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProtectionLabel))]
    [NotifyPropertyChangedFor(nameof(ProtectionSubLabel))]
    private bool _isConnecting;

    [ObservableProperty]
    private bool _isDisconnected;

    public string ProtectionLabel => Localizer.Get(IsConnecting
        ? "Home_ConnectionDetails_Connecting"
        : "Home_ConnectionDetails_Unprotected");

    public string ProtectionSubLabel => Localizer.Get(IsConnecting
        ? "Home_ConnectionDetails_ConnectingSubLabel"
        : "Home_ConnectionDetails_UnprotectedSubLabel");

    public LocationDetailsPageViewModel(
        ISettings settings,
        IDetailsViewNavigator viewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizer, logger, issueReporter)
    {
        _settings = settings;

        InvalidateLocation();
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (message.PropertyName == nameof(ISettings.DeviceLocation))
            {
                InvalidateLocation();
            }
        });
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        InvalidateLocation();
    }

    private void InvalidateLocation()
    {
        IpAddress = _settings.DeviceLocation?.IpAddress ?? string.Empty;
        Country = Localizer.GetCountryName(_settings.DeviceLocation?.CountryCode) ?? string.Empty;
        Isp = _settings.DeviceLocation?.Isp ?? string.Empty;
    }

    public void Receive(ConnectionStatusChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            IsConnected = message.ConnectionStatus == ConnectionStatus.Connected;
            IsConnecting = message.ConnectionStatus == ConnectionStatus.Connecting;
            IsDisconnected = message.ConnectionStatus == ConnectionStatus.Disconnected;
        });
    }
}