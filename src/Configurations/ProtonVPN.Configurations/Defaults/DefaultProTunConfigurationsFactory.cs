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

using ProtonVPN.Configurations.Contracts.Entities;
using ProtonVPN.Configurations.Entities;

namespace ProtonVPN.Configurations.Defaults;

public static class DefaultProTunConfigurationsFactory
{
    public static IProTunConfigurations Create()
    {
        return new ProTunConfigurations
        {
            WintunAdapterGuid = Guid.Parse("{9BEB3451-4026-4F8A-8762-8F608B124FEC}"),
            WintunAdapterHardwareId = "Wintun",
        };
    }
}