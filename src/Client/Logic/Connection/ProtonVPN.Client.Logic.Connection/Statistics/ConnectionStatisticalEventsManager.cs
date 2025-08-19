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

using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Extensions;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;
using ProtonVPN.StatisticalEvents.Contracts.Models;
using ProtonVPN.Logging.Contracts.Events.StatisticsLogs;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Observers;

namespace ProtonVPN.Client.Logic.Connection.Statistics;

public class ConnectionStatisticalEventsManager : IConnectionStatisticalEventsManager
{
    private readonly IVpnConnectionStatisticalEventSender _vpnConnectionStatisticalEventSender;
    private readonly IVpnDisconnectionStatisticalEventSender _vpnDisconnectionStatisticalEventSender;
    private readonly ISystemNetworkInterfaces _networkInterfaces;
    private readonly IFeatureFlagsObserver _featureFlagsObserver;
    private readonly ISettings _settings;
    private readonly ILogger _logger;

    private AttemptType? _currentAttemptType = null;
    private VpnTriggerDimension? _currentAttemptTrigger = null;
    private DateTime? _currentAttemptDateUtc = null;
    private ConnectionStatus? _currentAttemptConnectionStatus = null;

    private ConnectionDetails? _lastKnownConnectionDetails = null;

    public ConnectionStatisticalEventsManager(
        IVpnConnectionStatisticalEventSender vpnConnectionStatisticalEventSender,
        IVpnDisconnectionStatisticalEventSender vpnDisconnectionStatisticalEventSender,
        ISystemNetworkInterfaces networkInterfaces,
        IFeatureFlagsObserver featureFlagsObserver,
        ISettings settings,
        ILogger logger)
    {
        _vpnConnectionStatisticalEventSender = vpnConnectionStatisticalEventSender;
        _vpnDisconnectionStatisticalEventSender = vpnDisconnectionStatisticalEventSender;
        _networkInterfaces = networkInterfaces;
        _featureFlagsObserver = featureFlagsObserver;
        _settings = settings;
        _logger = logger;
    }

    public void SetConnectionAttempt(VpnTriggerDimension trigger, ConnectionStatus currentConnectionStatus)
    {
        SetAttempt(AttemptType.Connection, trigger, currentConnectionStatus);
    }

    public void SetReconnectionAttempt(VpnTriggerDimension trigger, ConnectionStatus currentConnectionStatus)
    {
        if (_currentAttemptType == AttemptType.Connection && trigger == VpnTriggerDimension.Auto)
        {
            return;
        }

        SetAttempt(AttemptType.Connection, trigger, currentConnectionStatus);
    }

    public void SetDisconnectionAttempt(VpnTriggerDimension trigger, ConnectionStatus currentConnectionStatus)
    {
        SetAttempt(AttemptType.Disconnection, trigger, currentConnectionStatus);
    }

    public void OnVpnStateChanged(VpnStatusIpcEntity vpnStatus, VpnErrorTypeIpcEntity vpnError, ConnectionDetails? connectionDetails)
    {
        try
        {
            if (connectionDetails != null)
            {
                // If connection details are not null, first update the connection details, then handle the vpn state update.
                _lastKnownConnectionDetails = connectionDetails;
                HandleVpnState(vpnStatus, vpnError);
            }
            else
            {
                // If connection details are null, first handle the vpn state update, then update the connection details.
                HandleVpnState(vpnStatus, vpnError);
                _lastKnownConnectionDetails = connectionDetails;
            }
        }
        catch (Exception e)
        {
            _logger.Warn<ConnectionStatisticsLog>($"Error while handling the vpn state change.", e);
        }
    }

    private void HandleVpnState(VpnStatusIpcEntity vpnStatus, VpnErrorTypeIpcEntity vpnError)
    {
        if (_currentAttemptType is null)
        {
            return;
        }

        OutcomeDimension? outcome = _currentAttemptType switch
        {
            AttemptType.Connection
                when vpnStatus is VpnStatusIpcEntity.Connected => OutcomeDimension.Success,
            AttemptType.Disconnection
                when vpnStatus is VpnStatusIpcEntity.Disconnected => OutcomeDimension.Success,
            AttemptType.Connection
                when vpnStatus is VpnStatusIpcEntity.Disconnected &&
                     vpnError is not VpnErrorTypeIpcEntity.None or VpnErrorTypeIpcEntity.NoneKeepEnabledKillSwitch => OutcomeDimension.Failure,
            AttemptType.Disconnection
                when vpnStatus is VpnStatusIpcEntity.Connected => OutcomeDimension.Failure,
            _ => null
        };

        if (outcome.HasValue)
        {
            HandleCurrentAttempt(outcome.Value);

            ResetAttempt();
        }
    }

    private void HandleCurrentAttempt(OutcomeDimension outcome)
    {
        if (_currentAttemptType is null)
        {
            return;
        }

        switch (_currentAttemptType)
        {
            case AttemptType.Connection:
                SendConnectionEvent(outcome);
                break;
            case AttemptType.Disconnection:
                SendDisconnectionEvent(outcome);
                break;
            default:
                break;
        }
    }

