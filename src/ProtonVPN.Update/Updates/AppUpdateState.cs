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

using System.Collections.Generic;

namespace ProtonVPN.Update.Updates
{
    /// <summary>
    /// Public app update state.
    /// </summary>
    internal class AppUpdateState : IAppUpdateState
    {
        internal AppUpdateState(IBaseAppUpdateState update, AppUpdateStatus status)
        {
            ReleaseHistory = update.ReleaseHistory();
            Available = update.Available;
            Ready = update.Ready;
            Status = status;
            FilePath = update.FilePath ?? string.Empty;
            FileArguments = update.FileArguments ?? string.Empty;
        }

        public IReadOnlyList<IRelease> ReleaseHistory { get; }
        public bool Available { get; }
        public bool Ready { get; }
        public AppUpdateStatus Status { get; }
        public string FilePath { get; }
        public string FileArguments { get; }
    }
}
