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
using System.Threading;
using System.Threading.Tasks;

namespace ProtonVPN.Update.Updates
{
    /// <summary>
    /// Marshals app update state changed notifications of <see cref="INotifyingAppUpdate"/>
    /// to synchronization context captured during creation of the object.
    /// </summary>
    public class SynchronizedAppUpdate : INotifyingAppUpdate
    {
        private readonly INotifyingAppUpdate _origin;
        private readonly SynchronizationContext _syncContext;

        public SynchronizedAppUpdate(INotifyingAppUpdate origin)
        {
            _origin = origin;

            _syncContext = SynchronizationContext.Current;
            _origin.StateChanged += AppUpdate_StateChanged;
        }

        public event EventHandler<IAppUpdateState> StateChanged;

        public void StartCheckingForUpdate(bool earlyAccess) => _origin.StartCheckingForUpdate(earlyAccess);

        public Task StartUpdating(bool auto) => _origin.StartUpdating(auto);

        private void AppUpdate_StateChanged(object sender, IAppUpdateState state)
        {
            _syncContext.Post(_ => StateChanged?.Invoke(sender, state), null);
        }
    }
}