    private void SetAttempt(AttemptType type, VpnTriggerDimension trigger, ConnectionStatus connectionStatus)
    {
        HandleCurrentAttempt(OutcomeDimension.Aborted);

        _currentAttemptType = type;
        _currentAttemptTrigger = trigger;
        _currentAttemptDateUtc = DateTime.UtcNow;
        _currentAttemptConnectionStatus = connectionStatus;
    }

    private void ResetAttempt()
    {
        _currentAttemptType = null;
        _currentAttemptTrigger = null;
        _currentAttemptDateUtc = null;
        _currentAttemptConnectionStatus = null;
    }

    private void SendConnectionEvent(OutcomeDimension outcome)
    {
        float connectionTimeInMs = _currentAttemptDateUtc.HasValue
            ? (float)DateTime.UtcNow.Subtract(_currentAttemptDateUtc.Value).TotalMilliseconds
            : 0;

        VpnConnectionEventData eventData = CreateConnectionEventData(outcome);

        _vpnConnectionStatisticalEventSender.Send(eventData, connectionTimeInMs);
        _logger.Info<ConnectionStatisticsLog>($"vpn_connection event from {eventData.VpnTrigger} trigger. {eventData.Outcome}. Connection time: {connectionTimeInMs}ms");
    }

    private void SendDisconnectionEvent(OutcomeDimension outcome)
    {
        float sessionTimeInMs = _lastKnownConnectionDetails?.EstablishedConnectionTimeUtc != null
            ? (float)DateTime.UtcNow.Subtract(_lastKnownConnectionDetails.EstablishedConnectionTimeUtc.Value).TotalMilliseconds
            : 0;

        VpnConnectionEventData eventData = CreateConnectionEventData(outcome);

        _vpnDisconnectionStatisticalEventSender.Send(eventData, sessionTimeInMs);
        _logger.Info<ConnectionStatisticsLog>($"vpn_disconnection event from {eventData.VpnTrigger} trigger. {eventData.Outcome}. Session time: {sessionTimeInMs}ms");
    }

    private VpnConnectionEventData CreateConnectionEventData(OutcomeDimension outcome)
    {
        VpnFeatureIntent vpnFeatureIntent = _lastKnownConnectionDetails?.OriginalConnectionIntent?.Feature switch
        {
            SecureCoreFeatureIntent => VpnFeatureIntent.SecureCore,
            P2PFeatureIntent => VpnFeatureIntent.P2P,
            TorFeatureIntent => VpnFeatureIntent.Tor,
            B2BFeatureIntent => VpnFeatureIntent.Gateway,
            _ => VpnFeatureIntent.Standard
        };

        DeviceLocation deviceLocation = _settings.DeviceLocation ?? DeviceLocation.Unknown;

        return new VpnConnectionEventData
        {
            Outcome = outcome,
            VpnStatus = _currentAttemptConnectionStatus is ConnectionStatus.Connected
                ? VpnStatusDimension.On
                : VpnStatusDimension.Off,
            VpnTrigger = _currentAttemptTrigger,
            NetworkConnectionType = _networkInterfaces.GetNetworkConnectionType(),
            Protocol = _lastKnownConnectionDetails?.Protocol,
            VpnFeatureIntent = vpnFeatureIntent,
            Isp = deviceLocation.Isp,
            UserCountry = deviceLocation.CountryCode,
            VpnCountry = _lastKnownConnectionDetails?.ExitCountryCode,
            Port = _lastKnownConnectionDetails?.Port ?? 0,
            VpnPlan = _settings.VpnPlan,
            IsIpv6Enabled = _featureFlagsObserver.IsIpv6SupportEnabled && _settings.IsIpv6Enabled,
            Server = new ServerDetailsEventData
            {
                Name = _lastKnownConnectionDetails?.Server.Name,
                EntryIp = _lastKnownConnectionDetails?.EntryIpAddress,
                IsFree = _lastKnownConnectionDetails?.Server.IsFree() ?? false,
                IsB2B = _lastKnownConnectionDetails?.Server.Features.IsB2B() ?? false,
                SupportsTor = _lastKnownConnectionDetails?.Server.Features.IsSupported(ServerFeatures.Tor) ?? false,
                SupportsP2P = _lastKnownConnectionDetails?.Server.Features.IsSupported(ServerFeatures.P2P) ?? false,
                SecureCore = _lastKnownConnectionDetails?.Server.Features.IsSupported(ServerFeatures.SecureCore) ?? false,
                SupportsStreaming = _lastKnownConnectionDetails?.Server.Features.IsSupported(ServerFeatures.Streaming) ?? false,
                SupportsIpv6 = _lastKnownConnectionDetails?.Server.Features.IsSupported(ServerFeatures.Ipv6) ?? false,
            },
        };
    }
}
