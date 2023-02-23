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
using System.Collections.Generic;

namespace ProtonVPN.Core.Update
{
    public class UpdateStateChangedEventArgs : EventArgs
    {
        private readonly UpdateState _update;

        public UpdateStateChangedEventArgs(UpdateState update, bool manualCheck)
        {
            _update = update;
            ManualCheck = manualCheck;
        }

        public IReadOnlyList<Release> ReleaseHistory => _update.ReleaseHistory;
        public bool Available => _update.Available;
        public bool Ready => _update.Ready;
        public UpdateStatus Status => _update.Status;
        public bool ManualCheck { get; }
        public string FilePath => _update.FilePath;
        public string FileArguments => _update.FileArguments;
    }
}
