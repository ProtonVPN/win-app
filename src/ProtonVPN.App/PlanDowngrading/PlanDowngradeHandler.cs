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

using System.Threading.Tasks;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Core.Windows.Popups;
using ProtonVPN.Notifications;
using ProtonVPN.Translations;
using ProtonVPN.Windows.Popups.Delinquency;
using ProtonVPN.Windows.Popups.SubscriptionExpiration;

namespace ProtonVPN.PlanDowngrading
{
    public class PlanDowngradeHandler : IVpnPlanAware, IVpnStateAware
    {
        private readonly IUserStorage _userStorage;
        private readonly IVpnManager _vpnManager;
        private readonly IAppSettings _appSettings;
        private readonly INotificationSender _notificationSender;
        private readonly IPopupWindows _popups;
        private readonly SubscriptionExpiredPopupViewModel _subscriptionExpiredPopupViewModel;
        private readonly DelinquencyPopupViewModel _delinquencyPopupViewModel;

        private VpnState _vpnState;
        private bool _isAwaitingNextConnection;
        private bool _notifyOnNextConnection;
        private Server _lastConnectedServer;
        private bool _isUserDelinquent;
        private bool _isDisconnectedDueToPlanDowngrade;

        public PlanDowngradeHandler(
            IUserStorage userStorage,
            IVpnManager vpnManager,
            IAppSettings appSettings,
            INotificationSender notificationSender,
            IPopupWindows popups,
            SubscriptionExpiredPopupViewModel subscriptionExpiredPopupViewModel,
            DelinquencyPopupViewModel delinquencyPopupViewModel)
        {
            _userStorage = userStorage;
            _vpnManager = vpnManager;
            _appSettings = appSettings;
            _notificationSender = notificationSender;
            _popups = popups;
            _subscriptionExpiredPopupViewModel = subscriptionExpiredPopupViewModel;
            _delinquencyPopupViewModel = delinquencyPopupViewModel;
        }

        public async Task OnVpnPlanChangedAsync(VpnPlanChangedEventArgs e)
        {
            if (e.IsDowngrade)
            {
                User user = _userStorage.GetUser();
                await DowngradeUserAsync(user);
            }
        }

        private async Task DowngradeUserAsync(User user)
        {
            DisablePaidFeatures(user);
            NotifyUserOfDowngrade(user);

            await _vpnManager.ReconnectAsync(new VpnReconnectionSettings
            {
                IsToForceSmartReconnect = true,
                IsToReconnectIfDisconnected = _isDisconnectedDueToPlanDowngrade
            });
        }

        private void DisablePaidFeatures(User user)
        {
            if (user.MaxTier < ServerTiers.Plus)
            {
                _appSettings.SecureCore = false;
                _appSettings.PortForwardingEnabled = false;
                _appSettings.NetShieldEnabled = false;
                _appSettings.AllowNonStandardPorts = false;
            }
        }

        private void NotifyUserOfDowngrade(User user)
        {
            if (user.IsDelinquent())
            {
                NotifyUserOfDelinquency();
            }
            else
            {
                NotifyUserOfSubscriptionExpiration();
            }
        }

        private void NotifyUserOfDelinquency()
        {
            bool isDisconnected = _vpnState.Status == VpnStatus.Disconnected || _vpnState.Status == VpnStatus.Disconnecting;
            if (isDisconnected || _lastConnectedServer.IsNullOrEmpty())
            {
                NotifyUserOfDelinquencyWithoutReconnection();
            }
            else
            {
                _isAwaitingNextConnection = _vpnState.Status == VpnStatus.Connected;
                _notifyOnNextConnection = true;
                _isUserDelinquent = true;
            }
        }

        private void NotifyUserOfDelinquencyWithoutReconnection()
        {
            _notificationSender.Send(
                Translation.Get("Notifications_Delinquency_Title"),
                Translation.Get("Notifications_Delinquency_Description"));

            _delinquencyPopupViewModel.SetNoReconnectionData();
            _popups.Show<DelinquencyPopupViewModel>();
        }

