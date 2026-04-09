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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers;

namespace ProtonVPN.StatisticalEvents.Dimensions.Builders;

public class UpsellDimensionsBuilder : IUpsellDimensionsBuilder
{
    private const string EXTERNAL_FLOW_TYPE = "external";

    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly IModalSourceDimensionMapper _modalSourceDimensionMapper;
    private readonly IVpnPlanTierDimensionMapper _vpnPlanTierDimensionMapper;
    private readonly IVpnPlanNameDimensionMapper _vpnPlanNameDimensionMapper;
    private readonly IDaysSinceAccountCreationDimensionMapper _daysSinceAccountCreationDimensionMapper;
    private readonly IOnOffDimensionMapper _onOffDimensionMapper;
    private readonly IYesNoDimensionMapper _yesNoDimensionMapper;
    private readonly IStringDimensionMapper _stringDimensionMapper;
    private readonly IPromoUrlDimensionMapper _promoUrlDimensionMapper;

    public UpsellDimensionsBuilder(
        ISettings settings,
        IConnectionManager connectionManager,
        IModalSourceDimensionMapper modalSourceDimensionMapper,
        IVpnPlanTierDimensionMapper vpnPlanTierDimensionMapper,
        IVpnPlanNameDimensionMapper vpnPlanNameDimensionMapper,
        IDaysSinceAccountCreationDimensionMapper daysSinceAccountCreationDimensionMapper,
        IOnOffDimensionMapper onOffDimensionMapper,
        IYesNoDimensionMapper yesNoDimensionMapper,
        IStringDimensionMapper stringDimensionMapper,
        IPromoUrlDimensionMapper promoUrlDimensionMapper)
    {
        _settings = settings;
        _connectionManager = connectionManager;
        _modalSourceDimensionMapper = modalSourceDimensionMapper;
        _vpnPlanTierDimensionMapper = vpnPlanTierDimensionMapper;
        _vpnPlanNameDimensionMapper = vpnPlanNameDimensionMapper;
        _daysSinceAccountCreationDimensionMapper = daysSinceAccountCreationDimensionMapper;
        _onOffDimensionMapper = onOffDimensionMapper;
        _yesNoDimensionMapper = yesNoDimensionMapper;
        _stringDimensionMapper = stringDimensionMapper;
        _promoUrlDimensionMapper = promoUrlDimensionMapper;
    }

    public Dictionary<string, string> Build(ModalSource modalSource, string? reference = null)
    {
        string? deviceCountryLocation = _settings.DeviceLocation?.CountryCode;
        DateTimeOffset? accountCreationDateUtc = _settings.UserCreationDateUtc;

        return new()
        {
            { "modal_source", _modalSourceDimensionMapper.Map(modalSource) },
            { "vpn_status", _onOffDimensionMapper.Map(_connectionManager.IsConnected) },
            { "user_country", _stringDimensionMapper.Map(deviceCountryLocation) },
            { "new_free_plan_ui", _yesNoDimensionMapper.Map(true) },
            { "days_since_account_creation", _daysSinceAccountCreationDimensionMapper.Map(accountCreationDateUtc) },
            { "reference", _stringDimensionMapper.Map(reference) },
            { "is_credential_less_enabled", _onOffDimensionMapper.Map(false) },
            { "flow_type", EXTERNAL_FLOW_TYPE }
        };
    }
    public Dictionary<string, string> BuildAttemptDimensions()
    {
        return BuildAttemptOrDisplayDimensions();
    }

    public Dictionary<string, string> BuildDisplayDimensions()
    {
        return BuildAttemptOrDisplayDimensions();
    }

    private Dictionary<string, string> BuildAttemptOrDisplayDimensions()
    {
        VpnPlan vpnPlan = _settings.VpnPlan;

        return new()
        {
            { "user_plan", _vpnPlanNameDimensionMapper.Map(vpnPlan) },
            { "user_tier", _vpnPlanTierDimensionMapper.Map(vpnPlan) },
        };
    }

    public Dictionary<string, string> BuildSuccessDimensions(string url, VpnPlan oldPlan, VpnPlan newPlan)
    {
        Uri? promoUri = null;
        if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out Uri? absoluteUri))
        {
            promoUri = absoluteUri;
        }

        return new()
        {
            { "user_plan", _vpnPlanNameDimensionMapper.Map(oldPlan) },
            { "user_tier", _vpnPlanTierDimensionMapper.Map(oldPlan) },
            { "upgraded_user_plan", _vpnPlanNameDimensionMapper.Map(newPlan) },
            { "upgraded_user_tier", _vpnPlanTierDimensionMapper.Map(newPlan) },
            { "billing_cycle", _promoUrlDimensionMapper.MapBillingCycle(promoUri) },
            { "coupon_code", _promoUrlDimensionMapper.MapCouponCode(promoUri) },
        };
    }
}