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

using System;
using System.Collections.Generic;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class PromoUrlDimensionMapper : DimensionMapperBase, IPromoUrlDimensionMapper
{
    private const string COUPON_PARAM = "coupon";
    private const string CYCLE_PARAM = "cycle";

    private static readonly HashSet<string> _allowedBillingCycles = new(StringComparer.Ordinal)
    {
        "1",
        "3",
        "12",
        "15",
        "18",
        "24",
        "30"
    };

    public string MapBillingCycle(Uri? promoUri)
    {
        if (promoUri is null || string.IsNullOrEmpty(promoUri.Query))
        {
            return NOT_AVAILABLE;
        }

        if (!TryGetQueryParameter(promoUri.Query, CYCLE_PARAM, out string? cycle) || string.IsNullOrEmpty(cycle))
        {
            return NOT_AVAILABLE;
        }

        return _allowedBillingCycles.Contains(cycle) ? cycle : NOT_AVAILABLE;
    }

    public string MapCouponCode(Uri? promoUri)
    {
        if (promoUri is null || string.IsNullOrEmpty(promoUri.Query))
        {
            return NOT_AVAILABLE;
        }

        if (!TryGetQueryParameter(promoUri.Query, COUPON_PARAM, out string? coupon) || string.IsNullOrEmpty(coupon))
        {
            return NOT_AVAILABLE;
        }

        return coupon;
    }

    private static bool TryGetQueryParameter(string query, string key, out string? value)
    {
        value = null;
        if (string.IsNullOrEmpty(query))
        {
            return false;
        }

        string trimmed = query.StartsWith('?') ? query[1..] : query;

        foreach (string part in trimmed.Split('&'))
        {
            int index = part.IndexOf('=');
            if (index <= 0)
            {
                continue;
            }

            string name = Uri.UnescapeDataString(part[..index]);
            if (!name.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            value = Uri.UnescapeDataString(part[(index + 1)..]);
            return true;
        }

        return false;
    }
}
