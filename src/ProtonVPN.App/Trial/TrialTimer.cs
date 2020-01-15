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

using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;

namespace ProtonVPN.Trial
{
    public class TrialTimer : ILogoutAware, ITrialStateAware
    {
        private readonly DispatcherTimer _timer;
        private readonly IUserStorage _userStorage;

        public TrialTimer(IUserStorage userStorage)
        {
            _userStorage = userStorage;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTrialSecondElapsed;
        }

        public event EventHandler<TrialTickEventArgs> TrialTimerTicked;

        public Task OnTrialStateChangedAsync(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.TrialStarted:
                    Start();
                    break;
                case PlanStatus.Expired:
                case PlanStatus.Paid:
                    Stop();
                    break;
            }

            return Task.CompletedTask;
        }

        public void OnUserLoggedOut()
        {
            Stop();
        }

        private void Start()
        {
            _timer.Start();
        }

        private void Stop()
        {
            _timer.Stop();
        }

        private void OnTrialSecondElapsed(object sender, EventArgs e)
        {
            var user = _userStorage.User();
            var secondsLeft = user.TrialExpirationTimeInSeconds();
            TrialTimerTicked?.Invoke(this, new TrialTickEventArgs(secondsLeft));

            if (secondsLeft <= 0)
                Stop();
        }
    }
}
