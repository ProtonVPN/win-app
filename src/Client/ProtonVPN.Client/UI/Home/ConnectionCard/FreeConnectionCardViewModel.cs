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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.UI.Home.ConnectionCard.Overlays;

namespace ProtonVPN.Client.UI.Home.ConnectionCard;

public partial class FreeConnectionCardViewModel : ConnectionCardViewModelBase,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<ChangeServerAttemptInvalidatedMessage>
{
    private const int FREE_COUNTRIES_DISPLAYED_AS_FLAGS = 3;

    private readonly IServersLoader _serversLoader;
    private readonly IOverlayActivator _overlayActivator;
    private readonly IChangeServerModerator _changeServerModerator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedFreeCountriesCount))]
    private int _freeCountriesCount;

    public string FormattedFreeCountriesCount => FreeCountriesCount > FREE_COUNTRIES_DISPLAYED_AS_FLAGS
        ? $"+{FreeCountriesCount - FREE_COUNTRIES_DISPLAYED_AS_FLAGS}"
        : string.Empty;

    public bool ShowFreeConnectionFlags => CurrentConnectionStatus != ConnectionStatus.Connected;

    public bool IsChangeServerTimerVisible => !_changeServerModerator.CanChangeServer();

    public string? FormattedRemainingTime => Localizer.GetFormattedShortTime(_changeServerModerator.GetRemainingDelayUntilNextAttempt());

    public FreeConnectionCardViewModel(
        IConnectionManager connectionManager,
        ILocalizationProvider localizationProvider,
        IServersLoader serversLoader,
        IOverlayActivator overlayActivator,
        IChangeServerModerator changeServerModerator,
        HomeViewModel homeViewModel)
        : base(connectionManager, localizationProvider, homeViewModel)
    {
        _serversLoader = serversLoader;
        _overlayActivator = overlayActivator;
        _changeServerModerator = changeServerModerator;

        CurrentConnectionIntent = ConnectionIntent.FreeDefault;

        InvalidateCurrentConnectionStatus();
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateFreeCountriesCount);
    }

    protected override void InvalidateCurrentConnectionStatus()
    {
        base.InvalidateCurrentConnectionStatus();

        OnPropertyChanged(nameof(ShowFreeConnectionFlags));

        ChangeServerCommand.NotifyCanExecuteChanged();
        ShowAboutFreeConnectionsCommand.NotifyCanExecuteChanged();

        if (ConnectionManager.ConnectionStatus == ConnectionStatus.Disconnected)
        {
            // Reset connection intent to Fastest free server on disconnect
            CurrentConnectionIntent = ConnectionIntent.FreeDefault;
        }
    }

    public void Receive(ChangeServerAttemptInvalidatedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsChangeServerTimerVisible));
            OnPropertyChanged(nameof(FormattedRemainingTime));
        });
    }

    [RelayCommand(CanExecute = nameof(CanShowAboutFreeConnections))]
    private async Task ShowAboutFreeConnectionsAsync()
    {
        await _overlayActivator.ShowOverlayAsync<FreeConnectionsOverlayViewModel>();
    }

    private bool CanShowAboutFreeConnections()
    {
        return CurrentConnectionStatus is ConnectionStatus.Disconnected;
    }

    [RelayCommand(CanExecute = nameof(CanChangeServer))]
    private async Task ChangeServerAsync()
    {
        if (_changeServerModerator.CanChangeServer())
        {
            string? logicalServerId = ConnectionManager.CurrentConnectionDetails?.ServerId;

            CurrentConnectionIntent = logicalServerId is null
                ? ConnectionIntent.FreeDefault
                : new ConnectionIntent(new FreeServerLocationIntent(logicalServerId));

            await ConnectionManager.ConnectAsync(CurrentConnectionIntent);
        }
        else
        {
            await _overlayActivator.ShowOverlayAsync<ChangeServerOverlayViewModel>();
        }
    }

    private bool CanChangeServer()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }

    private void InvalidateFreeCountriesCount()
    {
        List<string> freeCountries = _serversLoader.GetFreeCountryCodes().ToList();

        FreeCountriesCount = freeCountries.Count;
    }
}