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
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ProtonVPN.Api;
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.BugReporting.FormElements;
using ProtonVPN.BugReporting.Screens;
using ProtonVPN.BugReporting.Steps;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Login;
using ProtonVPN.Modals;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.BugReporting
{
    public class ReportBugModalViewModel : BaseModalViewModel,
        IHandle<SendReportAction>,
        IHandle<FinishReportAction>,
        IHandle<FormStateChange>,
        IHandle<GoBackAfterFailureAction>,
        ILogoutAware,
        ILoggedInAware
    {
        private readonly static TimeSpan CONNECT_TIMEOUT = TimeSpan.FromMinutes(5);

        private readonly ILogger _logger;
        private readonly IBugReport _bugReport;
        private readonly StepsContainerViewModel _stepsContainerViewModel;
        private readonly IEventAggregator _eventAggregator;
        private readonly SendingViewModel _sendingViewModel;
        private readonly FailureViewModel _failureViewModel;
        private readonly SentViewModel _sentViewModel;
        private readonly IApiAvailabilityVerifier _apiAvailabilityVerifier;
        private readonly GuestHoleState _guestHoleState;
        private readonly GuestHoleConnector _guestHoleConnector;
        private readonly ISingleAction _timeoutAction;

        private VpnStatus _lastVpnStatus = VpnStatus.Disconnected;
        private FormState _formState = FormState.Editing;

        private Screen _screenViewModel;
        private SendReportAction _messageToRetry;
        private bool _isUserLoggedIn;

        public ReportBugModalViewModel(ILogger logger,
            IBugReport bugReport,
            StepsContainerViewModel stepsContainerViewModel,
            IEventAggregator eventAggregator,
            SendingViewModel sendingViewModel,
            FailureViewModel failureViewModel,
            SentViewModel sentViewModel,
            IApiAvailabilityVerifier apiAvailabilityVerifier,
            GuestHoleState guestHoleState,
            GuestHoleConnector guestHoleConnector)
        {
            eventAggregator.Subscribe(this);
            _logger = logger;
            _bugReport = bugReport;
            _stepsContainerViewModel = stepsContainerViewModel;
            _eventAggregator = eventAggregator;
            _sendingViewModel = sendingViewModel;
            _failureViewModel = failureViewModel;
            _sentViewModel = sentViewModel;
            _apiAvailabilityVerifier = apiAvailabilityVerifier;
            _guestHoleState = guestHoleState;
            _guestHoleConnector = guestHoleConnector;

            ScreenViewModel = stepsContainerViewModel;

            _timeoutAction = new SingleAction(TimeoutAction);
            _timeoutAction.Completed += OnTimeoutActionCompleted;
        }

        public Screen ScreenViewModel
        {
            get => _screenViewModel;
            set => Set(ref _screenViewModel, value);
        }

        private async Task TimeoutAction(CancellationToken cancellationToken)
        {
            await Task.Delay(CONNECT_TIMEOUT, cancellationToken);

            if (_formState == FormState.Sending)
            {
                _logger.Info<AppLog>($"Failed to send bug report in {CONNECT_TIMEOUT}.");
                if (_guestHoleState.Active)
                {
                    _logger.Info<AppLog>($"Failed to send bug report with Guest Hole in {CONNECT_TIMEOUT}. " +
                        $"Disabling Guest Hole and disconnecting.");
                    await DisconnectGuestHoleAsync();
                }
                await ShowFailureViewAndPublishFailedToSendAsync("Timed out.");
            }
        }

        private async Task DisconnectGuestHoleAsync()
        {
            _guestHoleState.SetState(false);
            await _guestHoleConnector.Disconnect();
        }

        private void OnTimeoutActionCompleted(object sender, TaskCompletedEventArgs e)
        {
            _logger.Info<AppLog>("Timeout action completed.");
        }

        public async void Handle(SendReportAction message)
        {
            _timeoutAction.Run();
            await _eventAggregator.PublishOnUIThreadAsync(new FormStateChange(FormState.Sending));
            _messageToRetry = message;
            _sendingViewModel.HasFailedFirstAttempt = false;
            ShowSendingView();

            Result result = await SendBugReportAsync(message);

            if (result.Success)
            {
                await ShowSuccessViewAndPublishSentAsync(message);
            }
            else
            {
                await OnSendFailureAsync(result.Error);
            }
        }

        private async Task<Result> SendBugReportAsync(SendReportAction message)
        {
            return await _bugReport.SendAsync(message);
        }

        private async Task ShowSuccessViewAndPublishSentAsync(SendReportAction message)
        {
            ShowSuccessView(message.FormElements.GetEmailField().Value);
            await _eventAggregator.PublishOnUIThreadAsync(new FormStateChange(FormState.Sent));
        }

        public void Handle(FinishReportAction message)
        {
            TryClose();
        }

        public void Handle(FormStateChange message)
        {
            _formState = message.State;
            if (_formState is not FormState.Sending)
            {
                _messageToRetry = null;
            }
            if (_formState is FormState.FailedToSend or FormState.Sent)
            {
                StopTimeoutAction();
            }
        }

        private void StopTimeoutAction()
        {
            if (_timeoutAction.IsRunning)
            {
                _timeoutAction.Cancel();
            }
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

        private async Task OnSendFailureAsync(string error)
        {
            if (_isUserLoggedIn)
            {
                await ShowFailureViewAndPublishFailedToSendAsync(error);
            }
            else
            {
                _sendingViewModel.HasFailedFirstAttempt = true;
                bool isSignUpPageAccessible = await _apiAvailabilityVerifier.IsSignUpPageAccessibleAsync();
                if (isSignUpPageAccessible)
                {
                    await ShowFailureViewAndPublishFailedToSendAsync(error);
                }
                else
                {
                    await ConnectToGuestHoleAsync();
                }
            }
        }

        private async Task ShowFailureViewAndPublishFailedToSendAsync(string error)
        {
            ShowFailureView(error);
            await _eventAggregator.PublishOnUIThreadAsync(new FormStateChange(FormState.FailedToSend));
        }

        private void ShowFailureView(string error)
        {
            _failureViewModel.Error = error;
            ScreenViewModel = _failureViewModel;
        }

        private async Task ConnectToGuestHoleAsync()
        {
            _guestHoleState.SetState(true);
            await _guestHoleConnector.Connect();
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (_lastVpnStatus == e.State.Status)
            {
                return;
            }

            if (_formState == FormState.Sending && _guestHoleState.Active)
            {
                SendReportAction message = _messageToRetry;
                switch (e.State.Status)
                {
                    case VpnStatus.Connected when message is not null:
                        await OnConnectedWithGuestHoleAsync(message);
                        break;
                    case VpnStatus.Connected when message is null:
                        await OnConnectedWithGuestHoleWithoutMessageAsync();
                        break;
                    case VpnStatus.Disconnected:
                        await OnDisconnectedWithGuestHoleAsync();
                        break;
                }
            }

            _lastVpnStatus = e.State.Status;
        }

        private async Task OnConnectedWithGuestHoleAsync(SendReportAction message)
        {
            Result result = await SendBugReportAsync(message);
            await DisconnectGuestHoleAsync();
            if (result.Success)
            {
                await ShowSuccessViewAndPublishSentAsync(message);
            }
            else
            {
                await ShowFailureViewAndPublishFailedToSendAsync(result.Error);
            }
        }

        private async Task OnConnectedWithGuestHoleWithoutMessageAsync()
        {
            await DisconnectGuestHoleAsync();
            await ShowFailureViewAndPublishFailedToSendAsync("Connected with Guest Hole but the message is null.");
        }

        private async Task OnDisconnectedWithGuestHoleAsync()
        {
            await DisconnectGuestHoleAsync();
            await ShowFailureViewAndPublishFailedToSendAsync("Guest Hole was disconnected.");
        }

        public void OnUserLoggedOut()
        {
            _isUserLoggedIn = false;
        }

        public void OnUserLoggedIn()
        {
            _isUserLoggedIn = true;
        }
    }
}