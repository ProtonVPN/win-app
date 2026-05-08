/*
 * Copyright (c) 2026 Proton AG
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Statistics;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Observers;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Main.Home.Upsell;

namespace ProtonVPN.Client.UI.Main.Home.Card.Feedback;

public partial class ConnectionFeedbackComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<MainWindowFocusedMessage>,
    IEventMessageReceiver<TrayAppWindowFocusedMessage>,
    IEventMessageReceiver<FeatureFlagsChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private const int SUBMIT_FEEDBACK_ANIMATION_DURATION_MS = 1000;

    private readonly IConnectionManager _connectionManager;
    private readonly IConnectionStatisticsFeedback _connectionStatisticsFeedback;
    private readonly ISettings _settings;
    private readonly IFeatureFlagsObserver _featureFlagsObserver;
    private readonly IConnectionCardUpsellBannerModerator _connectionCardUpsellBannerModerator;

    private bool _hasReceivedAppFocus;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReportPoorConnectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReportGoodConnectionCommand))]
    [NotifyPropertyChangedFor(nameof(IsConnectionFeedbackVisible))]
    private bool _isFeedbackSent;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReportPoorConnectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReportGoodConnectionCommand))]
    private bool _isSendingFeedback;

    public bool IsConnectionFeedbackVisible => !IsFeedbackSent
                                            && IsElligible
                                            && _connectionManager.IsConnected
                                            && _hasReceivedAppFocus;

    public bool IsElligible => _settings.IsShareStatisticsEnabled
                            && _featureFlagsObserver.IsConnectionFeedbackEnabled;

    public ConnectionFeedbackComponentViewModel(
        IConnectionManager connectionManager,
        IConnectionStatisticsFeedback connectionStatisticsFeedback,
        ISettings settings,
        IFeatureFlagsObserver featureFlagsObserver,
        IConnectionCardUpsellBannerModerator connectionCardUpsellBannerModerator,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _connectionManager = connectionManager;
        _connectionStatisticsFeedback = connectionStatisticsFeedback;
        _settings = settings;
        _featureFlagsObserver = featureFlagsObserver;
        _connectionCardUpsellBannerModerator = connectionCardUpsellBannerModerator;
    }

    [RelayCommand(CanExecute = nameof(CanReportConnection))]
    private async Task ReportPoorConnectionAsync()
    {
        try
        {
            IsSendingFeedback = true;

            _connectionStatisticsFeedback.SubmitNegativeFeedback();

            await Task.Delay(SUBMIT_FEEDBACK_ANIMATION_DURATION_MS);
        }
        finally
        {
            IsFeedbackSent = true;
            IsSendingFeedback = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanReportConnection))]
    private async Task ReportGoodConnectionAsync()
    {
        try
        {
            IsSendingFeedback = true;

            _connectionStatisticsFeedback.SubmitPositiveFeedback();

            await Task.Delay(SUBMIT_FEEDBACK_ANIMATION_DURATION_MS);
        }
        finally
        {
            IsFeedbackSent = true;
            IsSendingFeedback = false;
        }
    }

    private bool CanReportConnection()
    {
        return !IsSendingFeedback
            && IsConnectionFeedbackVisible;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            // Reset feedback state when the connection drops.
            if (message.ConnectionStatus != ConnectionStatus.Connected)
            {
                _hasReceivedAppFocus = false;
                IsFeedbackSent = false;
            }

            NotifyFeedbackStateChanged();
        });
    }

    public void Receive(MainWindowFocusedMessage message)
    {
        ExecuteOnUIThread(OnAppFocused);
    }

    public void Receive(TrayAppWindowFocusedMessage message)
    {
        ExecuteOnUIThread(OnAppFocused);
    }

    public void Receive(FeatureFlagsChangedMessage message)
    {
        if (message.Changes.Any(f => f.Name == nameof(IFeatureFlagsObserver.IsConnectionFeedbackEnabled)))
        {
            ExecuteOnUIThread(NotifyFeedbackStateChanged);
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsShareStatisticsEnabled))
        {
            ExecuteOnUIThread(NotifyFeedbackStateChanged);
        }
    }

    private void OnAppFocused()
    {
        if (_connectionManager.IsConnected && !_connectionCardUpsellBannerModerator.IsBannerVisible)
        {
            _hasReceivedAppFocus = true;

            NotifyFeedbackStateChanged();
        }

        if (IsConnectionFeedbackVisible && !IsSendingFeedback)
        {
            // Feedback component is now visible, initialize feedback state for the current connection session to 'ignore'.
            // This ensures that if the user doesn't provide feedback, the session will be categorized as 'ignore' in the statistics, instead of 'unknown'.
            _connectionStatisticsFeedback.InitializeFeedback();
        }
    }

    private void NotifyFeedbackStateChanged()
    {
        OnPropertyChanged(nameof(IsElligible));
        OnPropertyChanged(nameof(IsConnectionFeedbackVisible));
        ReportPoorConnectionCommand.NotifyCanExecuteChanged();
        ReportGoodConnectionCommand.NotifyCanExecuteChanged();
    }
}