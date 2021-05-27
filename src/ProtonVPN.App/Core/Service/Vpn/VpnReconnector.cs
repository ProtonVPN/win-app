/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals.Protocols;
using ProtonVPN.Notifications;
using ProtonVPN.Sidebar;
using ProtonVPN.Translations;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnReconnector : IVpnReconnector, IVpnStateAware
    {
        private readonly IAppSettings _appSettings;
        private readonly ISimilarServerCandidatesGenerator _similarServerCandidatesGenerator;
        private readonly IModals _modals;
        private readonly IVpnConnector _vpnConnector;
        private readonly INotificationSender _notificationSender;
        private readonly Common.Configuration.Config _config;
        private readonly Lazy<ConnectionStatusViewModel> _connectionStatusViewModel;

        private VpnReconnectionSteps _reconnectionStep;
        private Server _lastConnectedServer;
        private Server _targetServer;
        private Profile _targetProfile;

        public VpnReconnector(IAppSettings appSettings,
            ISimilarServerCandidatesGenerator similarServerCandidatesGenerator,
            IModals modals,
            IVpnConnector vpnConnector, 
            INotificationSender notificationSender, 
            Common.Configuration.Config config,
            Lazy<ConnectionStatusViewModel> connectionStatusViewModel)
        {
            _appSettings = appSettings;
            _similarServerCandidatesGenerator = similarServerCandidatesGenerator;
            _modals = modals;
            _vpnConnector = vpnConnector;
            _notificationSender = notificationSender;
            _config = config;
            _connectionStatusViewModel = connectionStatusViewModel;
        }

        public async Task ReconnectAsync(Server lastServer, Profile lastProfile, VpnReconnectionSettings settings = null)
        {
            settings ??= new VpnReconnectionSettings();

            if (!settings.IsToReconnectIfDisconnected && IsDisconnected())
            {
                ResetReconnectionStep();
                return;
            }

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
                return VpnReconnectionSteps.QuickConnect;
            }

            return reconnectionStep;
        }

        private VpnReconnectionSteps UpdateReconnectionStepIfSimilarOnAutoProtocol(VpnReconnectionSteps reconnectionStep)
        {
            if (reconnectionStep == VpnReconnectionSteps.SimilarOnAutoProtocol)
            {
                reconnectionStep = CalculateSimilarOnAutoProtocolReconnectionStep();
            }

            return reconnectionStep;
        }

        private VpnReconnectionSteps CalculateSimilarOnAutoProtocolReconnectionStep()
        {
            if (_appSettings.DoNotShowEnableSmartProtocolDialog || _appSettings.GetProtocol() == Protocol.Auto)
            {
                return VpnReconnectionSteps.QuickConnect;
            }

            if (_appSettings.IsVpnAcceleratorNotificationsEnabled())
            {
                _notificationSender.Send(
                    Translation.Get("Notifications_EnableSmartProtocol_ttl"),
                    Translation.Get("Notifications_EnableSmartProtocol_msg"));
            }
            
            bool? isToChangeProtocolToAuto = _modals.Show<EnableSmartProtocolModalViewModel>();
            if (isToChangeProtocolToAuto.HasValue && isToChangeProtocolToAuto.Value)
            {
                _appSettings.OvpnProtocol = "auto";
                return VpnReconnectionSteps.SimilarOnAutoProtocol;
            }

            return VpnReconnectionSteps.QuickConnect;
        }

        private VpnReconnectionSteps RestartReconnectionStepsIfRequested(VpnReconnectionSteps reconnectionStep)
        {
            if (reconnectionStep == VpnReconnectionSteps.RestartReconnectionSteps)
            {
                reconnectionStep = VpnReconnectionSteps.SimilarOnSameProtocol;
            }

            return reconnectionStep;
        }

        private async Task ExecuteReconnectionAsync(VpnReconnectionSteps reconnectionStep, bool isToTryLastServer)
        {
            switch (reconnectionStep)
            {
                case VpnReconnectionSteps.SimilarOnSameProtocol:
                    await ConnectToSimilarServerAsync(isToTryLastServer, _appSettings.GetProtocol());
                    break;
                case VpnReconnectionSteps.SimilarOnAutoProtocol:
                    await ConnectToSimilarServerAsync(isToTryLastServer, Protocol.Auto);
                    break;
                case VpnReconnectionSteps.QuickConnect:
                    await _vpnConnector.QuickConnectAsync(maxServers: _config.MaxQuickConnectServersOnReconnection);
                    break;
            }
        }

        private async Task ConnectToSimilarServerAsync(bool isToTryLastServer, Protocol protocol)
        {
            ServerCandidates serverCandidates = _similarServerCandidatesGenerator
                .Generate(isToTryLastServer, _targetServer, _targetProfile);
            await _vpnConnector.ConnectToPreSortedCandidatesAsync(serverCandidates, protocol);
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
            return _appSettings.IsVpnAcceleratorNotificationsEnabled() &&
                   _reconnectionStep > VpnReconnectionSteps.UserChoice &&
                   !previousServer.IsNullOrEmpty() && 
                   !currentServer.IsNullOrEmpty() &&
                   !previousServer.Equals(currentServer);
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