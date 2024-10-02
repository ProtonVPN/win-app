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

using ProtonVPN.OperatingSystems.Registries.Contracts;

namespace ProtonVPN.ProcessCommunication.Common.Tests.Mocks;

public class MockOfRegistryEditor : IRegistryEditor
{
    private readonly Dictionary<string, object> _registry = new();

    public int? ReadInt(RegistryUri uri)
    {
        return _registry.TryGetValue(uri.ToString(), out object value) ? (int?)value : null;
    }

    public object ReadObject(RegistryUri uri)
    {
        return _registry.TryGetValue(uri.ToString(), out object value) ? value : null;
    }

    public string ReadString(RegistryUri uri)
    {
        return _registry.TryGetValue(uri.ToString(), out object value) ? (string)value : null;
    }

    public bool WriteInt(RegistryUri uri, int value)
    {
        return WriteObject(uri, value);
    }

    private bool WriteObject(RegistryUri uri, object value)
    {
        if (_registry.ContainsKey(uri.ToString()))
        {
            _registry.Remove(uri.ToString());
        }

        return _registry.TryAdd(uri.ToString(), value);
    }

    public bool WriteString(RegistryUri uri, string value)
    {
        return WriteObject(uri, value);
    }

    public bool? Delete(RegistryUri uri)
    {
        return _registry.ContainsKey(uri.ToString()) ? _registry.Remove(uri.ToString()) : null;
    }
}