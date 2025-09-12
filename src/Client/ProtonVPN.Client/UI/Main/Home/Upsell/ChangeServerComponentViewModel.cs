/*
 * Copyright (c) 2024 Proton AG
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
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.FreeServers;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Services.Upselling;
using ProtonVPN.StatisticalEvents.Contracts;
using ProtonVPN.StatisticalEvents.Contracts.Dimensions;

namespace ProtonVPN.Client.UI.Main.Home.Upsell;

public partial class ChangeServerComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<ChangeServerAttemptInvalidatedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly IChangeServerModerator _changeServerModerator;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly IAccountUpgradeUrlLauncher _accountUpgradeUrlLauncher;

    public bool IsChangeServerTimerVisible => !_changeServerModerator.CanChangeServer();
    public bool IsAttemptsLimitReached => _changeServerModerator.IsAttemptsLimitReached();
    public double DelayInSeconds => _changeServerModerator.GetDelayUntilNextAttempt().TotalSeconds;
    public double RemainingDelayInSeconds => _changeServerModerator.GetRemainingDelayUntilNextAttempt().TotalSeconds;
    public string? FormattedRemainingTime => Localizer.GetFormattedShortTime(_changeServerModerator.GetRemainingDelayUntilNextAttempt());

    public ChangeServerComponentViewModel(
        IConnectionManager connectionManager,
        IChangeServerModerator changeServerModerator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IViewModelHelper viewModelHelper,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher)
        : base(viewModelHelper)
    {
        _connectionManager = connectionManager;
        _changeServerModerator = changeServerModerator;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _accountUpgradeUrlLauncher = accountUpgradeUrlLauncher;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateChangeServer);
        }
    }

    public void Receive(ChangeServerAttemptInvalidatedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateChangeServer);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateChangeServer();
    }

    [RelayCommand(CanExecute = nameof(CanChangeServer))]
    private Task ChangeServerAsync()
    {
        Server? currentServer = _connectionManager.CurrentConnectionDetails?.Server;

        IConnectionIntent connectionIntent = currentServer is null
            ? ConnectionIntent.FreeDefault
            : new ConnectionIntent(FreeServerLocationIntent.Random(ServerInfo.From(currentServer.Id, currentServer.Name)));

        return _connectionManager.ConnectAsync(VpnTriggerDimension.ChangeServer, connectionIntent);
    }

    private bool CanChangeServer()
    {
        return _connectionManager.IsConnected && _changeServerModerator.CanChangeServer();
    }

    [RelayCommand]
    private async Task UpgradePlanAsync()
    {
        await _accountUpgradeUrlLauncher.OpenAsync(ModalSource.ChangeServer);
    }

    private void InvalidateChangeServer()
    {
        OnPropertyChanged(nameof(IsChangeServerTimerVisible));
        OnPropertyChanged(nameof(IsAttemptsLimitReached));
        OnPropertyChanged(nameof(DelayInSeconds));
        OnPropertyChanged(nameof(RemainingDelayInSeconds));
        OnPropertyChanged(nameof(FormattedRemainingTime));

        ChangeServerCommand.NotifyCanExecuteChanged();
    }
}