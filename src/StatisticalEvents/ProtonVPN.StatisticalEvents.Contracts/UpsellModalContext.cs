/*
 * Copyright (c) 2026 Proton AG
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

namespace ProtonVPN.StatisticalEvents.Contracts;

public readonly struct UpsellModalContext
{
    public static UpsellModalContext Undefined => new();

    public ModalSource? Source { get; init; }
    public ModalTrigger? Trigger { get; init; }
    public string? CountryCode { get; init; }

    public UpsellModalContext(ModalSource source, ModalTrigger trigger, string? countryCode = null)
    {
        Source = source;
        Trigger = trigger;
        CountryCode = countryCode;
    }
}
