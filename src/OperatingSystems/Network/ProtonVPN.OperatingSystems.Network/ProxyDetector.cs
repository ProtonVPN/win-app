/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.OperatingSystems.Network.Contracts;
using ProtonVPN.OperatingSystems.Registries.Contracts;

namespace ProtonVPN.OperatingSystems.Network;

public class ProxyDetector : IProxyDetector
{
    private const string REGISTRY_PATH = "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";
    private const string REGISTRY_KEY = "ProxyEnable";

    private readonly RegistryUri _registryUri = RegistryUri.CreateCurrentUserUri(REGISTRY_PATH, REGISTRY_KEY);

    private readonly IRegistryEditor _registryEditor;

    public ProxyDetector(IRegistryEditor registryEditor)
    {
        _registryEditor = registryEditor;
    }

    public bool IsEnabled()
    {
        return _registryEditor.ReadInt(_registryUri) == 1;
    }
}