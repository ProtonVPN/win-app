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

using ByteSizeLib;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.BugReporting.Attachments;
using ProtonVPN.BugReporting.Errors;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Modals;
using ProtonVPN.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ProtonVPN.BugReporting
{
    public class ReportBugModalViewModel : BaseModalViewModel
    {
        private readonly Common.Configuration.Config _appConfig;
        private readonly BugReport _bugReport;
        private readonly SentViewModel _sentViewModel;
        private readonly SendingViewModel _sendingViewModel;
        private readonly FailureViewModel _failureViewModel;

        private ViewModel _currentViewModel;
        private bool _sending;
        private bool _sent;
        private object _error = string.Empty;
        private string _errorDetails;

        public ReportBugModalViewModel(
            Common.Configuration.Config appConfig,
            BugReport bugReport,
            SendingViewModel sendingViewModel,
            SentViewModel sentViewModel,
            FormViewModel formViewModel,
            FailureViewModel failureViewModel)
        {
            _appConfig = appConfig;
            _bugReport = bugReport;
            _sendingViewModel = sendingViewModel;
            _sentViewModel = sentViewModel;
            FormViewModel = formViewModel;
            _failureViewModel = failureViewModel;

            SendReportCommand = new RelayCommand(SendReport, CanSend);
            BackCommand = new RelayCommand(Back);
            RetryCommand = new RelayCommand(Retry, CanSend);
            CloseErrorCommand = new RelayCommand(CloseErrorAction);
        }

        public ICommand SendReportCommand { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand RetryCommand { get; set; }
        public ICommand CloseErrorCommand { get; set; }
        public FormViewModel FormViewModel { get; set; }

        public object Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public string ErrorDetails
        {
            get => _errorDetails;
            set => Set(ref _errorDetails, value);
        }

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
                ClearOverlay();

            FormViewModel.Load();
        }

        public override void CloseAction()
        {
            base.CloseAction();

            Sent = false;
            Error = string.Empty;
            if (OverlayViewModel is FailureViewModel)
            {
                ClearOverlay();
            }
        }

        public void OnAttachmentErrorOccured(AttachmentErrorEventArgs error)
        {
            var attachments = error.Attachments;
            if (attachments.TooLarge().Any())
            {
                var maxSize = ByteSize.FromBytes(_appConfig.ReportBugMaxFileSize);
                Error = new AttachmentSizeLimit {MaxSize = maxSize};
            }
            else if (attachments.TooMany().Any())
            { 
                Error = new AttachmentLimit {Limit = _appConfig.ReportBugMaxFiles};
            }
            else if (attachments.FailedToRead().Any())
            {
                Error = new AttachmentReadError();
            }

            ErrorDetails = FormattedErrorDetails(attachments);
        }

        private string FormattedErrorDetails(IEnumerable<Attachment> items)
        {
            var details = items
                .GroupBy(i => i.ErrorType)
                .Select(g => FormattedErrorDetails(g.Key, g));

            return string.Join("\n\n", details);
        }

        private string FormattedErrorDetails(AttachmentErrorType errorType, IEnumerable<Attachment> items)
        {
            return $"{ErrorHeader(errorType)}\n{FileNameList(items)}";
        }

        private string ErrorHeader(AttachmentErrorType errorType)
        {
            switch (errorType)
            {
                case AttachmentErrorType.FileTooLarge:
                    return StringResources.Get("BugReport_AttachmentError_lbl_TooLarge");
                case AttachmentErrorType.TooManyFiles:
                    return StringResources.Get("BugReport_AttachmentError_lbl_TooMany");
                case AttachmentErrorType.FileReadError:
                    return StringResources.Get("BugReport_AttachmentError_lbl_ReadError");
                default:
                    return string.Empty;
            }
        }

        private string FileNameList(IEnumerable<Attachment> items)
        {
            return string.Join(", \n", items.Select(i => i.Name));
        }

        private void ShowFailureView()
        {
            OverlayViewModel = _failureViewModel;
        }

        private async void SendReport()
        {
            Sending = true;
            ShowSendingWindow();
            Error = string.Empty;

            var result = await _bugReport.Send(
                FormViewModel.GetFields());

            Sending = false;

            if (result.Success)
            {
                ShowReportSentWindow();
                Error = string.Empty;
            }
            else
            {
                ShowFailureView();
            }
        }

        private bool CanSend()
        {
            return !_sending && FormViewModel.IsValid();
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

        private void CloseErrorAction()
        {
            Error = string.Empty;
        }
    }
}
