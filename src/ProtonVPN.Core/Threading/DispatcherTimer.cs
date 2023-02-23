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
using ProtonVPN.Common.Threading;

namespace ProtonVPN.Core.Threading
{
    internal class DispatcherTimer : ISchedulerTimer
    {
        private System.Windows.Threading.DispatcherTimer _timer;

        internal DispatcherTimer(Dispatcher dispatcher)
        {
            _timer = new System.Windows.Threading.DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Tick += Timer_Tick;
        }

        public TimeSpan Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public bool IsEnabled
        {
            get => _timer.IsEnabled;
            set => _timer.IsEnabled = value;
        }

        public event EventHandler Tick;

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();

        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
            _timer = null;
            Tick = null;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_timer.IsEnabled)
                Tick?.Invoke(sender, e);
        }
    }
}