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
using ProtonVPN.Builds.Variables;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Update.Contracts;
using ProtonVPN.Update.Contracts.Config;

namespace ProtonVPN.Service.Update
{
    public class FeedUrlProvider : IFeedUrlProvider
    {
        private readonly IConfiguration _config;
        private FeedType _feedType = FeedType.Public;

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

            return new Uri(_config.Urls.UpdateUrl);
        }

        public void SetFeedType(FeedType feedType)
        {
            _feedType = feedType;
        }
    }
}