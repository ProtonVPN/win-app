﻿/*
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
using ProtonVPN.Account;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Servers.Reconnections;
using ProtonVPN.Sidebar;
using ProtonVPN.Translations;

namespace ProtonVPN.Windows.Popups.SubscriptionExpiration
{
    public class SubscriptionExpiredPopupViewModel : BaseUpgradePlanPopupViewModel, ISubscriptionExpiredPopupViewModel
    {
        private readonly Lazy<ConnectionStatusViewModel> _connectionStatusViewModel;
        private readonly ServerManager _serverManager;
        
        public ReconnectionData ReconnectionData { get; private set; }

        public SubscriptionExpiredPopupViewModel(
            Lazy<ConnectionStatusViewModel> connectionStatusViewModel,
            ServerManager serverManager,
            ISubscriptionManager subscriptionManager,
            AppWindow appWindow)
            : base(subscriptionManager, appWindow)
        {
            _connectionStatusViewModel = connectionStatusViewModel;
            _serverManager = serverManager;
        }

        public string ListOption1
        {
            get
            {
                int totalCountries = _serverManager.GetCountries().Count;
                return string.Format(Translation.GetPlural("Dialogs_SubscriptionExpired_ListOption1", totalCountries), totalCountries);
            }
        }

        protected override void OnViewAttached(object view, object context)
        {
            CloseVpnAcceleratorReconnectionPopup();
            base.OnViewAttached(view, context);
        }

        private void CloseVpnAcceleratorReconnectionPopup()
        {
            _connectionStatusViewModel.Value.CloseVpnAcceleratorReconnectionPopupAction();
        }

        protected override void OnViewReady(object view)
        {
            CloseVpnAcceleratorReconnectionPopup();
            base.OnViewReady(view);
        }

        protected override void OnViewLoaded(object view)
        {
            CloseVpnAcceleratorReconnectionPopup();
            base.OnViewLoaded(view);
        }

        public void SetNoReconnectionData()
        {
            ReconnectionData = new ReconnectionData();
        }

        public void SetReconnectionData(Server previousServer, Server currentServer)
        {
            ReconnectionData = new ReconnectionData(previousServer, currentServer);
        }
    }
}