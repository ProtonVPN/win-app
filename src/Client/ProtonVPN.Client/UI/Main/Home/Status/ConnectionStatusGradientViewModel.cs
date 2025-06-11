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

using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.UI.Main.Home.Status;

public class ConnectionStatusGradientViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;

    public bool IsConnected => _connectionManager.IsConnected;

    public bool IsConnecting => _connectionManager.IsConnecting;

    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public bool IsNotConnectedAndAdvancedKillSwitchActive => !IsConnected && _settings.IsAdvancedKillSwitchActive();

    public ConnectionStatusGradientViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        IConnectionManager connectionManager)
        : base(viewModelHelper)
    {
        _settings = settings;
        _connectionManager = connectionManager;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateConnectionStatus);
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName is nameof(ISettings.KillSwitchMode) or nameof(ISettings.IsKillSwitchEnabled))
        {
            ExecuteOnUIThread(InvalidateConnectionStatus);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateConnectionStatus();
    }

    private void InvalidateConnectionStatus()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsConnecting));
        OnPropertyChanged(nameof(IsDisconnected));
        OnPropertyChanged(nameof(IsNotConnectedAndAdvancedKillSwitchActive));
    }
}