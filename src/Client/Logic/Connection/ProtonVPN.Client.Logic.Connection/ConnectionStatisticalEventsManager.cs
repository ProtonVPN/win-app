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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;
using ProtonVPN.StatisticalEvents.Contracts.Models;

namespace ProtonVPN.Client.Logic.Connection;

public class ConnectionStatisticalEventsManager : IConnectionStatisticalEventsManager
{
    private readonly IVpnConnectionStatisticalEventSender _vpnConnectionStatisticalEventSender;
    private readonly IVpnDisconnectionStatisticalEventSender _vpnDisconnectionStatisticalEventSender;
    private readonly ISystemNetworkInterfaces _networkInterfaces;
    private readonly ISettings _settings;

    private DateTime? _lastConnectionAttemptDateUtc;
    private DateTime? _lastSuccessfulConnectionDateUtc;
    private VpnTriggerDimension? _lastConnectionTrigger;
    private VpnTriggerDimension? _lastDisconnectionTrigger;
    private bool _isConnectionCanceled;
    private bool _wasConnected;
    private ConnectionDetails? _lastConnectionDetails;

    public bool HasSuccessfulConnection => _lastSuccessfulConnectionDateUtc.HasValue;

    public IConnectionIntent? CurrentConnectionIntent { get; set; }

    public ConnectionStatisticalEventsManager(
        IVpnConnectionStatisticalEventSender vpnConnectionStatisticalEventSender,
        IVpnDisconnectionStatisticalEventSender vpnDisconnectionStatisticalEventSender,
        ISystemNetworkInterfaces networkInterfaces,
        ISettings settings)
    {
        _vpnConnectionStatisticalEventSender = vpnConnectionStatisticalEventSender;
        _vpnDisconnectionStatisticalEventSender = vpnDisconnectionStatisticalEventSender;
        _networkInterfaces = networkInterfaces;
        _settings = settings;
    }

    public void SetConnectionAttempt(VpnTriggerDimension trigger, bool forceUpdate = true)
    {
        if (!forceUpdate)
        {
            return;
        }

        _lastConnectionAttemptDateUtc = DateTime.UtcNow;
        _lastConnectionTrigger = trigger;
    }

    public void SetReconnectionAttempt(ConnectionStatus connectionStatus)
    {
        bool forceUpdate = connectionStatus == ConnectionStatus.Connected;
        SetConnectionAttempt(VpnTriggerDimension.Auto, forceUpdate);
    }

    public void SetDisconnectionTrigger(VpnTriggerDimension trigger)
    {
        _lastDisconnectionTrigger = trigger;
    }

    public void SetConnectionCanceled(bool isCanceled)
    {
        _isConnectionCanceled = isCanceled;
    }

    public void UpdateConnectionDetails(ConnectionDetails? details)
    {
        _lastConnectionDetails = details;
    }

    public void HandleStatisticalEvents(ConnectionStatus connectionStatus, VpnErrorTypeIpcEntity error)
    {
        if (connectionStatus == ConnectionStatus.Connected)
        {
            HandleConnectionStatisticalEvent();
            _wasConnected = true;
        }
        else if (connectionStatus == ConnectionStatus.Disconnected && error != VpnErrorTypeIpcEntity.NoneKeepEnabledKillSwitch)
        {
            HandleDisconnectionStatisticalEvent(error);
            _wasConnected = false;
        }
    }

    private void HandleConnectionStatisticalEvent()
    {
        if (_lastConnectionAttemptDateUtc == null || _lastConnectionTrigger == null)
        {
            return;
        }

        float totalMilliseconds = (float)DateTime.UtcNow.Subtract(_lastConnectionAttemptDateUtc.Value).TotalMilliseconds;
        _lastSuccessfulConnectionDateUtc = DateTime.UtcNow;

        VpnConnectionEventData eventData = GetConnectionEventData(OutcomeDimension.Success, _lastConnectionTrigger.Value);

        _vpnConnectionStatisticalEventSender.Send(eventData, totalMilliseconds);
    }

    private void HandleDisconnectionStatisticalEvent(VpnErrorTypeIpcEntity error)
    {
        if (_lastConnectionAttemptDateUtc is null)
        {
            return;
        }

        DateTime? pastTime = _isConnectionCanceled
            ? _lastConnectionAttemptDateUtc
            : _wasConnected ? _lastSuccessfulConnectionDateUtc : _lastConnectionAttemptDateUtc;

        float sessionTimeInMilliseconds = pastTime.HasValue
            ? (float)DateTime.UtcNow.Subtract(pastTime.Value).TotalMilliseconds
            : 0;

        OutcomeDimension outcomeDimension = _isConnectionCanceled
            ? OutcomeDimension.Aborted
            : (_lastSuccessfulConnectionDateUtc is not null && (error == VpnErrorTypeIpcEntity.None || error == VpnErrorTypeIpcEntity.NoneKeepEnabledKillSwitch)
                ? OutcomeDimension.Success
                : OutcomeDimension.Failure);

        _lastConnectionAttemptDateUtc = null;
        _lastSuccessfulConnectionDateUtc = null;
        _isConnectionCanceled = false;

        VpnConnectionEventData eventData = GetConnectionEventData(outcomeDimension, _lastDisconnectionTrigger ?? VpnTriggerDimension.Auto);

        _vpnDisconnectionStatisticalEventSender.Send(eventData, sessionTimeInMilliseconds);
    }

    private VpnConnectionEventData GetConnectionEventData(OutcomeDimension outcomeDimension, VpnTriggerDimension vpnTriggerDimension)
    {
        VpnFeatureIntent vpnFeatureIntent = CurrentConnectionIntent?.Feature switch
        {
            SecureCoreFeatureIntent => VpnFeatureIntent.SecureCore,
            P2PFeatureIntent => VpnFeatureIntent.P2P,
            TorFeatureIntent => VpnFeatureIntent.Tor,
            _ => VpnFeatureIntent.Standard
        };

        return new VpnConnectionEventData
        {
            Outcome = outcomeDimension,
            VpnStatus = _wasConnected ? VpnStatusDimension.On : VpnStatusDimension.Off,
            VpnTrigger = vpnTriggerDimension,
            NetworkConnectionType = _networkInterfaces.GetNetworkConnectionType(),
            Protocol = _lastConnectionDetails?.Protocol,
            VpnFeatureIntent = vpnFeatureIntent,
            Isp = _settings.DeviceLocation?.Isp,
            UserCountry = _settings.DeviceLocation?.CountryCode,
            VpnCountry = _lastConnectionDetails?.ExitCountryCode,
            Port = _lastConnectionDetails?.Port ?? 0,
            VpnPlan = _settings.VpnPlan,
            Server = new ServerDetailsEventData
            {
                Name = _lastConnectionDetails?.Server.Name,
                IsFree = _lastConnectionDetails?.Server.IsFree() ?? false,
                IsB2B = _lastConnectionDetails?.Server.Features.IsSupported(ServerFeatures.B2B) ?? false,
                SupportsTor = _lastConnectionDetails?.Server.Features.IsSupported(ServerFeatures.Tor) ?? false,
                SupportsP2P = _lastConnectionDetails?.Server.Features.IsSupported(ServerFeatures.P2P) ?? false,
                SecureCore = _lastConnectionDetails?.Server.Features.IsSupported(ServerFeatures.SecureCore) ?? false,
                SupportsStreaming = _lastConnectionDetails?.Server.Features.IsSupported(ServerFeatures.Streaming) ?? false,
                SupportsIpv6 = _lastConnectionDetails?.Server.Features.IsSupported(ServerFeatures.Ipv6) ?? false,
            },
        };
    }
}