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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;

namespace ProtonVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class ServerLoadFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<NetworkTrafficChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    private double _serverLoad;

    public string ServerLoadLearnMoreUri => _urlsBrowser.ServerLoadLearnMore;

    public ServerLoadFlyoutViewModel(
        IUrlsBrowser urlsBrowser,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _connectionManager = connectionManager;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(Refresh);
    }

    public void Receive(NetworkTrafficChangedMessage message)
    {
        ExecuteOnUIThread(Refresh);
    }

    private void Refresh()
    {
        if (IsActive)
        {
            SetServerLoad();
        }
    }

    private void SetServerLoad()
    {
        ServerLoad = _connectionManager.CurrentConnectionDetails?.ServerLoad ?? 0;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        SetServerLoad();
    }
}