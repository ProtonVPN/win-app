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

using System.Collections.Generic;
using ProtonVPN.StatisticalEvents.Dimensions.Mappers;

namespace ProtonVPN.StatisticalEvents.Dimensions.Builders;

public class ClientInstallsDimensionsBuilder : IClientInstallsDimensionsBuilder
{
    private const string WINDOWS_CLIENT = "windows";
    private const string VPN_PRODUCT = "vpn";

    private readonly IBooleanDimensionMapper _booleanDimensionMapper;

    public ClientInstallsDimensionsBuilder(
        IBooleanDimensionMapper booleanDimensionMapper)
    {
        _booleanDimensionMapper = booleanDimensionMapper;
    }

    public Dictionary<string, string> Build(bool isMailInstalled, bool isDriveInstalled, bool isPassInstalled)
    {
        return new Dictionary<string, string>
        {
            { "client", WINDOWS_CLIENT },
            { "product", VPN_PRODUCT },
            { "is_mail_installed", _booleanDimensionMapper.Map(isMailInstalled) },
            { "is_drive_installed", _booleanDimensionMapper.Map(isDriveInstalled) },
            { "is_pass_installed", _booleanDimensionMapper.Map(isPassInstalled) },
        };
    }
}