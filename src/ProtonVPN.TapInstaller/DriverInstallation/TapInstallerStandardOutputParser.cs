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

using System.Text.RegularExpressions;
using TapInstaller.Extensions;

namespace TapInstaller.DriverInstallation
{
    public class TapInstallerStandardOutputParser
    {
        private const string NoDeviceFound = "No matching devices found";
        private const string DeviceWasFound = " matching device(s) found";
        private const string DeviceHasAProblem = "The device has the following problem";
        private const string DeviceIsStopped = "Device is currently stopped";
        private const string DeviceIsDisabled = "Device is disabled";

        public DriverState ParseInstallerStatus(string standardOutput)
        {
            if (standardOutput.ContainsIgnoringCase(DeviceHasAProblem))
                return DriverState.DeviceHasAProblem;

            if (standardOutput.ContainsIgnoringCase(DeviceIsDisabled))
                return DriverState.DeviceIsDisabled;

            if (standardOutput.ContainsIgnoringCase(DeviceIsStopped))
                return DriverState.DeviceIsStopped;

            if (standardOutput.Contains(NoDeviceFound))
                return DriverState.NoDeviceFound;

            if (standardOutput.Contains(DeviceWasFound))
                return DriverState.DeviceExists;

            return DriverState.Unknown;
        }

        public int? ParseDeviceCode(string standardOutput)
        {
            var match = Regex.Match(standardOutput, ": ?(\\d+)");

            if (!match.Success)
                return null;

            if (int.TryParse(match.Groups[1].Value, out int code))
                return code;

            return null;
        }
    }
}
