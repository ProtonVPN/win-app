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

using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Models;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Settings;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Factories;

public class CommonItemFactory : ICommonItemFactory
{
    private readonly ILocalizationProvider _localizer;
    private readonly IApplicationThemeSelector _themeSelector;

    public CommonItemFactory(
        ILocalizationProvider localizer,
        IApplicationThemeSelector themeSelector)
    {
        _localizer = localizer;
        _themeSelector = themeSelector;
    }

    public ProtocolItem GetProtocol(VpnProtocol protocol)
    {
        return new ProtocolItem(_localizer, protocol);
    }

    public NetShieldModeItem GetNetShieldMode(NetShieldMode? netShieldMode)
    {
        return GetNetShieldMode(netShieldMode.HasValue, netShieldMode.GetValueOrDefault(DefaultSettings.NetShieldMode));
    }

    public NetShieldModeItem GetNetShieldMode(bool isEnabled, NetShieldMode netShieldMode)
    {
        return new NetShieldModeItem(_localizer, _themeSelector, isEnabled, netShieldMode);
    }

    public NatTypeItem GetNatType(NatType natType)
    {
        return new NatTypeItem(_localizer, natType);
    }

    public PortForwardingItem GetPortForwardingMode(bool isEnabled)
    {
        return new PortForwardingItem(_localizer, _themeSelector, isEnabled);
    }

    public FeatureItem GetFeature(Feature feature)
    {
        return new FeatureItem(_localizer, _themeSelector, feature);
    }
}