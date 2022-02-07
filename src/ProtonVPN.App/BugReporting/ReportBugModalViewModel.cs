/*
 * Copyright (c) 2022 Proton Technologies AG
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

using Caliburn.Micro;
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.BugReporting.FormElements;
using ProtonVPN.BugReporting.Screens;
using ProtonVPN.BugReporting.Steps;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Modals;

namespace ProtonVPN.BugReporting
{
    public class ReportBugModalViewModel : BaseModalViewModel,
        IHandle<SendReportAction>,
        IHandle<FinishReportAction>,
        IHandle<FormStateChange>,
        IHandle<GoBackAfterFailureAction>
    {
        private readonly IBugReport _bugReport;
        private readonly StepsContainerViewModel _stepsContainerViewModel;
        private readonly IEventAggregator _eventAggregator;
        private readonly SendingViewModel _sendingViewModel;
        private readonly FailureViewModel _failureViewModel;
        private readonly SentViewModel _sentViewModel;
        private FormState _formState = FormState.Editing;

        private Screen _screenViewModel;

        public ReportBugModalViewModel(
            IBugReport bugReport,
            StepsContainerViewModel stepsContainerViewModel,
            IEventAggregator eventAggregator,
            SendingViewModel sendingViewModel,
            FailureViewModel failureViewModel,
            SentViewModel sentViewModel)
        {
            eventAggregator.Subscribe(this);

            _bugReport = bugReport;
            _stepsContainerViewModel = stepsContainerViewModel;
            _eventAggregator = eventAggregator;
            _sendingViewModel = sendingViewModel;
            _failureViewModel = failureViewModel;
            _sentViewModel = sentViewModel;

            ScreenViewModel = stepsContainerViewModel;
        }

        public Screen ScreenViewModel
        {
            get => _screenViewModel;
            set => Set(ref _screenViewModel, value);
        }

        public async void Handle(SendReportAction message)
        {
            await _eventAggregator.PublishOnUIThreadAsync(new FormStateChange(FormState.Sending));

            ShowSendingView();

            Result result = await _bugReport.SendAsync(message);

            FormState formState;
            if (result.Success)
            {
                formState = FormState.Sent;
                ShowSuccessView(message.FormElements.GetEmailField().Value);
            }
            else
            {
                formState = FormState.FailedToSend;
                ShowFailureView(result.Error);
            }

            await _eventAggregator.PublishOnUIThreadAsync(new FormStateChange(formState));
        }

        public void Handle(FinishReportAction message)
        {
            TryClose();
        }

        public void Handle(FormStateChange message)
        {
            _formState = message.State;
        }

        public void Handle(GoBackAfterFailureAction message)
        {
            ShowStepsView();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            if (_formState == FormState.Sent)
            {
                ShowStepsView();
            }
        }

        private void ShowStepsView()
        {
            ScreenViewModel = _stepsContainerViewModel;
        }

        private void ShowSendingView()
        {
            ScreenViewModel = _sendingViewModel;
        }

        private void ShowSuccessView(string email)
        {
            _sentViewModel.Email = email;
            ScreenViewModel = _sentViewModel;
        }

        private void ShowFailureView(string error)
        {
            _failureViewModel.Error = error;
            ScreenViewModel = _failureViewModel;
        }
    }
}