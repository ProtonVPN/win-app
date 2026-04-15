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
    public static VpnPlan Default => new(string.Empty, string.Empty, 0, false);

    public string Title { get; }
    public string Name { get; }
    public bool IsPaid { get; }
    public sbyte MaxTier { get; }
    public bool IsB2B { get; }

    public VpnPlan(string title, string name, sbyte maxTier, bool isB2B)
    {
        Title = title ?? string.Empty;
        Name = name ?? string.Empty;
        IsPaid = maxTier > 0;
        MaxTier = maxTier;
        IsB2B = isB2B;
    }

    public bool IsDefaultPlan => string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Name) && MaxTier == 0;

    public bool IsFreePlan => !IsDefaultPlan && !IsPaid;

    public bool IsVpnPlan => !IsDefaultPlan && IsPaid && Name.Contains("vpn", StringComparison.OrdinalIgnoreCase);

    public bool IsProtonPlan => !IsDefaultPlan && IsPaid && !IsVpnPlan;

    public bool IsPaidB2CPlan => !IsDefaultPlan && IsPaid && !IsB2B;
}