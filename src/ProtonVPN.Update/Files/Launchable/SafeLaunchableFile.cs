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

using System;
using ProtonVPN.Update.Helpers;

namespace ProtonVPN.Update.Files.Launchable
{
    /// <summary>
    /// Wraps expected exceptions of <see cref="LaunchableFile"/> into <see cref="AppUpdateException"/>.
    /// </summary>
    public class SafeLaunchableFile : ILaunchableFile
    {
        private readonly ILaunchableFile _origin;

        public SafeLaunchableFile(ILaunchableFile origin)
        {
            _origin = origin;
        }

        public void Launch(string filename, string arguments)
        {
            try
            {
                _origin.Launch(filename, arguments);
            }
            catch (Exception e) when (e.IsProcessException())
            {
                throw new AppUpdateException("Failed to launch update installer", e);
            }
        }
    }
}
