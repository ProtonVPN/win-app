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
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Services.Activation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Settings.Pages.Connection;

public partial class ConnectionSettingsViewModel : PageViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;

    public bool IsPaidUser => _settings.VpnPlan.IsPaid;

    public string ConnectionProtocolState => Localizer.Get($"Settings_SelectedProtocol_{_settings.VpnProtocol}");

    public string VpnAcceleratorSettingsState => Localizer.GetToggleValue(_settings.IsVpnAcceleratorEnabled);
    public string NetShieldSettingsState => Localizer.GetToggleValue(_settings.IsNetShieldEnabled);
    public string KillSwitchSettingsState => Localizer.GetToggleValue(_settings.IsKillSwitchEnabled);
    public string PortForwardingSettingsState => Localizer.GetToggleValue(_settings.IsPortForwardingEnabled);
    public string SplitTunnelingSettingsState => Localizer.GetToggleValue(_settings.IsSplitTunnelingEnabled);

    public ConnectionSettingsViewModel(
        ISettings settings,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        ISettingsViewNavigator settingsViewNavigator,
        ILocalizationProvider localizer, ILogger logger,
        IIssueReporter issueReporter) : base(localizer, logger, issueReporter)
    {
        _settings = settings;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _settingsViewNavigator = settingsViewNavigator;
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            switch (message.PropertyName)
            {
                case nameof(ISettings.IsNetShieldEnabled):
                    OnPropertyChanged(nameof(NetShieldSettingsState));
                    break;

                case nameof(ISettings.VpnPlan):
                    OnPropertyChanged(nameof(IsPaidUser));
                    break;

                case nameof(ISettings.IsKillSwitchEnabled):
                case nameof(ISettings.KillSwitchMode):
                    OnPropertyChanged(nameof(KillSwitchSettingsState));
                    break;
            }
        });
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(ConnectionProtocolState));
        OnPropertyChanged(nameof(VpnAcceleratorSettingsState));
    }

    [RelayCommand]
    private async Task NavigateToProtocolPageAsync()
    {
        await _settingsViewNavigator.NavigateToProtocolSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToNetShieldPageAsync()
    {
        if (!IsPaidUser)
        {
            _upsellCarouselWindowActivator.Activate();
            return;
        }

        await _settingsViewNavigator.NavigateToNetShieldSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToKillSwitchPageAsync()
    {
        await _settingsViewNavigator.NavigateToKillSwitchSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToPortForwardingPageAsync()
    {
        if (!IsPaidUser)
        {
            _upsellCarouselWindowActivator.Activate();
            return;
        }

        await _settingsViewNavigator.NavigateToPortForwardingSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToSplitTunnelingPageAsync()
    {
        if (!IsPaidUser)
        {
            _upsellCarouselWindowActivator.Activate();
            return;
        }

        await _settingsViewNavigator.NavigateToSplitTunnelingSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToVpnAcceleratorPageAsync()
    {
        if (!IsPaidUser)
        {
            _upsellCarouselWindowActivator.Activate();
            return;
        }

        await _settingsViewNavigator.NavigateToVpnAcceleratorSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateToAdvancedSettingsPageAsync()
    {
        await _settingsViewNavigator.NavigateToAdvancedSettingsViewAsync();
    }
}