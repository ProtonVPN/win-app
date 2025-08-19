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
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.History;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Home.Details.Connection;

public partial class ConnectionDetailsPageViewModel : PageViewModelBase<IDetailsViewNavigator>,
    IEventMessageReceiver<ConnectionDetailsChangedMessage>,
    IEventMessageReceiver<NetworkTrafficChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly INetworkTrafficManager _networkTrafficManager;

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
        ISettings settings,
        IConnectionManager connectionManager,
        INetworkTrafficManager networkTrafficManager,
        IDetailsViewNavigator viewNavigator,
        IViewModelHelper viewModelHelper)
        : base(viewNavigator, viewModelHelper)
    {
        _settings = settings;
        _connectionManager = connectionManager;
        _networkTrafficManager = networkTrafficManager;
    }

    public void Receive(ConnectionDetailsChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateServerIpAddress);
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

        InvalidateServerIpAddress();
        SetDetails();
    }

    public void Receive(NetworkTrafficChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(SetDetails);
        }
    }

    private void InvalidateServerIpAddress()
    {
        ServerIpAddress = EmptyValueExtensions.GetValueOrDefault(_connectionManager.CurrentConnectionDetails?.ServerIpAddress?.Ipv4Address);
    }

    private void SetDetails()
    {
        ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;
        ServerLoad = connectionDetails?.ServerLoad ?? 0;
        Protocol = connectionDetails?.Protocol;
        Volume = (long)_networkTrafficManager.GetVolume().Sum();
    }
}