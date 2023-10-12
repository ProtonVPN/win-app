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
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Account;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.Translations;

namespace ProtonVPN.Servers
{
    public class AnnouncementBannerViewModel : ViewModel, IServerListItem
    {
        private readonly ILogger _logger;
        private readonly IOsProcesses _processes;
        private readonly IWebAuthenticator _webAuthenticator;
        private readonly IUpsellDisplayStatisticalEventSender _upsellDisplayStatisticalEventSender;
        private readonly IUpsellUpgradeAttemptStatisticalEventSender _upsellUpgradeAttemptStatisticalEventSender;
        private readonly ISchedulerTimer _timer;
        private DateTime _endDate;
        private PanelButton _panelButton = null;
        private string _reference;

        public string Id => string.Empty;
        public string Name => string.Empty;
        public bool Maintenance => false;
        public bool Connected => false;
        
        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set => Set(ref _imagePath, value);
        }

        private string _timeLeft;
        public string TimeLeft
        {
            get => _timeLeft;
            set => Set(ref _timeLeft, value);
        }

        private bool _isTimeLeftVisible;
        public bool IsTimeLeftVisible
        {
            get => _isTimeLeftVisible;
            set => Set(ref _isTimeLeftVisible, value);
        }

        public ICommand OpenUrlCommand { get; }

        public AnnouncementBannerViewModel(ILogger logger,
            IScheduler scheduler,
            IOsProcesses processes,
            IWebAuthenticator webAuthenticator,
            IUpsellDisplayStatisticalEventSender upsellDisplayStatisticalEventSender,
            IUpsellUpgradeAttemptStatisticalEventSender upsellUpgradeAttemptStatisticalEventSender)
        {
            _logger = logger;
            _processes = processes;
            _webAuthenticator = webAuthenticator;
            _upsellDisplayStatisticalEventSender = upsellDisplayStatisticalEventSender;
            _upsellUpgradeAttemptStatisticalEventSender = upsellUpgradeAttemptStatisticalEventSender;
            OpenUrlCommand = new RelayCommand(OpenUrlAction);

            _timer = scheduler.Timer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
        }

        private async void OpenUrlAction()
        {
            if (_panelButton is not null && _panelButton.Action == "OpenURL")
            {
                string url = _panelButton.Behaviors.Contains("AutoLogin")
                    ? await _webAuthenticator.GetLoginUrlAsync(_panelButton.Url, ModalSources.PromoOffer, _reference)
                    : _panelButton.Url;
                _processes.Open(url);
                _upsellDisplayStatisticalEventSender.Send(ModalSources.PromoOffer, _reference);
                _upsellUpgradeAttemptStatisticalEventSender.Send(ModalSources.PromoOffer, _reference);
            }
            else
            {
                _logger.Error<AppLog>($"The button is null or the action '{_panelButton?.Action}' is unsupported.");
            }
        }

        public void SetWithCountdown(string imagePath, DateTime endDate, PanelButton panelButton, string reference)
        {
            SetProperties(imagePath, endDate, panelButton, reference);
            _timer.IsEnabled = true;
            IsTimeLeftVisible = true;
        }

        private void SetProperties(string imagePath, DateTime endDate, PanelButton panelButton, string reference)
        {
            ImagePath = imagePath;
            _endDate = endDate;
            _panelButton = panelButton;
            _reference = reference;
        }

        public void Set(string imagePath, DateTime endDate, PanelButton panelButton, string reference)
        {
            SetProperties(imagePath, endDate, panelButton, reference);
            _timer.IsEnabled = false;
            IsTimeLeftVisible = false;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            TimeSpan timeUntilExpiry = _endDate - DateTime.UtcNow;
            if (timeUntilExpiry.TotalSeconds < 1.0)
            {
                TimeLeft = string.Empty;
                IsTimeLeftVisible = false;
                _timer.IsEnabled = false;
            }
            else
            {
                TimeLeft = GenerateCountdownString(timeUntilExpiry);
                IsTimeLeftVisible = true;
            }
        }

        private string GenerateCountdownString(TimeSpan timeSpan)
        {
            try
            {
                string firstTimeUnit;
                string secondTimeUnit;

                if (timeSpan.Days > 0)
                {
                    firstTimeUnit = $"{timeSpan.Days} {Translation.GetPlural("TimeUnit_val_Day", timeSpan.Days)}";
                    secondTimeUnit = $"{timeSpan.Hours} {Translation.GetPlural("TimeUnit_val_Hour", timeSpan.Hours)}";
                }
                else if (timeSpan.Minutes > 0)
                {
                    firstTimeUnit = $"{timeSpan.Hours} {Translation.GetPlural("TimeUnit_val_Hour", timeSpan.Hours)}";
                    secondTimeUnit = $"{timeSpan.Minutes} {Translation.GetPlural("TimeUnit_val_Minute", timeSpan.Minutes)}";
                }
                else
                {
                    firstTimeUnit = $"{timeSpan.Minutes} {Translation.GetPlural("TimeUnit_val_Minute", timeSpan.Minutes)}";
                    secondTimeUnit = $"{timeSpan.Seconds} {Translation.GetPlural("TimeUnit_val_Second", timeSpan.Seconds)}";
                }

                return string.Format(Translation.Get("Sidebar_AnnouncementBanner_lbl_Countdown"), firstTimeUnit, secondTimeUnit);
            }
            catch (Exception ex)
            {
                _logger.Error<AppLog>($"Error when transforming the TimeSpan {timeSpan} to text.", ex);
                return timeSpan.ToString("d\\.hh\\:mm\\:ss");
            }
        }

        public void OnVpnStateChanged(VpnState state)
        {
        }
    }
}