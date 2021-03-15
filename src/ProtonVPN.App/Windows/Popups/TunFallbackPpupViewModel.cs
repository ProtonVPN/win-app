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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.BugReporting;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Core.Modals;
using ProtonVPN.Settings;

namespace ProtonVPN.Windows.Popups
{
    public class TunFallbackPopupViewModel : BasePopupViewModel
    {
        private const string NoReplyEmail = "noreply@protonvpn.com";

        private readonly SettingsModalViewModel _settingsModalViewModel;
        private readonly IModals _modals;
        private readonly IBugReport _bugReport;
        private readonly IReportFieldProvider _reportFieldProvider;

        private bool _success;
        private bool _failed;
        private bool _sending;

        public TunFallbackPopupViewModel(
            AppWindow appWindow,
            SettingsModalViewModel settingsModalViewModel,
            IModals modals,
            IBugReport bugReport,
            IReportFieldProvider reportFieldProvider) : base(appWindow)
        {
            _bugReport = bugReport;
            _modals = modals;
            _settingsModalViewModel = settingsModalViewModel;
            _reportFieldProvider = reportFieldProvider;

            OpenAdvancedSettingsCommand = new RelayCommand(OpenAdvancedSettingsAction);
            ReportProblemCommand = new RelayCommand(ReportProblemAction);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Sending = false;
            Success = false;
            Failed = false;
        }

        public bool Sending
        {
            get => _sending;
            set => Set(ref _sending, value);
        }

        public bool Success
        {
            get => _success;
            set => Set(ref _success, value);
        }

        public bool Failed
        {
            get => _failed;
            set => Set(ref _failed, value);
        }

        public ICommand OpenAdvancedSettingsCommand { get; }

        public ICommand ReportProblemCommand { get; }

        private void OpenAdvancedSettingsAction()
        {
            _settingsModalViewModel.OpenAdvancedTab();
            _modals.Show<SettingsModalViewModel>();
        }

        private async void ReportProblemAction()
        {
            Sending = true;
            Result result = await SendBugReport();
            Sending = false;
            Success = result.Success;
            Failed = result.Failure;
        }

        private async Task<Result> SendBugReport()
        {
            KeyValuePair<string, string>[] fields =
                _reportFieldProvider.GetFields("TUN adapter was not found, switched to TAP.", NoReplyEmail);

            return await _bugReport.SendWithLogsAsync(fields);
        }
    }
}