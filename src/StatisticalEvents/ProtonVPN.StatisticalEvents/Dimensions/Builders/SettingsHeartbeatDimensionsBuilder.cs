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

using System.Collections.Generic;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.StatisticalEvents.Dimensions.Extensions;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Settings;

namespace ProtonVPN.StatisticalEvents.Dimensions.Builders;

public class SettingsHeartbeatDimensionsBuilder : ISettingsHeartbeatDimensionsBuilder
{
    private readonly ISettings _settings;
    private readonly IBooleanDimensionMapper _booleanDimensionMapper;
    private readonly IVpnPlanTierDimensionMapper _vpnPlanTierDimensionMapper;
    private readonly IDefaultConnectionTypeDimensionMapper _defaultConnectionTypeDimensionMapper;
    private readonly ILanModeDimensionMapper _lanModeDimensionMapper;
    private readonly ISplitTunnelingModeDimensionMapper _splitTunnelingModeDimensionMapper;
    private readonly ISplitTunnelingAppsCountDimensionMapper _splitTunnelingAppsCountDimensionMapper;
    private readonly ISplitTunnelingIpsCountDimensionMapper _splitTunnelingIpsCountDimensionMapper;
    private readonly IWindowSizeCategoryDimensionMapper _windowSizeCategoryDimensionMapper;
    private readonly IUiThemeDimensionMapper _uiThemeDimensionMapper;
    private readonly INetShieldModeDimensionMapper _netShieldModeDimensionMapper;
    private readonly IKillSwitchModeDimensionMapper _killSwitchModeDimensionMapper;
    private readonly INatTypeDimensionMapper _natTypeDimensionMapper;
    private readonly IExcludedLocationsCountDimensionMapper _excludedLocationsCountDimensionMapper;

    public SettingsHeartbeatDimensionsBuilder(
        ISettings settings,
        IBooleanDimensionMapper booleanDimensionMapper,
        IVpnPlanTierDimensionMapper vpnPlanTierDimensionMapper,
        IDefaultConnectionTypeDimensionMapper defaultConnectionTypeDimensionMapper,
        ILanModeDimensionMapper lanModeDimensionMapper,
        ISplitTunnelingModeDimensionMapper splitTunnelingModeDimensionMapper,
        ISplitTunnelingAppsCountDimensionMapper splitTunnelingAppsCountDimensionMapper,
        ISplitTunnelingIpsCountDimensionMapper splitTunnelingIpsCountDimensionMapper,
        IWindowSizeCategoryDimensionMapper windowSizeCategoryDimensionMapper,
        IUiThemeDimensionMapper uiThemeDimensionMapper,
        INetShieldModeDimensionMapper netShieldModeDimensionMapper,
        IKillSwitchModeDimensionMapper killSwitchModeDimensionMapper,
        INatTypeDimensionMapper natTypeDimensionMapper,
        IExcludedLocationsCountDimensionMapper excludedLocationsCountDimensionMapper)
    {
        _settings = settings;
        _booleanDimensionMapper = booleanDimensionMapper;
        _vpnPlanTierDimensionMapper = vpnPlanTierDimensionMapper;
        _defaultConnectionTypeDimensionMapper = defaultConnectionTypeDimensionMapper;
        _lanModeDimensionMapper = lanModeDimensionMapper;
        _splitTunnelingModeDimensionMapper = splitTunnelingModeDimensionMapper;
        _splitTunnelingAppsCountDimensionMapper = splitTunnelingAppsCountDimensionMapper;
        _splitTunnelingIpsCountDimensionMapper = splitTunnelingIpsCountDimensionMapper;
        _windowSizeCategoryDimensionMapper = windowSizeCategoryDimensionMapper;
        _uiThemeDimensionMapper = uiThemeDimensionMapper;
        _netShieldModeDimensionMapper = netShieldModeDimensionMapper;
        _killSwitchModeDimensionMapper = killSwitchModeDimensionMapper;
        _natTypeDimensionMapper = natTypeDimensionMapper;
        _excludedLocationsCountDimensionMapper = excludedLocationsCountDimensionMapper;
    }

    public Dictionary<string, string> Build()
    {
        List<ExcludedLocation> excludedLocations = _settings.ExcludedLocationsList ?? [];

        Dictionary<string, string> dimensionDictionary = new()
        {
            { "is_auto_connect_enabled", _booleanDimensionMapper.Map(_settings.IsAutoConnectEnabled) },
            { "default_connection_type", _defaultConnectionTypeDimensionMapper.Map(_settings.DefaultConnection.Type) },
            { "ui_theme", _uiThemeDimensionMapper.Map(_settings.Theme) },
            { "user_tier", _vpnPlanTierDimensionMapper.Map(_settings.VpnPlan) },
            { "is_ipv6_enabled", _booleanDimensionMapper.Map(_settings.IsIpv6Enabled) },
            { "custom_dns_count", (_settings.CustomDnsServersList?.Count ?? 0).ToDnsCountDimension() },
            { "first_custom_dns_address_family", _settings.CustomDnsServersList.GetFirstActiveDnsFamily() },
            { "is_custom_dns_enabled", _booleanDimensionMapper.Map(_settings.IsCustomDnsServersEnabled) },
            { "lan_mode", _lanModeDimensionMapper.Map(_settings.IsLocalAreaNetworkAccessEnabled, _settings.IsLocalDnsEnabled) },
            { "is_port_forwarding_enabled", _booleanDimensionMapper.Map(_settings.IsPortForwardingEnabled) },
            { "is_split_tunneling_enabled", _booleanDimensionMapper.Map(_settings.IsSplitTunnelingEnabled) },
            { "split_tunneling_mode", _splitTunnelingModeDimensionMapper.Map(_settings.SplitTunnelingMode) },
            { "split_tunneling_apps_count", _splitTunnelingAppsCountDimensionMapper.Map(
                _settings.SplitTunnelingMode,
                _settings.SplitTunnelingStandardAppsList,
                _settings.SplitTunnelingInverseAppsList) },
            { "split_tunneling_ips_count", _splitTunnelingIpsCountDimensionMapper.Map(
                _settings.SplitTunnelingMode,
                _settings.SplitTunnelingStandardIpAddressesList,
                _settings.SplitTunnelingInverseIpAddressesList) },
            { "window_size_category", _windowSizeCategoryDimensionMapper.Map(_settings.WindowWidth, _settings.WindowHeight, _settings.IsWindowMaximized) },
            { "netshield_level", _netShieldModeDimensionMapper.Map(_settings.IsNetShieldEnabled, _settings.NetShieldMode) },
            { "kill_switch_level", _killSwitchModeDimensionMapper.Map(_settings.IsKillSwitchEnabled, _settings.KillSwitchMode) },
            { "nat_type", _natTypeDimensionMapper.Map(_settings.NatType) },
            { "excluded_countries_count", _excludedLocationsCountDimensionMapper.MapCountries(excludedLocations) },
            { "excluded_cities_count", _excludedLocationsCountDimensionMapper.MapCitiesAndStates(excludedLocations) },
        };

        return dimensionDictionary;
    }
}