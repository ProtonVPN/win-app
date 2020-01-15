/*
 * Copyright (c) 2020 Proton Technologies AG
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

using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Core.Startup
{
    internal class SyncableAutoStartup : ISyncableAutoStartup
    {
        private readonly IAppSettings _appSettings;
        private readonly IScheduler _scheduler;
        private readonly IAutoStartup _autoStartup;

        private bool _syncing;

        public SyncableAutoStartup(
            IAppSettings appSettings,
            IScheduler scheduler,
            IAutoStartup autoStartup)
        {
            _appSettings = appSettings;
            _scheduler = scheduler;
            _autoStartup = autoStartup;
        }

        public void Sync()
        {
            if (_syncing)
                return;

            SyncForward();

            if (SyncBackRequired())
                _scheduler.Schedule(SyncBack);
        }

        private void SyncForward()
        {
            _autoStartup.Enabled = _appSettings.StartOnStartup;
        }

        private bool SyncBackRequired()
        {
            return _autoStartup.Enabled != _appSettings.StartOnStartup;
        }

        private void SyncBack()
        {
            _syncing = true;

            _appSettings.StartOnStartup = _autoStartup.Enabled;

            _syncing = false;
        }
    }
}
