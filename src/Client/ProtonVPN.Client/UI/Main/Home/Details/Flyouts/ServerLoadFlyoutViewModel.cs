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
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class ServerLoadFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<NetworkTrafficChangedMessage>
{
    private readonly IUrls _urls;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    private double _serverLoad;

    public string ServerLoadLearnMoreUri => _urls.ServerLoadLearnMore;

    public ServerLoadFlyoutViewModel(
        IUrls urls,
        IConnectionManager connectionManager,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) :
        base(localizer, logger, issueReporter)
    {
        _urls = urls;
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