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
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Trial;

namespace ProtonVPN.Sidebar.Trial
{
    public class TrialViewModel : ViewModel, ITrialStateAware, ITrialDurationAware
    {
        private readonly IActiveUrls _urls;
        private readonly IUserStorage _userStorage;

        private TimeSpan _timeLeft;
        private PlanStatus _trialStatus;

        public TrialViewModel(IActiveUrls urls, IUserStorage userStorage)
        {
            _urls = urls;
            _userStorage = userStorage;
            if (_userStorage.User().IsDelinquent())
            {
                TrialStatus = PlanStatus.Delinquent;
            }
            UpgradeCommand = new RelayCommand(UpgradeAction);
        }

        public PlanStatus TrialStatus
        {
            get => _trialStatus;
            set => Set(ref _trialStatus, value);
        }

        public TimeSpan TimeLeft
        {
            get => _timeLeft;
            set
            {
                _timeLeft = value;
                Set(ref _timeLeft, value);
            }
        }

        public ICommand UpgradeCommand { get; set; }

        private void UpgradeAction()
        {
            IActiveUrl url = _trialStatus == PlanStatus.Delinquent ? _urls.InvoicesUrl : _urls.AccountUrl;
            url.Open();
        }

        public Task OnTrialStateChangedAsync(PlanStatus status)
        {
            TrialStatus = _userStorage.User().IsDelinquent() ? PlanStatus.Delinquent : status;
            return Task.CompletedTask;
        }

        public void OnTrialSecondElapsed(TrialTickEventArgs e)
        {
            TimeLeft = DateTime.Now.AddSeconds(e.SecondsLeft) - DateTime.Now;
            OnPropertyChanged(nameof(TimeLeft));
        }
    }
}
