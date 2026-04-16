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
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers;

namespace ProtonVPN.StatisticalEvents.Dimensions.Builders;

public class ProductPromptDimensionsBuilder : IProductPromptDimensionsBuilder
{
    private readonly ISettings _settings;
    private readonly IVpnPlanTierDimensionMapper _vpnPlanTierDimensionMapper;
    private readonly IPromptTypeDimensionMapper _promptTypeDimensionMapper;
    private readonly IPromptContextDimensionMapper _promptContextDimensionMapper;
    private readonly IPromptActionDimensionMapper _promptActionDimensionMapper;

    public ProductPromptDimensionsBuilder(
        ISettings settings,
        IVpnPlanTierDimensionMapper vpnPlanTierDimensionMapper,
        IPromptTypeDimensionMapper promptTypeDimensionMapper,
        IPromptContextDimensionMapper promptContextDimensionMapper,
        IPromptActionDimensionMapper promptActionDimensionMapper)
    {
        _settings = settings;
        _vpnPlanTierDimensionMapper = vpnPlanTierDimensionMapper;
        _promptTypeDimensionMapper = promptTypeDimensionMapper;
        _promptContextDimensionMapper = promptContextDimensionMapper;
        _promptActionDimensionMapper = promptActionDimensionMapper;
    }

    public Dictionary<string, string> Build(PromptType promptType, PromptContext promptContext)
    {
        return new Dictionary<string, string>
        {
            { "user_tier", _vpnPlanTierDimensionMapper.Map(_settings.VpnPlan) },
            { "prompt_type", _promptTypeDimensionMapper.Map(promptType) },
            { "prompt_context", _promptContextDimensionMapper.Map(promptContext) }
        };
    }

    public Dictionary<string, string> BuildAction(PromptAction promptAction)
    {
        return new Dictionary<string, string>
        {
            { "prompt_action", _promptActionDimensionMapper.Map(promptAction) }
        };
    }
}