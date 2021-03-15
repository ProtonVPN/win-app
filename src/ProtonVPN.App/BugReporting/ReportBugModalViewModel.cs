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

using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Modals;

namespace ProtonVPN.BugReporting
{
    public class ReportBugModalViewModel : BaseModalViewModel
    {
        private readonly IBugReport _bugReport;
        private readonly SentViewModel _sentViewModel;
        private readonly SendingViewModel _sendingViewModel;
        private readonly FailureViewModel _failureViewModel;

        private ViewModel _currentViewModel;
        private bool _sending;
        private bool _sent;

        public ReportBugModalViewModel(
            IBugReport bugReport,
            SendingViewModel sendingViewModel,
            SentViewModel sentViewModel,
            FormViewModel formViewModel,
            FailureViewModel failureViewModel)
        {
            _bugReport = bugReport;
            _sendingViewModel = sendingViewModel;
            _sentViewModel = sentViewModel;
            FormViewModel = formViewModel;
            _failureViewModel = failureViewModel;

            SendReportCommand = new RelayCommand(SendReport, CanSend);
            BackCommand = new RelayCommand(Back);
            RetryCommand = new RelayCommand(Retry, CanSend);
        }

        public ICommand SendReportCommand { get; set; }
        public ICommand BackCommand { get; set; }
        public RelayCommand RetryCommand { get; set; }
        public FormViewModel FormViewModel { get; set; }

        public ViewModel OverlayViewModel
        {
            get => _currentViewModel;
            set => Set(ref _currentViewModel, value);
        }

        public bool Sending
        {
            get => _sending;
            set => Set(ref _sending, value);
        }

        public bool Sent
        {
            get => _sent;
            set => Set(ref _sent, value);
        }

        public void ShowSendingWindow()
        {
            OverlayViewModel = _sendingViewModel;
        }

        public void ShowReportSentWindow()
        {
            OverlayViewModel = _sentViewModel;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (OverlayViewModel is SentViewModel)
            {
                ClearOverlay();
            }

            FormViewModel.Load();
        }

        public override void CloseAction()
        {
            base.CloseAction();

            Sent = false;
            if (OverlayViewModel is FailureViewModel)
            {
                ClearOverlay();
            }
        }

        private void ShowFailureView(string error)
        {
            _failureViewModel.Error = error;
            OverlayViewModel = _failureViewModel;
            RetryCommand.RaiseCanExecuteChanged();
        }

        private async void SendReport()
        {
            Sending = true;
            ShowSendingWindow();

            Result result = FormViewModel.IncludeLogs
                ? await _bugReport.SendWithLogsAsync(FormViewModel.GetFields())
                : await _bugReport.SendAsync(FormViewModel.GetFields());

            Sending = false;

            if (result.Success)
            {
                ShowReportSentWindow();
            }
            else
            {
                ShowFailureView(result.Error);
            }
        }

        private bool CanSend()
        {
            return !_sending && !FormViewModel.HasErrors;
        }

        private void Retry()
        {
            SendReport();
        }

        private void ClearOverlay()
        {
            OverlayViewModel = null;
        }

        private void Back()
        {
            ClearOverlay();
        }
    }
}