        private void NotifyUserOfSubscriptionExpiration()
        {
            bool isDisconnected = _vpnState.Status == VpnStatus.Disconnected || _vpnState.Status == VpnStatus.Disconnecting;
            if (isDisconnected || _lastConnectedServer.IsNullOrEmpty())
            {
                NotifyUserOfSubscriptionExpirationWithoutReconnection();
            }
            else
            {
                _isAwaitingNextConnection = _vpnState.Status == VpnStatus.Connected;
                _notifyOnNextConnection = true;
                _isUserDelinquent = false;
            }
        }

        private void NotifyUserOfSubscriptionExpirationWithoutReconnection()
        {
            _notificationSender.Send(
                Translation.Get("Notifications_SubscriptionExpired_Title"),
                Translation.Get("Notifications_SubscriptionExpired_Description"));

            _subscriptionExpiredPopupViewModel.SetNoReconnectionData();
            _popups.Show<SubscriptionExpiredPopupViewModel>();
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (_notifyOnNextConnection && (
                    (e.State.Status == VpnStatus.Disconnected && !e.UnexpectedDisconnect) ||
                    (e.State.Status == VpnStatus.Connected && !_isAwaitingNextConnection)))
            {
                _notifyOnNextConnection = false;
                NotifyUserOfReconnectionDueToDowngradeToFreeTier(e.State.Server);
            }

            if (e.State.Status != VpnStatus.Connected && _isAwaitingNextConnection)
            {
                _isAwaitingNextConnection = false;
            }

            bool isDisconnected = _vpnState?.Status is VpnStatus.Disconnected or VpnStatus.Disconnecting;
            if (!isDisconnected && e.Error == VpnError.PlanNeedsToBeUpgraded)
            {
                _isDisconnectedDueToPlanDowngrade = true;
            }

            if (e.State.Status == VpnStatus.Connected)
            {
                _lastConnectedServer = e.State.Server;
                _isDisconnectedDueToPlanDowngrade = false;
            }

            _vpnState = e.State;
        }

        private void NotifyUserOfReconnectionDueToDowngradeToFreeTier(Server currentServer)
        {
            if (_isUserDelinquent)
            {
                NotifyUserOfDelinquency(currentServer);
            }
            else
            {
                NotifyUserOfSubscriptionExpiration(currentServer);
            }
        }

        private void NotifyUserOfDelinquency(Server currentServer)
        {
            if (currentServer.IsNullOrEmpty() || _lastConnectedServer.IsNullOrEmpty() || _lastConnectedServer.Equals(currentServer))
            {
                NotifyUserOfDelinquencyWithoutReconnection();
            }
            else
            {
                NotifyUserOfDelinquencyWithReconnection(_lastConnectedServer, currentServer);
            }
        }

        private void NotifyUserOfDelinquencyWithReconnection(Server previousServer, Server currentServer)
        {
            _notificationSender.Send(
                Translation.Get("Notifications_Delinquency_Title"),
                Translation.Get("Notifications_Delinquency_Reconnected_Description"));

            _delinquencyPopupViewModel.SetReconnectionData(previousServer, currentServer);
            _popups.Show<DelinquencyPopupViewModel>();
        }

        private void NotifyUserOfSubscriptionExpiration(Server currentServer)
        {
            if (currentServer.IsNullOrEmpty() || _lastConnectedServer.IsNullOrEmpty() || _lastConnectedServer.Equals(currentServer))
            {
                NotifyUserOfSubscriptionExpirationWithoutReconnection();
            }
            else
            {
                NotifyUserOfSubscriptionExpirationWithReconnection(_lastConnectedServer, currentServer);
            }
        }

        private void NotifyUserOfSubscriptionExpirationWithReconnection(Server previousServer, Server currentServer)
        {
            _notificationSender.Send(
                Translation.Get("Notifications_SubscriptionExpired_Title"),
                Translation.Get("Notifications_SubscriptionExpired_Reconnected_Description"));

            _subscriptionExpiredPopupViewModel.SetReconnectionData(previousServer, currentServer);
            _popups.Show<SubscriptionExpiredPopupViewModel>();
        }
    }
}