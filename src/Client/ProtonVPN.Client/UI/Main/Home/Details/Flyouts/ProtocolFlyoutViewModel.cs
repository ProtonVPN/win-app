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
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class ProtocolFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IConnectionManager _connectionManager;
    private readonly IProfileEditor _profileEditor;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProtocolName))]
    [NotifyPropertyChangedFor(nameof(ProtocolDescription))]
    private VpnProtocol? _protocol;

    public string ProtocolName => Localizer.GetVpnProtocol(Protocol);

    public string ProtocolDescription => Localizer.GetVpnProtocolDescription(Protocol);

    public string VpnProtocolLearnMoreUri => _urlsBrowser.ProtocolsLearnMore;

    public ProtocolFlyoutViewModel(
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IUrlsBrowser urlsBrowser,
        IConnectionManager connectionManager,
        IProfileEditor profileEditor,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _connectionManager = connectionManager;
        _profileEditor = profileEditor;
        _mainViewNavigator = mainViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(SetProtocol);
        }
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
        if (_connectionManager.IsConnected && _connectionManager.CurrentConnectionIntent is IConnectionProfile profile)
        {
            await _profileEditor.TryRedirectToProfileAsync(Localizer.Get("Settings_Connection_Protocol"), profile);
            return;
        }

        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToProtocolSettingsViewAsync(isDirectNavigation: true);
    }
}