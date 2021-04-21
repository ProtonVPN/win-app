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

using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals.Protocols;
using ProtonVPN.Modals.Reconnections;
using ProtonVPN.Notifications;
using ProtonVPN.Translations;

namespace ProtonVPN.Core.Service.Vpn
{
    public class VpnReconnector : IVpnReconnector
    {
        private readonly IAppSettings _appSettings;
        private readonly ISimilarServerCandidatesGenerator _similarServerCandidatesGenerator;
        private readonly IModals _modals;
        private readonly IVpnConnector _vpnConnector;
        private readonly INotificationSender _notificationSender;

        private VpnReconnectionSteps _reconnectionStep;

        public VpnReconnector(IAppSettings appSettings,
            ISimilarServerCandidatesGenerator similarServerCandidatesGenerator,
            IModals modals,
            IVpnConnector vpnConnector, 
            INotificationSender notificationSender)
        {
            _appSettings = appSettings;
            _similarServerCandidatesGenerator = similarServerCandidatesGenerator;
            _modals = modals;
            _vpnConnector = vpnConnector;
            _notificationSender = notificationSender;
        }

        public async Task ReconnectAsync(Server lastServer, Profile lastProfile, VpnReconnectionSettings settings = null)
        {
            settings ??= new VpnReconnectionSettings();

            if (!settings.IsToReconnectIfDisconnected && IsDisconnected())
            {
                _reconnectionStep = VpnReconnectionSteps.UserChoice;
                return;
            }

            VpnReconnectionSteps reconnectionStep = _reconnectionStep;
            bool isSmartReconnectEnabled = settings.IsToForceSmartReconnect || _appSettings.IsSmartReconnectEnabled();

            reconnectionStep = IncrementReconnectionStep(reconnectionStep, lastServer, lastProfile,
                isSmartReconnectEnabled: isSmartReconnectEnabled, isToExcludeLastServer: settings.IsToExcludeLastServer);

            await ExecuteReconnectionAsync(reconnectionStep, lastServer, lastProfile,
                isSmartReconnectEnabled: isSmartReconnectEnabled, isToTryLastServer: !settings.IsToExcludeLastServer);

            if (reconnectionStep == VpnReconnectionSteps.Disconnect)
            {
                ShowDisconnectionModal(isSmartReconnectEnabled, lastServer, lastProfile);
                reconnectionStep = VpnReconnectionSteps.UserChoice;
            }

            _reconnectionStep = reconnectionStep;
        }

        private bool IsDisconnected()
        {
            return _vpnConnector.State.Status == VpnStatus.Disconnected ||
                   _vpnConnector.State.Status == VpnStatus.Disconnecting;
        }

        private VpnReconnectionSteps IncrementReconnectionStep(VpnReconnectionSteps reconnectionStep,
            Server lastServer, Profile lastProfile, bool isSmartReconnectEnabled, bool isToExcludeLastServer)
        {
            if (reconnectionStep < VpnReconnectionSteps.Disconnect)
            {
                reconnectionStep++;
            }

            reconnectionStep = UpdateReconnectionStepIfIsToExcludeLastServerAndSmartReconnectIsDisabled(
                isSmartReconnectEnabled: isSmartReconnectEnabled, isToExcludeLastServer: isToExcludeLastServer, reconnectionStep);
            reconnectionStep = UpdateReconnectionStepIfLastConnectionDataIsNotAvailable(reconnectionStep, lastServer, lastProfile);
            reconnectionStep = UpdateReconnectionStepIfSimilarOnAutoProtocol(reconnectionStep);
            reconnectionStep = UpdateQuickConnectReconnectionStepIfSmartReconnectIsDisabled(isSmartReconnectEnabled, reconnectionStep);

            return reconnectionStep;
        }

        private VpnReconnectionSteps UpdateReconnectionStepIfIsToExcludeLastServerAndSmartReconnectIsDisabled(
            bool isSmartReconnectEnabled, bool isToExcludeLastServer, VpnReconnectionSteps reconnectionStep)
        {
            if (isToExcludeLastServer && !isSmartReconnectEnabled)
            {
                reconnectionStep = VpnReconnectionSteps.Disconnect;
            }

            return reconnectionStep;
        }

