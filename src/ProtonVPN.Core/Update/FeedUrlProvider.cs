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
using System.Threading.Tasks;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Update.Config;
using ProtonVPN.Update.Feed;

namespace ProtonVPN.Core.Update
{
    public class FeedUrlProvider : IFeedUrlProvider, IVpnStateAware
    {
        private readonly IConfiguration _config;
        private FeedType _feedType = FeedType.Public;

        public event EventHandler<FeedUrlChangeEventArgs> FeedUrlChanged;

        public FeedUrlProvider(IConfiguration config)
        {
            _config = config;
        }

        public Uri GetFeedUrl()
        {
            if (_feedType is FeedType.Internal)
            {
                return new Uri(GlobalConfig.InternalReleaseUpdateUrl);
            }
            else
            {
                return new Uri(_config.Urls.UpdateUrl);
            }
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            FeedType type = e.State.Status == VpnStatus.Connected && e.State.Server.Tier == ServerTiers.Internal
                ? FeedType.Internal
                : FeedType.Public;

            if (_feedType != type)
            {
                _feedType = type;
                FeedUrlChanged?.Invoke(this, new FeedUrlChangeEventArgs(type));
            }
        }
    }
}