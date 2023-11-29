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

using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Account;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Servers;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Sidebar.ChangeServer;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Modals
{
    public class ChangeServerModalViewModel : UpsellModalViewModel, IHandle<ChangeServerTimeLeftMessage>
    {
        protected override ModalSources ModalSource => ModalSources.ChangeServer;

        private readonly ServerChangeManager _serverChangeManager;

        public ChangeServerModalViewModel(
            IEventAggregator eventAggregator,
            ServerChangeManager serverChangeManager,
            ISubscriptionManager subscriptionManager,
            ServerManager serverManager,
            IActiveUrls urls,
            IUpsellUpgradeAttemptStatisticalEventSender upsellUpgradeAttemptStatisticalEventSender,
            IUpsellDisplayStatisticalEventSender upsellDisplayStatisticalEventSender)
            : base(subscriptionManager, serverManager, urls, upsellUpgradeAttemptStatisticalEventSender, 
                  upsellDisplayStatisticalEventSender)
        {
            eventAggregator.Subscribe(this);

            _serverChangeManager = serverChangeManager;

            ChangeServerCommand = new RelayCommand(ChangeServerActionAsync);
        }

        public ICommand ChangeServerCommand { get; }

        private string _timeLeft = string.Empty;

        public string TimeLeft
        {
            get => _timeLeft;
            set => Set(ref _timeLeft, value);
        }

        private double _progress;

        public double Progress
        {
            get => _progress;
            set => Set(ref _progress, value);
        }

        private bool _isToShowUpgradeButton;

        public bool IsToShowUpgradeButton
        {
            get => _isToShowUpgradeButton;
            set => Set(ref _isToShowUpgradeButton, value);
        }

        private double _timeLeftInSeconds;
        public double TimeLeftInSeconds
        {
            get => _timeLeftInSeconds;
            set => Set(ref _timeLeftInSeconds, value);
        }

        private bool _isToShowChangeServerButton;

        public bool IsToShowChangeServerButton
        {
            get => _isToShowChangeServerButton;
            set => Set(ref _isToShowChangeServerButton, value);
        }

        private bool _isToShowTitle;

        public bool IsToShowTitle
        {
            get => _isToShowTitle;
            set => Set(ref _isToShowTitle, value);
        }

        private bool _isToShowSubtitle;

        public bool IsToShowSubtitle
        {
            get => _isToShowSubtitle;
            set => Set(ref _isToShowSubtitle, value);
        }

        private async void ChangeServerActionAsync()
        {
            await _serverChangeManager.ChangeServerAsync();
        }

        public async Task HandleAsync(ChangeServerTimeLeftMessage message, CancellationToken cancellationToken)
        {
            TimeLeft = message.TimeLeftFormatted;
            Progress = 100 - message.Progress;
            IsToShowChangeServerButton = message.TimeLeftInSeconds <= 0;
            IsToShowUpgradeButton = message.TimeLeftInSeconds > 0;
            IsToShowTitle = message.IsLongDelay && message.TimeLeftInSeconds > 0;
            IsToShowSubtitle = message.TimeLeftInSeconds > 0;
            TimeLeftInSeconds = message.TimeLeftInSeconds;
        }
    }
}