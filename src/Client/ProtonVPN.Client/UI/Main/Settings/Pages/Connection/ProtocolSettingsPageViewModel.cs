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
using ProtonVPN.Client.Common.Attributes;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Main.Settings.Bases;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.Connection;

public partial class ProtocolSettingsPageViewModel : SettingsPageViewModelBase
{
    private readonly IUrlsBrowser _urlsBrowser;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.VpnProtocol))]
    [NotifyPropertyChangedFor(nameof(IsSmartProtocol))]
    [NotifyPropertyChangedFor(nameof(IsWireGuardUdpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsWireGuardTcpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsWireGuardTlsProtocol))]
    [NotifyPropertyChangedFor(nameof(IsOpenVpnUdpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsOpenVpnTcpProtocol))]
    private VpnProtocol _currentVpnProtocol;

    public override string Title => Localizer.Get("Settings_Connection_Protocol");

    public string Recommended => Localizer.Get("Common_Tags_Recommended").ToUpperInvariant();

    public bool IsSmartProtocol
    {
        get => IsProtocol(VpnProtocol.Smart);
        set => SetProtocol(value, VpnProtocol.Smart);
    }

    public bool IsWireGuardUdpProtocol
    {
        get => IsProtocol(VpnProtocol.WireGuardUdp);
        set => SetProtocol(value, VpnProtocol.WireGuardUdp);
    }

    public bool IsWireGuardTcpProtocol
    {
        get => IsProtocol(VpnProtocol.WireGuardTcp);
        set => SetProtocol(value, VpnProtocol.WireGuardTcp);
    }

    public bool IsWireGuardTlsProtocol
    {
        get => IsProtocol(VpnProtocol.WireGuardTls);
        set => SetProtocol(value, VpnProtocol.WireGuardTls);
    }

    public bool IsOpenVpnUdpProtocol
    {
        get => IsProtocol(VpnProtocol.OpenVpnUdp);
        set => SetProtocol(value, VpnProtocol.OpenVpnUdp);
    }

    public bool IsOpenVpnTcpProtocol
    {
        get => IsProtocol(VpnProtocol.OpenVpnTcp);
        set => SetProtocol(value, VpnProtocol.OpenVpnTcp);
    }

    public string LearnMoreUrl => _urlsBrowser.ProtocolsLearnMore;

    public ProtocolSettingsPageViewModel(
        IUrlsBrowser urlsBrowser,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.VpnProtocol, () => CurrentVpnProtocol)
        ];
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Recommended));
    }

    protected override void OnRetrieveSettings()
    {
        CurrentVpnProtocol = Settings.VpnProtocol;
    }

    private bool IsProtocol(VpnProtocol protocol)
    {
        return CurrentVpnProtocol == protocol;
    }

    private void SetProtocol(bool value, VpnProtocol protocol)
    {
        if (value)
        {
            CurrentVpnProtocol = protocol;
        }
    }
}