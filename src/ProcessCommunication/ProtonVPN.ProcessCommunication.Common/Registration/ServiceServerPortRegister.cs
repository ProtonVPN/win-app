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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.OperatingSystems.Registries.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Registration;

namespace ProtonVPN.ProcessCommunication.Common.Registration;

public class ServiceServerPortRegister : ServerPortRegisterBase, IServiceServerPortRegister
{
    private const string KEY = "ServiceServerPort";

    private static readonly TimeSpan DELAY = TimeSpan.FromMilliseconds(100);

    public ServiceServerPortRegister(IRegistryEditor registryEditor, ILogger logger)
        : base(registryEditor, logger)
    {
    }

    protected override string GetKey()
    {
        return KEY;
    }

    public async Task<int> ReadAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            int? serverBoundPort = ReadOnce();
            if (serverBoundPort.HasValue && serverBoundPort.Value > 0)
            {
                return serverBoundPort.Value;
            }
            await Task.Delay(DELAY, cancellationToken);
        }
    }
}