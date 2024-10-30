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
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.UI.Main.Home.Details.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Home.Details.Connection;

public partial class ConnectionDetailsPageViewModel : PageViewModelBase<IDetailsViewNavigator>, IConnectionDetailsAware,
    IEventMessageReceiver<ConnectionDetailsChanged>
{
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    private string? _serverIpAddress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedVolume))]
    private long? _volume;

    [ObservableProperty]
    private double _serverLoad;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedProtocol))]
    private VpnProtocol? _protocol;

    public string FormattedVolume => Localizer.GetFormattedSize(Volume);

    public string FormattedProtocol => Localizer.GetVpnProtocol(Protocol);

    public ConnectionDetailsPageViewModel(
        IConnectionManager connectionManager,
        IDetailsViewNavigator viewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(viewNavigator, localizer, logger, issueReporter)
    {
        _connectionManager = connectionManager;
    }

    public void Receive(ConnectionDetailsChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            ServerIpAddress = message.ServerIpAddress;
        });
    }

    public void Refresh(ConnectionDetails? connectionDetails, TrafficBytes volume, TrafficBytes speed)
    {
        if (IsActive)
        {
            Volume = (long)volume.BytesIn + (long)volume.BytesOut;
            ServerLoad = connectionDetails?.ServerLoad ?? 0;
            Protocol = connectionDetails?.Protocol;
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(FormattedVolume));
        OnPropertyChanged(nameof(FormattedProtocol));
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        Volume = null;
        ServerLoad = 0;
        Protocol = null;
    }
}