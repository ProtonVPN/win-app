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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Models.Navigation;

namespace ProtonVPN.Client.UI.Home.ConnectionCard.Overlays;

public partial class ChangeServerOverlayViewModel : OverlayViewModelBase,
    IEventMessageReceiver<ConnectionStatusChanged>,
    IEventMessageReceiver<ChangeServerAttemptInvalidatedMessage>
{
    private readonly IChangeServerModerator _changeServerModerator;
    private readonly IConnectionManager _connectionManager;

    public bool IsChangeServerTimerVisible => !_changeServerModerator.CanChangeServer();

    public bool IsAttemptsLimitReached => _changeServerModerator.IsAttemptsLimitReached();

    public double DelayInSeconds => _changeServerModerator.GetDelayUntilNextAttempt().TotalSeconds;

    public double RemainingDelayInSeconds => _changeServerModerator.GetRemainingDelayUntilNextAttempt().TotalSeconds;

    public string? FormattedRemainingTime => Localizer.GetFormattedShortTime(_changeServerModerator.GetRemainingDelayUntilNextAttempt());

    public ChangeServerOverlayViewModel(
        ILocalizationProvider localizationProvider,
        IMainViewNavigator viewNavigator,
        IOverlayActivator overlayActivator,
        IChangeServerModerator changeServerModerator,
        IConnectionManager connectionManager)
        : base(localizationProvider,
               viewNavigator,
               overlayActivator)
    {
        _changeServerModerator = changeServerModerator;
        _connectionManager = connectionManager;
    }

    public void Receive(ConnectionStatusChanged message)
    {
        if (_connectionManager.IsDisconnected)
        {
            ExecuteOnUIThread(CloseOverlay);
        }
    }
    public void Receive(ChangeServerAttemptInvalidatedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsChangeServerTimerVisible));
            OnPropertyChanged(nameof(FormattedRemainingTime));
            OnPropertyChanged(nameof(RemainingDelayInSeconds));
            OnPropertyChanged(nameof(DelayInSeconds));
            OnPropertyChanged(nameof(IsAttemptsLimitReached));

            ChangeServerCommand.NotifyCanExecuteChanged();
            UpgradePlanCommand.NotifyCanExecuteChanged();
        });
    }

    [RelayCommand(CanExecute = nameof(CanChangeServer))]
    private async Task ChangeServerAsync()
    {
        CloseOverlay();

        string? logicalServerId = _connectionManager.CurrentConnectionDetails?.ServerId;

        IConnectionIntent intent = logicalServerId is null || !_connectionManager.IsConnected
            ? ConnectionIntent.FreeDefault
            : new ConnectionIntent(new FreeServerLocationIntent(logicalServerId));

        await _connectionManager.ConnectAsync(intent);
    }

    private bool CanChangeServer()
    {
        return _changeServerModerator.CanChangeServer();
    }

    [RelayCommand]
    private void UpgradePlan()
    {
        // TODO: Trigger upgrade plan process
    }

    private bool CanUpgradePlan()
    {
        return !_changeServerModerator.CanChangeServer();
    }
}