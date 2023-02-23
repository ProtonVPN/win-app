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
using System.Windows.Threading;
using ProtonVPN.Core.Auth;

namespace ProtonVPN.Core.Events
{
    public class EventTimer : ILogoutAware
    {
        private readonly DispatcherTimer _timer;
        private readonly EventClient _eventClient;

        public EventTimer(EventClient eventClient, TimeSpan interval)
        {
            _eventClient = eventClient;

            _timer = new DispatcherTimer
            {
                Interval = interval
            };
            _timer.Tick += OnTimerTick;
        }

        public void OnUserLoggedOut()
        {
            Stop();
        }

        public void Start()
        {
            _timer.Start();
        }

        private void Stop()
        {
            _timer.Stop();
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            await _eventClient.GetEvents();
        }
    }
}
