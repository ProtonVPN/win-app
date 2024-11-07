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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Contracts.Messages;

namespace ProtonVPN.Client.UI.Main.Map;

public partial class MapComponentViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;

    public bool IsConnecting => _connectionManager.IsConnecting;

    public bool IsConnected => _connectionManager.IsConnected;

    [ObservableProperty]
    private string _activeCountryCode = string.Empty;

    [ObservableProperty]
    private bool _isMainWindowVisible;

    public MapComponentViewModel(
        ISettings settings,
        IConnectionManager connectionManager,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizer, logger, issueReporter)
    {
        _settings = settings;
        _connectionManager = connectionManager;

        InvalidateActiveCountryCode();
    }

    private void InvalidateActiveCountryCode()
    {
        switch (_connectionManager.ConnectionStatus)
        {
            case ConnectionStatus.Connected:
            {
                string? countryCode = _connectionManager.CurrentConnectionDetails?.ExitCountryCode;
                if (!string.IsNullOrEmpty(countryCode))
                {
                    ActiveCountryCode = countryCode;
                }

                break;
            }

            case ConnectionStatus.Connecting:
            case ConnectionStatus.Disconnected:
                ActiveCountryCode = _settings.DeviceLocation?.CountryCode ?? string.Empty;
                break;
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(IsConnected));

            InvalidateActiveCountryCode();
        });
    }


    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (message.PropertyName == nameof(ISettings.DeviceLocation))
            {
                InvalidateActiveCountryCode();
            }
        });
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        ExecuteOnUIThread(() => IsMainWindowVisible = message.IsMainWindowVisible);
    }
}