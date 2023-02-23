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

namespace ProtonVPN.Core.Update
{
    public class UpdateState
    {
        public UpdateState(
            IReadOnlyList<Release> releaseHistory,
            bool available,
            bool ready,
            UpdateStatus status,
            string filePath,
            string fileArguments)
        {
            ReleaseHistory = releaseHistory;
            Available = available;
            Ready = ready;
            Status = status;
            FilePath = filePath;
            FileArguments = fileArguments;
        }

        public IReadOnlyList<Release> ReleaseHistory { get; }

        public bool Available { get; }

        public bool Ready { get; }

        public UpdateStatus Status { get; }

        public string FilePath { get; }

        public string FileArguments { get; }
    }
}