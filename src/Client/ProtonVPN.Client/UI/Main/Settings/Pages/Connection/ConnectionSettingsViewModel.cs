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

using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Messages;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.Connection;

public partial class ConnectionSettingsViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly IProfileEditor _profileEditor;
    private readonly IConnectionManager _connectionManager;

    public bool IsPaidUser => _settings.VpnPlan.IsPaid;

    public IConnectionProfile? CurrentProfile => _connectionManager.CurrentConnectionIntent as IConnectionProfile;

    public bool AreSettingsOverridden => _connectionManager.IsConnected && CurrentProfile != null;

    public string SettingsOverriddenTagline => AreSettingsOverridden
        ? Localizer.GetFormat("Settings_OverriddenByProfile_Tagline", CurrentProfile!.Name)
        : string.Empty;

    public string ConnectionProtocolState => Localizer.Get($"Settings_SelectedProtocol_{Protocol}");

    public string VpnAcceleratorSettingsState => Localizer.GetToggleValue(IsPaidUser && _settings.IsVpnAcceleratorEnabled);

    public string NetShieldSettingsState => Localizer.GetToggleValue(IsNetShieldEnabled);

    public string KillSwitchSettingsState => _settings.IsKillSwitchEnabled
        ? Localizer.GetKillSwitchMode(_settings.KillSwitchMode)
        : Localizer.GetToggleValue(false);

    public string PortForwardingSettingsState => Localizer.GetToggleValue(IsPortForwardingEnabled);

    public string SplitTunnelingSettingsState => _settings.IsSplitTunnelingEnabled
        ? Localizer.GetSplitTunnelingMode(_settings.SplitTunnelingMode, useShortVersion: true)
        : Localizer.GetToggleValue(false);

    protected VpnProtocol Protocol => AreSettingsOverridden
        ? CurrentProfile!.Settings.VpnProtocol
        : _settings.VpnProtocol;

    protected bool IsNetShieldEnabled => AreSettingsOverridden
        ? CurrentProfile!.Settings.IsNetShieldEnabled
        : _settings.IsNetShieldEnabled;

    protected bool IsPortForwardingEnabled => AreSettingsOverridden
        ? CurrentProfile!.Settings.IsPortForwardingEnabled
        : _settings.IsPortForwardingEnabled;

    public ConnectionSettingsViewModel(
        ISettings settings,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        ISettingsViewNavigator settingsViewNavigator,
        IProfileEditor profileEditor,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _settings = settings;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _settingsViewNavigator = settingsViewNavigator;
        _profileEditor = profileEditor;
        _connectionManager = connectionManager;
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            switch (message.PropertyName)
            {
                case nameof(ISettings.VpnProtocol):
                    OnPropertyChanged(nameof(ConnectionProtocolState));
                    break;

                case nameof(ISettings.IsVpnAcceleratorEnabled):
                    OnPropertyChanged(nameof(VpnAcceleratorSettingsState));
                    break;

                case nameof(ISettings.IsNetShieldEnabled):
                    OnPropertyChanged(nameof(NetShieldSettingsState));
                    break;

                case nameof(ISettings.IsKillSwitchEnabled):
                case nameof(ISettings.KillSwitchMode):
                    OnPropertyChanged(nameof(KillSwitchSettingsState));
                    break;

                case nameof(ISettings.IsPortForwardingEnabled):
                    OnPropertyChanged(nameof(PortForwardingSettingsState));
                    break;

                case nameof(ISettings.IsSplitTunnelingEnabled):
                    OnPropertyChanged(nameof(SplitTunnelingSettingsState));
                    break;
            }
        });
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    public void Receive(ProfilesChangedMessage message)
    {
        if (IsActive && AreSettingsOverridden)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(ConnectionProtocolState));
        OnPropertyChanged(nameof(VpnAcceleratorSettingsState));
        OnPropertyChanged(nameof(NetShieldSettingsState));
        OnPropertyChanged(nameof(KillSwitchSettingsState));
        OnPropertyChanged(nameof(PortForwardingSettingsState));
        OnPropertyChanged(nameof(SplitTunnelingSettingsState));
        OnPropertyChanged(nameof(SettingsOverriddenTagline));
    }

    [RelayCommand]
    private Task NavigateToProtocolPageAsync()
    {
        return AreSettingsOverridden
            ? _profileEditor.TryRedirectToProfileAsync(Localizer.Get("Settings_Connection_Protocol"), CurrentProfile!)
            : _settingsViewNavigator.NavigateToProtocolSettingsViewAsync();
    }

    [RelayCommand]
    private Task NavigateToNetShieldPageAsync()
    {
        return IsPaidUser
            ? AreSettingsOverridden
                ? _profileEditor.TryRedirectToProfileAsync(Localizer.Get("Settings_Connection_NetShield"), CurrentProfile!)
                : _settingsViewNavigator.NavigateToNetShieldSettingsViewAsync()
            : _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.NetShield);
    }

    [RelayCommand]
    private async Task NavigateToKillSwitchPageAsync()
    {
        await _settingsViewNavigator.NavigateToKillSwitchSettingsViewAsync();
    }

    [RelayCommand]
    private Task NavigateToPortForwardingPageAsync()
    {
        return IsPaidUser
            ? AreSettingsOverridden
                ? _profileEditor.TryRedirectToProfileAsync(Localizer.Get("Settings_Connection_PortForwarding"), CurrentProfile!)
                : _settingsViewNavigator.NavigateToPortForwardingSettingsViewAsync()
            : _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.P2P);
    }

    [RelayCommand]
    private Task NavigateToSplitTunnelingPageAsync()
    {
        return IsPaidUser
            ? _settingsViewNavigator.NavigateToSplitTunnelingSettingsViewAsync()
            : _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.SplitTunneling);
    }

    [RelayCommand]
    private Task NavigateToVpnAcceleratorPageAsync()
    {
        return IsPaidUser
            ? _settingsViewNavigator.NavigateToVpnAcceleratorSettingsViewAsync()
            : _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.Speed);
    }

    [RelayCommand]
    private async Task NavigateToAdvancedSettingsPageAsync()
    {
        await _settingsViewNavigator.NavigateToAdvancedSettingsViewAsync();
    }
}