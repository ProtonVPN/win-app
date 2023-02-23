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

using System.Diagnostics;

namespace ProtonVPN.Common.OS.Processes
{
    public static class ProcessStartInfoExtensions
    {
        public  static ProcessStartInfo StandardInfo(this ProcessStartInfo info)
        {
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardInput = true;

            return info;
        }

        public static ProcessStartInfo NoRedirectInfo(this ProcessStartInfo info)
        {
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            return info;
        }

        public static ProcessStartInfo ElevatedInfo(this ProcessStartInfo info)
        {
            info.CreateNoWindow = true;
            info.UseShellExecute = true;
            info.RedirectStandardOutput = false;
            info.Verb = "runas";

            return info;
        }
    }
}
