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

using ProtonVPN.Api.Contracts.Common;

namespace ProtonVPN.Api.Contracts.VpnConfig
{
    public class VpnConfigResponse : BaseResponse
    {
        public DefaultPortsResponse DefaultPorts { get; set; }

        public int? ServerRefreshInterval { get; set; }

        public ConfigFlagsResponse FeatureFlags { get; set; }

        public SmartProtocolResponse SmartProtocol { get; set; }

        public int ChangeServerAttemptLimit { get; set; }

        public int ChangeServerShortDelayInSeconds { get; set; }

        public int ChangeServerLongDelayInSeconds { get; set; }
    }
}