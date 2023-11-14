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
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Account;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.Translations;

namespace ProtonVPN.Servers
{
    public class AnnouncementBannerViewModel : ViewModel, IServerListItem, ILoggedInAware
    {
        private readonly ILogger _logger;
        private readonly IOsProcesses _processes;
        private readonly IWebAuthenticator _webAuthenticator;
        private readonly IUpsellDisplayStatisticalEventSender _upsellDisplayStatisticalEventSender;
        private readonly IUpsellUpgradeAttemptStatisticalEventSender _upsellUpgradeAttemptStatisticalEventSender;
        private readonly IAnnouncementService _announcementService;
        private readonly ISchedulerTimer _timer;

        private Announcement _announcement;

        public string Id => string.Empty;
        public string Name => string.Empty;
        public bool Maintenance => false;
        public bool Connected => false;
        
        public string ImagePath => _announcement?.Panel?.FullScreenImage?.Source;

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

        private bool _isBannerVisible;
        public bool IsBannerVisible
        {
            get => _isBannerVisible;
            set => Set(ref _isBannerVisible, value);
        }

        public ICommand OpenUrlCommand { get; }

        public ICommand CloseBannerCommand { get; }

        public AnnouncementBannerViewModel(ILogger logger,
            IScheduler scheduler,
            IOsProcesses processes,
            IWebAuthenticator webAuthenticator,
            IUpsellDisplayStatisticalEventSender upsellDisplayStatisticalEventSender,
            IUpsellUpgradeAttemptStatisticalEventSender upsellUpgradeAttemptStatisticalEventSender,
            IAnnouncementService announcementService)
        {
            _logger = logger;
            _processes = processes;
            _webAuthenticator = webAuthenticator;
            _upsellDisplayStatisticalEventSender = upsellDisplayStatisticalEventSender;
            _upsellUpgradeAttemptStatisticalEventSender = upsellUpgradeAttemptStatisticalEventSender;
            _announcementService = announcementService;

            OpenUrlCommand = new RelayCommand(OpenUrlAction);
            CloseBannerCommand = new RelayCommand(CloseBannerAction, CanCloseBanner);

            IsBannerVisible = true;

            _timer = scheduler.Timer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
        }

        private async void OpenUrlAction()
        {
            string action = _announcement?.Panel?.Button?.Action ?? string.Empty;

            if (action == "OpenURL")
            {
                string baseUrl = _announcement?.Panel?.Button?.Url ?? string.Empty;
                List<string> behaviors = _announcement?.Panel?.Button?.Behaviors ?? new List<string>();
                string reference = _announcement?.Reference ?? string.Empty;

                string url = behaviors.Contains("AutoLogin")
                    ? await _webAuthenticator.GetLoginUrlAsync(baseUrl, ModalSources.PromoOffer, reference)
                    : baseUrl;

                _processes.Open(url);
                _upsellDisplayStatisticalEventSender.Send(ModalSources.PromoOffer, reference);
                _upsellUpgradeAttemptStatisticalEventSender.Send(ModalSources.PromoOffer, reference);
            }
            else
            {
                _logger.Error<AppLog>($"The button is null or the action '{action}' is unsupported.");
            }
        }

        private void CloseBannerAction()
        {
            IsBannerVisible = false;
            _timer.IsEnabled = false;

            if (_announcement != null)
            {
                _announcementService.MarkAsSeen(_announcement.Id);
            }
        }

        private bool CanCloseBanner()
        {
            return _announcement?.IsDismissible ?? false;
        }

        public void Set(Announcement announcement)
        {
            _announcement = announcement;

            OnPropertyChanged(nameof(ImagePath));
            (CloseBannerCommand as RelayCommand)?.RaiseCanExecuteChanged();

            bool showCountdown = _announcement?.ShowCountdown ?? false;

            _timer.IsEnabled = showCountdown;
            IsTimeLeftVisible = showCountdown;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            TimeSpan timeUntilExpiry = _announcement != null
                ? _announcement.EndDateTimeUtc - DateTime.UtcNow
                : TimeSpan.Zero;

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
                else if (timeSpan.Hours > 0)
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

        public void OnUserLoggedIn()
        {
            // Reset banner visibility when user logs in
            IsBannerVisible = true;
        }
    }
}