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

using ProtonVPN.Config.Url;
using ProtonVPN.Core.Servers.Models;

namespace ProtonVPN.Windows.Popups.SubscriptionExpiration
{
    public class SubscriptionExpiredPopupViewModel : BaseUpgradePlanPopupViewModel
    {
        public bool IsReconnection { get; private set; }

        public Server FromServer { get; private set; }
        public bool IsFromServerSecureCore { get; private set; }

        public Server ToServer { get; private set; }
        public bool IsToServerSecureCore { get; private set; }

        public SubscriptionExpiredPopupViewModel(IActiveUrls urls, AppWindow appWindow)
            : base(urls, appWindow)
        {
        }

        public void SetNoReconnectionData()
        {
            IsReconnection = false;
            FromServer = null;
            ToServer = null;
        }

        public void SetReconnectionData(Server previousServer, Server currentServer)
        {
            IsReconnection = true;

            FromServer = previousServer;
            IsFromServerSecureCore = previousServer.IsSecureCore();

            ToServer = currentServer;
            IsToServerSecureCore = currentServer.IsSecureCore();
        }
    }
}
