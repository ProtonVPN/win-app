/*
 * Copyright (c) 2023 Proton AG
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

using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Configurations;

public class StaticConfiguration : IStaticConfiguration
{
    private readonly DefaultConfiguration _default = new();

    public StaticConfiguration()
    {
        _serviceName = new(() => _default.ServiceName);
        _clientLogsFullFilePath = new(() => _default.ClientLogsFilePath);
        _serviceLogsFullFilePath = new(() => _default.ServiceLogsFilePath);
    }

    private readonly Lazy<string> _serviceName;
    public string ServiceName => _serviceName.Value;

    private readonly Lazy<string> _clientLogsFullFilePath;
    public string ClientLogsFilePath => _clientLogsFullFilePath.Value;

    private readonly Lazy<string> _serviceLogsFullFilePath;
    public string ServiceLogsFilePath => _serviceLogsFullFilePath.Value;
}