        private VpnReconnectionSteps UpdateReconnectionStepIfLastConnectionDataIsNotAvailable(
            VpnReconnectionSteps reconnectionStep, Server lastServer, Profile lastProfile)
        {
            if (lastProfile == null && (lastServer == null || lastServer.IsEmpty()))
            {
                reconnectionStep = reconnectionStep == VpnReconnectionSteps.Disconnect
                    ? VpnReconnectionSteps.Disconnect
                    : VpnReconnectionSteps.QuickConnect;
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
            Protocol protocol = _appSettings.GetProtocol();
            if (protocol == Protocol.Auto)
            {
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
                return VpnReconnectionSteps.SimilarOnAutoProtocol;
            }

            return VpnReconnectionSteps.Disconnect;
        }

        private VpnReconnectionSteps UpdateQuickConnectReconnectionStepIfSmartReconnectIsDisabled(
            bool isSmartReconnectEnabled, VpnReconnectionSteps reconnectionStep)
        {
            if (!isSmartReconnectEnabled && reconnectionStep == VpnReconnectionSteps.QuickConnect)
            {
                reconnectionStep = VpnReconnectionSteps.Disconnect;
            }

            return reconnectionStep;
        }

        private async Task ExecuteReconnectionAsync(VpnReconnectionSteps reconnectionStep, Server lastServer, Profile lastProfile,
            bool isSmartReconnectEnabled, bool isToTryLastServer)
        {
            if (isSmartReconnectEnabled)
            {
                await ExecuteSmartReconnectStepAsync(reconnectionStep, lastServer, lastProfile, isToTryLastServer);
            }
            else if (isToTryLastServer)
            {
                await ExecuteNonSmartReconnectStepAsync(reconnectionStep, lastServer, lastProfile);
            }
        }

        private async Task ExecuteSmartReconnectStepAsync(VpnReconnectionSteps reconnectionStep,
            Server lastServer, Profile lastProfile, bool isToTryLastServer)
        {
            switch (reconnectionStep)
            {
                case VpnReconnectionSteps.SimilarOnSameProtocol:
                    await ConnectToSimilarServerAsync(isToTryLastServer, lastServer, lastProfile, _appSettings.GetProtocol());
                    break;
                case VpnReconnectionSteps.SimilarOnAutoProtocol:
                    await ConnectToSimilarServerAsync(isToTryLastServer, lastServer, lastProfile, Protocol.Auto);
                    break;
                case VpnReconnectionSteps.QuickConnect:
                    await _vpnConnector.QuickConnectAsync();
                    break;
            }
        }

        private async Task ConnectToSimilarServerAsync(bool isToTryLastServer, Server lastServer, Profile lastProfile, Protocol protocol)
        {
            ServerCandidates serverCandidates = _similarServerCandidatesGenerator
                .Generate(isToTryLastServer, lastServer, lastProfile);
            await _vpnConnector.ConnectToPreSortedCandidatesAsync(serverCandidates, protocol);
        }

        private async Task ExecuteNonSmartReconnectStepAsync(VpnReconnectionSteps reconnectionStep, Server lastServer, Profile lastProfile)
        {
            switch (reconnectionStep)
            {
                case VpnReconnectionSteps.SimilarOnSameProtocol:
                    await ConnectToLastServerAsync(lastServer, lastProfile, _appSettings.GetProtocol());
                    break;
                case VpnReconnectionSteps.SimilarOnAutoProtocol:
                    await ConnectToLastServerAsync(lastServer, lastProfile, Protocol.Auto);
                    break;
            }
        }

        private async Task ConnectToLastServerAsync(Server lastServer, Profile lastProfile, Protocol protocol)
        {
            if (IsSecureCoreDifferentFromLastChoice(lastServer, lastProfile))
            {
                await ConnectToSimilarServerAsync(isToTryLastServer: false, lastServer, lastProfile, protocol);
            }
            else
            {
                Profile profile = lastProfile ?? new Profile
                {
                    IsTemporary = true,
                    ProfileType = ProfileType.Custom,
                    Features = (Features)lastServer.Features,
                    ServerId = lastServer.Id
                };

                await _vpnConnector.ConnectToProfileAsync(profile, protocol);
            }
        }

        private bool IsSecureCoreDifferentFromLastChoice(Server lastServer, Profile lastProfile)
        {
            return
                ((lastServer != null && !lastServer.IsEmpty()) && IsSecureCoreDifferent((Features)lastServer.Features)) ||
                (lastProfile != null && IsSecureCoreDifferent(lastProfile.Features));
        }

        private bool IsSecureCoreDifferent(Features features)
        {
            return (features & Features.SecureCore) > 0 != _appSettings.SecureCore;
        }

        private void ShowDisconnectionModal(bool isSmartReconnectEnabled, Server lastServer, Profile lastProfile)
        {
            if (!isSmartReconnectEnabled && (lastProfile != null || (lastServer != null && !lastServer.IsEmpty())))
            {
                _modals.Show<NonSmartReconnectionFailedModalViewModel>();
            }
        }

        public void OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (e.State.Status == VpnStatus.Connected)
            {
                ResetReconnectionStep();
            }
        }

        public void ResetReconnectionStep()
        {
            _reconnectionStep = VpnReconnectionSteps.UserChoice;
        }

        public void OnDisconnectionRequest()
        {
            ResetReconnectionStep();
        }
    }
}