/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.StatisticalEvents.DimensionBuilders;

public class UpsellDimensionBuilder : IUpsellDimensionBuilder
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;

    public UpsellDimensionBuilder(ISettings settings, IConnectionManager connectionManager)
    {
        _settings = settings;
        _connectionManager = connectionManager;
    }

    public Dictionary<string, string> Build(ModalSource modalSource, string? reference = null)
    {
        VpnPlan vpnPlan = _settings.VpnPlan;
        string? country = _settings.DeviceLocation?.CountryCode;
        return new()
        {
            { "modal_source", ModalSourceTranslator.Translate(modalSource) },
            { "user_plan", GetUserPlan(vpnPlan) },
            { "user_tier", GetUserTier(vpnPlan) },
            { "vpn_status", _connectionManager.IsConnected ? "on" : "off" },
            { "user_country", string.IsNullOrWhiteSpace(country) ? "n/a" : country },
            { "new_free_plan_ui", "yes" },
            { "days_since_account_creation", GetDaysSinceAccountCreation() },
            { "reference", string.IsNullOrWhiteSpace(reference) ? "n/a" : reference },
            { "is_credential_less_enabled", "off" },
            { "flow_type", "external" }
        };
    }

    private string GetDaysSinceAccountCreation()
    {
        DateTimeOffset? creationDateUtc = _settings.UserCreationDateUtc;
        DateTime nowUtc = DateTime.UtcNow;
        if (creationDateUtc is null)
        {
            return "n/a";
        }
        if (nowUtc <= creationDateUtc.Value)
        {
            return "0";
        }

        long daysSinceAccountCreation = (long)(nowUtc - creationDateUtc.Value).TotalDays;
        return daysSinceAccountCreation <= 0
            ? "0"
            : daysSinceAccountCreation <= 3
            ? "1-3"
            : daysSinceAccountCreation <= 7
            ? "4-7"
            : daysSinceAccountCreation <= 14
            ? "8-14"
            : ">14";
    }

    private string GetUserTier(VpnPlan vpnPlan)
    {
        return vpnPlan.MaxTier switch
        {
            0 => "free",
            1 or 2 => "paid",
            3 => "internal",
            _ => "n/a"
        };
    }

    public string GetUserPlan(VpnPlan vpnPlan)
    {
        return vpnPlan.Title ?? "n/a";
    }
}