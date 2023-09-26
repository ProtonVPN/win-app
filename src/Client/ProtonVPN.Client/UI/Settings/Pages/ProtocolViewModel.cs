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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class ProtocolViewModel : ConnectionSettingsPageViewModelBase
{
    private readonly IUrls _urls;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSmartProtocol))]
    [NotifyPropertyChangedFor(nameof(IsWireGuardUdpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsOpenVpnUdpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsOpenVpnTcpProtocol))]
    private VpnProtocol _currentVpnProtocol;

    public override string? Title => Localizer.Get("Settings_Connection_Protocol");

    public string Recommended => Localizer.Get("Settings_Protocols_Recommended").ToUpperInvariant();

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

    public string LearnMoreUrl => _urls.ProtocolsLearnMore;

    public ProtocolViewModel(IMainViewNavigator viewNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        IConnectionManager connectionManager,
        IUrls urls)
        : base(viewNavigator, localizationProvider, settings, connectionManager)
    {
        _urls = urls;
    }

    public void Receive(SettingChangedMessage message)
    {
        switch (message.PropertyName)
        {
            case nameof(ISettings.VpnProtocol):
                OnPropertyChanged(nameof(IsSmartProtocol));
                OnPropertyChanged(nameof(IsWireGuardUdpProtocol));
                OnPropertyChanged(nameof(IsOpenVpnUdpProtocol));
                OnPropertyChanged(nameof(IsOpenVpnTcpProtocol));
                break;
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Recommended));
    }

    protected override bool HasConfigurationChanged()
    {
        return Settings.VpnProtocol != CurrentVpnProtocol;
    }

    protected override void SaveSettings()
    {
        Settings.VpnProtocol = CurrentVpnProtocol;
    }

    protected override void RetrieveSettings()
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