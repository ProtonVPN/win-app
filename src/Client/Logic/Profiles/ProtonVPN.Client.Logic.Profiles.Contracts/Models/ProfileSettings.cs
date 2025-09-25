/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Profiles.Contracts.Models;

public class ProfileSettings : IProfileSettings
{
    public static IProfileSettings Default => new ProfileSettings()
    {
        VpnProtocol = DefaultSettings.VpnProtocol,
        IsNetShieldEnabled = DefaultSettings.IsNetShieldEnabled(true),
        NetShieldMode = DefaultSettings.NetShieldMode,
        IsPortForwardingEnabled = DefaultSettings.IsPortForwardingEnabled,
        NatType = DefaultSettings.NatType,
    };

    public VpnProtocol VpnProtocol { get; set; }

    public bool IsNetShieldEnabled { get; set; }

    public NetShieldMode NetShieldMode { get; set; }

    public bool IsPortForwardingEnabled { get; set; }

    public NatType NatType { get; set; }

    public bool? IsCustomDnsServersEnabled => IsNetShieldEnabled ? false : null;

    public IProfileSettings Copy()
    {
        return (IProfileSettings)MemberwiseClone();
    }
}