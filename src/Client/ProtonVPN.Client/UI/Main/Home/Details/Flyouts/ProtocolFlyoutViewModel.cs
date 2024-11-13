﻿/*
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
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class ProtocolFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly IUrls _urls;
    private readonly IConnectionManager _connectionManager;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProtocolName))]
    [NotifyPropertyChangedFor(nameof(ProtocolDescription))]
    private VpnProtocol? _protocol;

    public string ProtocolName => Localizer.GetVpnProtocol(Protocol);

    public string ProtocolDescription => Localizer.GetVpnProtocolDescription(Protocol);

    public string VpnProtocolLearnMoreUri => _urls.ProtocolsLearnMore;

    public ProtocolFlyoutViewModel(
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IUrls urls,
        IConnectionManager connectionManager,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) :
        base(localizer, logger, issueReporter)
    {
        _urls = urls;
        _connectionManager = connectionManager;
        _mainViewNavigator = mainViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (IsActive)
            {
                SetProtocol();
            }
        });
    }

    private void SetProtocol()
    {
        Protocol = _connectionManager.CurrentConnectionDetails?.Protocol;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        SetProtocol();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(ProtocolName));
        OnPropertyChanged(nameof(ProtocolDescription));
    }

    [RelayCommand]
    private async Task OpenProtocolSettingsAsync()
    {
        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToProtocolSettingsViewAsync();
    }
}