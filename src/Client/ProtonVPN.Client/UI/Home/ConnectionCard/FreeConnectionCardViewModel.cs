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
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts.Messages;

namespace ProtonVPN.Client.UI.Home.ConnectionCard;

public partial class FreeConnectionCardViewModel : ConnectionCardViewModelBase,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private const int FREE_COUNTRIES_DISPLAYED_AS_FLAGS = 3;
    private readonly IServersLoader _serversLoader;

    [ObservableProperty]
    private int _freeCountriesCount;

    public string FormattedFreeCountriesCount => FreeCountriesCount > FREE_COUNTRIES_DISPLAYED_AS_FLAGS
        ? $"+{FreeCountriesCount - FREE_COUNTRIES_DISPLAYED_AS_FLAGS}"
        : string.Empty;

    public bool ShowFreeConnectionFlags => CurrentConnectionStatus != ConnectionStatus.Connected;

    public FreeConnectionCardViewModel(
        IConnectionManager connectionManager,
        ILocalizationProvider localizationProvider,
        IServersLoader serversLoader,
        HomeViewModel homeViewModel)
        : base(connectionManager, localizationProvider, homeViewModel)
    {
        _serversLoader = serversLoader;

        CurrentConnectionIntent = ConnectionIntent.FreeDefault;

        InvalidateCurrentConnectionStatus();
    }

    protected override void InvalidateCurrentConnectionStatus()
    {
        base.InvalidateCurrentConnectionStatus();

        OnPropertyChanged(nameof(ShowFreeConnectionFlags));

        ChangeServerCommand.NotifyCanExecuteChanged();

        if (ConnectionManager.ConnectionStatus == ConnectionStatus.Disconnected)
        {
            // Reset connection intent to Fastest free server on disconnect
            CurrentConnectionIntent = ConnectionIntent.FreeDefault;
        }
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() => 
        {
            InvalidateFreeCountriesCount();
        });
    }

    private void InvalidateFreeCountriesCount()
    {
        List<string> freeCountries = _serversLoader.GetCountryCodesByTier(ServerTiers.Free).ToList();

        FreeCountriesCount = freeCountries.Count;
    }

    [RelayCommand(CanExecute = nameof(CanChangeServer))]
    private async Task ChangeServerAsync()
    {
        CurrentConnectionIntent = new ConnectionIntent(new FreeServerLocationIntent(FreeServerType.Random));

        await ConnectionManager.ConnectAsync(CurrentConnectionIntent);
    }

    private bool CanChangeServer()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected;
    }

    protected override void ShowConnectionDetails()
    {
        switch (CurrentConnectionStatus)
        {
            case ConnectionStatus.Disconnected:
                // TODO: Implement free countries overlay
                break;
            case ConnectionStatus.Connected:
                base.ShowConnectionDetails();
                break;
            default:
                break;
        }
    }

    protected override bool CanShowConnectionDetails()
    {
        return CurrentConnectionStatus == ConnectionStatus.Connected
            || CurrentConnectionStatus == ConnectionStatus.Disconnected;
    }
}