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

using System;
using System.IO;

namespace ProtonVPN.Client.Localization.Tests.Helpers;

public static class SourcePathResolver
{
    private static readonly Lazy<string> _solutionRoot = new(FindSolutionRoot);

    private static string SolutionRoot => _solutionRoot.Value;

    public static string SourceRoot => Path.Combine(SolutionRoot, "src");

    public static string LocalizationStringsRoot => 
        Path.Combine(SourceRoot, "Client", "Localization", "ProtonVPN.Client.Localization", "Strings");

    public static string EnUsReswPath =>
        Path.Combine(LocalizationStringsRoot, "en-US", "Resources.resw");

    private static string FindSolutionRoot()
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "ProtonVPN.slnx")))
            {
                return dir;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        return dir ?? throw new InvalidOperationException(
            "Could not resolve solution root directory.");
    }
}