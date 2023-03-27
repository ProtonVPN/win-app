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

using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppUpdateLogs;

namespace ProtonVPN.Update.Updates
{
    /// <summary>
    /// Suppresses and logs expected exceptions of <see cref="AppUpdates"/>.
    /// </summary>
    public class SafeAppUpdates : IAppUpdates
    {
        private readonly ILogger _logger;
        private readonly IAppUpdates _origin;

        public SafeAppUpdates(ILogger logger, IAppUpdates origin)
        {
            Ensure.NotNull(logger, nameof(logger));
            Ensure.NotNull(origin, nameof(origin));

            _logger = logger;
            _origin = origin;
        }

        public void Cleanup()
        {
            try
            {
                _origin.Cleanup();
            }
            catch (AppUpdateException e)
            {
                _logger.Error<AppUpdateLog>("Error when deleting old update files.", e);
            }
        }
    }
}
