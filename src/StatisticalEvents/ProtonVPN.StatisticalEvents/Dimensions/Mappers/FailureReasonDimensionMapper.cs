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

using ProtonVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace ProtonVPN.StatisticalEvents.Dimensions.Mappers;

public class FailureReasonDimensionMapper : DimensionMapperBase, IFailureReasonDimensionMapper
{
    private const int LOCAL_AGENT_ERROR_CODE_START = 86100;
    private const int LOCAL_AGENT_ERROR_CODE_END = 86999;

    public string Map(int? failureCode)
    {
        return failureCode switch
        {
            int code when IsLocalAgentError(code) => code.ToString(),
            _ => NOT_AVAILABLE
        };
    }

    private bool IsLocalAgentError(int failureCode)
    {
        return failureCode >= LOCAL_AGENT_ERROR_CODE_START 
            && failureCode <= LOCAL_AGENT_ERROR_CODE_END;
    }
}