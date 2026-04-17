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

using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class PromptTypeDimensionMapper : DimensionMapperBase, IPromptTypeDimensionMapper
{
    private const string FEATURE_DISCOVERY = "feature_discovery";
    private const string ANNOUNCEMENT = "announcement";
    private const string EDUCATION = "education";

    public string Map(PromptType promptType)
    {
        return promptType switch
        {
            PromptType.FeatureDiscovery => FEATURE_DISCOVERY,
            PromptType.Announcement => ANNOUNCEMENT,
            PromptType.Education => EDUCATION,
            _ => NOT_AVAILABLE
        };
    }
}
