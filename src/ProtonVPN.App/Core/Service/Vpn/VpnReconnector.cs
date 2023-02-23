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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectLogs;
using ProtonVPN.Common.Networking;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Windows.Popups;
using ProtonVPN.Modals.Protocols;
using ProtonVPN.Notifications;
using ProtonVPN.Sidebar;
using ProtonVPN.Translations;
using ProtonVPN.Vpn.Connectors;
using ProtonVPN.Windows.Popups.Delinquency;
using ProtonVPN.Windows.Popups.SubscriptionExpiration;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnReconnector : IVpnReconnector, IVpnStateAware
    {
        private readonly IAppSettings _appSettings;
        private readonly ISimilarServerCandidatesGenerator _similarServerCandidatesGenerator;
        private readonly IModals _modals;
        private readonly IPopupWindows _popups;
        private readonly IVpnConnector _vpnConnector;
        private readonly INotificationSender _notificationSender;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly Lazy<ConnectionStatusViewModel> _connectionStatusViewModel;
        private readonly ServerManager _serverManager;
        private readonly IProfileFactory _profileFactory;
        private readonly Lazy<ProfileConnector> _profileConnector;

        private VpnReconnectionSteps _reconnectionStep;
        private Server _lastConnectedServer;
        private Server _targetServer;
        private Profile _targetProfile;
        private bool _isToShowReconnectionPopup;

        public VpnReconnector(IAppSettings appSettings,
            ISimilarServerCandidatesGenerator similarServerCandidatesGenerator,
            IModals modals,
            IPopupWindows popups,
            IVpnConnector vpnConnector,
            INotificationSender notificationSender,
            ILogger logger,
            IConfiguration config,
            Lazy<ConnectionStatusViewModel> connectionStatusViewModel,
            ServerManager serverManager,
            IProfileFactory profileFactory,
            Lazy<ProfileConnector> profileConnector)
        {
            _appSettings = appSettings;
            _similarServerCandidatesGenerator = similarServerCandidatesGenerator;
            _modals = modals;
            _popups = popups;
            _vpnConnector = vpnConnector;
            _notificationSender = notificationSender;
            _logger = logger;
            _config = config;
            _connectionStatusViewModel = connectionStatusViewModel;
            _serverManager = serverManager;
            _profileFactory = profileFactory;
            _profileConnector = profileConnector;
        }

        public async Task ReconnectAsync(Server lastServer, Profile lastProfile, VpnReconnectionSettings settings = null)
        {
            settings ??= new VpnReconnectionSettings();

            if (!settings.IsToReconnectIfDisconnected && IsDisconnected())
            {
                _logger.Info<ConnectLog>("Reconnection refused because it is disconnected.");
                ResetReconnectionStep();
                return;
            }
            
            if (_appSettings.IsSmartReconnectEnabled() || settings.IsToForceSmartReconnect || HasSecureCoreChanged(lastProfile, lastServer))
            {
                await SmartReconnectAsync(lastServer, lastProfile, settings);
            }
            else
            {
                await NonSmartReconnectAsync(lastServer, lastProfile);
            }
        }

        private bool HasSecureCoreChanged(Profile lastProfile, Server lastServer)
        {
            return (lastProfile != null && (lastProfile.Features & Features.SecureCore) > 0 != _appSettings.SecureCore) || 
                   (lastProfile == null && lastServer != null && lastServer.IsSecureCore() != _appSettings.SecureCore);
        }

        private async Task NonSmartReconnectAsync(Server lastServer, Profile lastProfile)
        {
            if (lastProfile != null)
            {
                _logger.Info<ConnectLog>("Reconnecting to last profile");
                await _vpnConnector.ConnectToProfileAsync(lastProfile);
            }
            else if (lastServer != null)
            {
                _logger.Info<ConnectLog>("Reconnecting to last server");
                Profile profile = _profileFactory.CreateFromServer(lastServer);
                await _vpnConnector.ConnectToProfileAsync(profile);
            }
            else
            {
                _logger.Warn<ConnectLog>("Cannot reconnect because last profile and last server are empty.");
            }

            ResetReconnectionStep();
        }

        private async Task SmartReconnectAsync(Server lastServer, Profile lastProfile, VpnReconnectionSettings settings)
        {
            _isToShowReconnectionPopup = settings.IsToShowReconnectionPopup;
            VpnReconnectionSteps reconnectionStep = _reconnectionStep;
            SetTargetServerAndProfile(lastServer, lastProfile, reconnectionStep);
            reconnectionStep = IncrementReconnectionStep(reconnectionStep);
            await ExecuteReconnectionAsync(reconnectionStep, isToTryLastServer: !settings.IsToExcludeLastServer);

            if (reconnectionStep == VpnReconnectionSteps.Disconnect)
            {
                ResetReconnectionStep();
            }
            else
            {
                _reconnectionStep = reconnectionStep;
            }
        }

        private bool IsDisconnected()
        {
            return _vpnConnector.State.Status == VpnStatus.Disconnected ||
                   _vpnConnector.State.Status == VpnStatus.Disconnecting;
        }

        private void SetTargetServerAndProfile(Server lastServer, Profile lastProfile, VpnReconnectionSteps reconnectionStep)
        {
            if (reconnectionStep == VpnReconnectionSteps.UserChoice)
            {
                _targetServer = lastServer;
                _targetProfile = lastProfile;
            }
            else if (_targetServer != null && !_targetServer.IsEmpty())
            {
                Server updatedTargetServer = _serverManager.GetServers(new ServerById(_targetServer.Id)).FirstOrDefault();
                if (updatedTargetServer != null && !updatedTargetServer.IsEmpty())
                {
                    _targetServer = updatedTargetServer;
                }
            }
        }

        private VpnReconnectionSteps IncrementReconnectionStep(VpnReconnectionSteps reconnectionStep)
        {
            if (reconnectionStep < VpnReconnectionSteps.RestartReconnectionSteps)
            {
                reconnectionStep++;
            }

            reconnectionStep = UpdateReconnectionStepIfLastConnectionDataIsNotAvailable(reconnectionStep);
            reconnectionStep = UpdateReconnectionStepIfSimilarOnAutoProtocol(reconnectionStep);
            reconnectionStep = RestartReconnectionStepsIfRequested(reconnectionStep);

            return reconnectionStep;
        }

        private VpnReconnectionSteps UpdateReconnectionStepIfLastConnectionDataIsNotAvailable(
            VpnReconnectionSteps reconnectionStep)
        {
            if (_targetProfile == null && 
                (_targetServer == null || _targetServer.IsEmpty()) &&
                reconnectionStep != VpnReconnectionSteps.RestartReconnectionSteps && 
                reconnectionStep != VpnReconnectionSteps.Disconnect)
            {
                _logger.Info<ConnectLog>("There is no last connection data, so reconnection will fast forward to quick connect.");
                return VpnReconnectionSteps.QuickConnect;
            }

            return reconnectionStep;
        }

        private VpnReconnectionSteps UpdateReconnectionStepIfSimilarOnAutoProtocol(VpnReconnectionSteps reconnectionStep)
        {
            if (reconnectionStep == VpnReconnectionSteps.SimilarOnSmartProtocol)
            {
                reconnectionStep = CalculateSimilarOnAutoProtocolReconnectionStep();
            }

            return reconnectionStep;
        }

        private VpnReconnectionSteps CalculateSimilarOnAutoProtocolReconnectionStep()
        {
            if (_appSettings.DoNotShowEnableSmartProtocolDialog || _appSettings.GetProtocol() == VpnProtocol.Smart)
            {
                _logger.Info<ConnectLog>(_appSettings.GetProtocol() == VpnProtocol.Smart
                    ? "Smart protocol is already enabled. Reconnection will fast forward to quick connect."
                    : "Not asking again if the user wants to enable Smart Protocol. Reconnection will fast forward to quick connect.");
                return VpnReconnectionSteps.QuickConnect;
            }

            if (_appSettings.IsSmartReconnectNotificationsEnabled())
            {
                _notificationSender.Send(
                    Translation.Get("Notifications_EnableSmartProtocol_ttl"),
                    Translation.Get("Notifications_EnableSmartProtocol_msg"));
            }
            
            bool? isToChangeProtocolToAuto = _modals.Show<EnableSmartProtocolModalViewModel>();
            if (isToChangeProtocolToAuto.HasValue && isToChangeProtocolToAuto.Value)
            {
                _appSettings.OvpnProtocol = "auto";
                return VpnReconnectionSteps.SimilarOnSmartProtocol;
            }

            _logger.Info<ConnectLog>("User refused to enable Smart Protocol. Reconnection will fast forward to quick connect.");
            return VpnReconnectionSteps.QuickConnect;
        }

        private VpnReconnectionSteps RestartReconnectionStepsIfRequested(VpnReconnectionSteps reconnectionStep)
        {
            if (reconnectionStep == VpnReconnectionSteps.RestartReconnectionSteps)
            {
                _logger.Info<ConnectLog>("Restarting reconnection process from the first step.");
                reconnectionStep = VpnReconnectionSteps.SimilarOnSameProtocol;
            }

            return reconnectionStep;
        }

        private async Task ExecuteReconnectionAsync(VpnReconnectionSteps reconnectionStep, bool isToTryLastServer)
        {
            _logger.Info<ConnectLog>($"Executing Smart Reconnect step '{reconnectionStep}' (IsToTryLastServer: '{isToTryLastServer}').");
            switch (reconnectionStep)
            {
                case VpnReconnectionSteps.SimilarOnSameProtocol:
                    await ConnectToSimilarServerAsync(isToTryLastServer, _appSettings.GetProtocol());
                    break;
                case VpnReconnectionSteps.SimilarOnSmartProtocol:
                    await ConnectToSimilarServerAsync(isToTryLastServer, VpnProtocol.Smart);
                    break;
                case VpnReconnectionSteps.QuickConnect:
                    await ConnectToSimilarServerOrQuickConnectAsync(isToTryLastServer, _appSettings.GetProtocol());
                    break;
            }
        }

        private async Task ConnectToSimilarServerOrQuickConnectAsync(bool isToTryLastServer, VpnProtocol vpnProtocol)
        {
            IList<Server> serverCandidates = _similarServerCandidatesGenerator.Generate(isToTryLastServer, _targetServer, _targetProfile);
            IEnumerable<Server> quickConnectServers = (await _vpnConnector.GetSortedAndValidQuickConnectServersAsync(
                _config.MaxQuickConnectServersOnReconnection)).Except(serverCandidates);

            serverCandidates.AddRange(quickConnectServers);
            if (_config.MaxQuickConnectServersOnReconnection.HasValue)
            {
                serverCandidates = serverCandidates.Take(_config.MaxQuickConnectServersOnReconnection.Value).ToList();
            }

            await ConnectToPreSortedCandidatesAsync(serverCandidates, vpnProtocol);
        }

        private async Task ConnectToPreSortedCandidatesAsync(IList<Server> serverCandidates, VpnProtocol vpnProtocol)
        {
            string firstServerMessage = string.Empty;
            if (serverCandidates.Any())
            {
                Server firstServer = serverCandidates.First();
                firstServerMessage = $" First server is {firstServer.Name} ({firstServer.ExitIp}).";
            }
            _logger.Info<ConnectLog>($"Reconnecting to {serverCandidates.Count} servers.{firstServerMessage}");

            await _vpnConnector.ConnectToPreSortedCandidatesAsync((IReadOnlyCollection<Server>)serverCandidates, vpnProtocol);
        }

        private async Task ConnectToSimilarServerAsync(bool isToTryLastServer, VpnProtocol vpnProtocol)
        {
            IList<Server> sortedCandidates = _similarServerCandidatesGenerator
                .Generate(isToTryLastServer, _targetServer, _targetProfile);
            await ConnectToPreSortedCandidatesAsync(sortedCandidates, vpnProtocol);
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (e.State.Status == VpnStatus.Connected)
            {
                Server currentServer = e.State.Server;
                ShowVpnAcceleratorReconnectionPopupIfPossible(currentServer);
                ResetReconnectionStep();
                _lastConnectedServer = currentServer;
            }
        }

        private void ShowVpnAcceleratorReconnectionPopupIfPossible(Server currentServer)
        {
            Server previousServer = _lastConnectedServer;

            if (IsToShowVpnAcceleratorReconnectionPopup(previousServer: previousServer, currentServer: currentServer))
            {
                _connectionStatusViewModel.Value.ShowVpnAcceleratorReconnectionPopup(
                    previousServer: previousServer, currentServer: currentServer);
            }
        }

        private bool IsToShowVpnAcceleratorReconnectionPopup(Server previousServer, Server currentServer)
        {
            return _isToShowReconnectionPopup &&
                   _appSettings.IsSmartReconnectNotificationsEnabled() &&
                   _reconnectionStep > VpnReconnectionSteps.UserChoice &&
                   !previousServer.IsNullOrEmpty() && 
                   !currentServer.IsNullOrEmpty() &&
                   !previousServer.Equals(currentServer) &&
                   !_profileConnector.Value.IsServerFromProfile(currentServer, _vpnConnector.LastProfile) &&
                   !_popups.IsOpen<DelinquencyPopupViewModel>() &&
                   !_popups.IsOpen<SubscriptionExpiredPopupViewModel>();
        }

        public void ResetReconnectionStep()
        {
            _reconnectionStep = VpnReconnectionSteps.UserChoice;
            _targetServer = null;
            _targetProfile = null;
        }

        public void OnDisconnectionRequest()
        {
            ResetReconnectionStep();
            _lastConnectedServer = null;
        }
    }
}