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

using Microsoft.Win32;

namespace ProtonVPN.OperatingSystems.Registries.Contracts;

public class RegistryUri
{
    public RegistryHive HiveKey { get; }
    public string Path { get; }
    public string Key { get; }

    private RegistryUri(RegistryHive hiveKey, string path, string key)
    {
        HiveKey = hiveKey;
        Path = path;
        Key = key;
    }

    public static RegistryUri CreateLocalMachineUri(string path, string key)
        => new(RegistryHive.LocalMachine, path: path, key: key);

    public static RegistryUri CreateCurrentUserUri(string path, string key)
        => new(RegistryHive.CurrentUser, path: path, key: key);

    public static RegistryUri CreateClassesRootUri(string path, string key)
        => new(RegistryHive.ClassesRoot, path: path, key: key);

    public override string ToString()
    {
        return $"'{HiveKey}':'{Path}':'{Key}'";
    }
}