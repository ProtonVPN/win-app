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

using System.Threading.Tasks;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Navigation.Bases;

namespace ProtonVPN.Client.Core.Services.Navigation;

public interface ISettingsViewNavigator : IViewNavigator
{
    Task<bool> NavigateToFeatureViewAsync(ConnectionFeature feature, bool isWidgetNavigation = false);

    Task<bool> NavigateToCommonSettingsViewAsync(bool forceNavigation = false);

    Task<bool> NavigateToAdvancedSettingsViewAsync();

    Task<bool> NavigateToDefaultConnectionSettingsViewAsync();

    Task<bool> NavigateToProtocolSettingsViewAsync();

    Task<bool> NavigateToNetShieldSettingsViewAsync(bool isWidgetNavigation = false);

    Task<bool> NavigateToKillSwitchSettingsViewAsync(bool isWidgetNavigation = false);

    Task<bool> NavigateToPortForwardingSettingsViewAsync(bool isWidgetNavigation = false);

    Task<bool> NavigateToSplitTunnelingSettingsViewAsync(bool isWidgetNavigation = false);

    Task<bool> NavigateToVpnAcceleratorSettingsViewAsync();

    Task<bool> NavigateToCustomDnsSettingsViewAsync();

    Task<bool> NavigateToAutoStartupSettingsViewAsync();

    Task<bool> NavigateToDebugLogsSettingsViewAsync();

    Task<bool> NavigateToAboutViewAsync();

    Task<bool> NavigateToCensorshipViewAsync();

    Task<bool> NavigateToLicensingViewAsync();
}