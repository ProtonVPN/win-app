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

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ProtonVPN.ProTun.Generated;

public static class ProTunDllLoader
{
    private const string DLL_NAME = "protun";

    private static readonly ConcurrentDictionary<string, IntPtr> _cache = new();

    public static void Register()
    {
        NativeLibrary.SetDllImportResolver(typeof(ProTunApi).Assembly, ImportResolver);
    }

    private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return libraryName == DLL_NAME ? _cache.GetOrAdd(libraryName, CreateDllHandle) : IntPtr.Zero;
    }

    private static IntPtr CreateDllHandle(string arg)
    {
        string architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => "arm64",
            _ => "x64"
        };

        string libPath = Path.Combine(AppContext.BaseDirectory, architecture, $"{DLL_NAME}.dll");
        return NativeLibrary.Load(libPath);
    }
}
