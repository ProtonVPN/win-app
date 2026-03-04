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

namespace ProtonVPN.Client.Logic.Users.Contracts.Messages;

public readonly struct VpnPlan
{
    public static VpnPlan Default => new(null, null, 0);

    public string? Title { get; }
    public string? Name { get; }
    public bool IsPaid { get; }
    public sbyte MaxTier { get; }

    public VpnPlan(string? title, string? name, sbyte maxTier)
    {
        Title = title;
        Name = name;
        IsPaid = maxTier > 0;
        MaxTier = maxTier;
    }

    public bool IsPlus => Name is "vpnplus" or "vpn2022" or "vpnpass2023" or "vpn2024";

    public bool IsDuo => Name is "duo2024";

    public bool IsFamily => Name is "family2022";

    public bool IsUnlimited => Name == "bundle2022";

    public bool IsVisionary => Name == "visionary2022";


    public bool IsBusinessVpn => Name is "vpnpro2023" or "vpnbiz2023" or "vpnpassbiz2025";

    public bool IsBusinessBundle => Name is "bundlepro2022" or "bundlepro2024" or "bundlebiz2025";


    public bool IsDefault => Title is null && Name is null && MaxTier == 0;

    public bool IsB2B => IsBusinessVpn || IsBusinessBundle;

    public bool IsVpnPlan => IsPlus || IsBusinessVpn;

    public bool IsProtonPlan => IsDuo || IsFamily || IsUnlimited || IsVisionary || IsBusinessBundle;
}