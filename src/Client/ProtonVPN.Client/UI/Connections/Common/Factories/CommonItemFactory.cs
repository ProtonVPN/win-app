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

using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.UI.Connections.Common.Items;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Connections.Common.Factories;

public class CommonItemFactory
{
    private readonly ILocalizationProvider _localizer;

    public CommonItemFactory(
        ILocalizationProvider localizer)
    {
        _localizer = localizer;
    }

    public ProtocolItem GetProtocol(VpnProtocol protocol)
    {
        return new ProtocolItem(_localizer, protocol);
    }

    public FeatureItem GetFeature(Feature feature)
    {
        return new FeatureItem(_localizer, feature);
    }
}