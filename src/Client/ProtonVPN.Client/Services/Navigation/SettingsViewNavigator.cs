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

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Services.Mapping;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Contracts.Services.Navigation.Bases;
using ProtonVPN.Client.UI.Main.Settings.Pages;
using ProtonVPN.Client.UI.Main.Settings.Pages.About;
using ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;
using ProtonVPN.Client.UI.Main.Settings.Pages.DefaultConnections;

namespace ProtonVPN.Client.Services.Navigation;

public class SettingsViewNavigator : ViewNavigatorBase, ISettingsViewNavigator
{
    public override bool IsNavigationStackEnabled => true;

    public SettingsViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper)
        : base(logger, pageViewMapper)
    {
    }

    public Task<bool> NavigateToAdvancedSettingsViewAsync()
    {
        return NavigateToAsync<AdvancedSettingsPageViewModel>();
    }

    public Task<bool> NavigateToCommonSettingsViewAsync()
    {
        return NavigateToAsync<CommonSettingsPageViewModel>();
    }

    public Task<bool> NavigateToDefaultConnectionSettingsViewAsync()
    {
        return NavigateToAsync<DefaultConnectionSettingsPageViewModel>();
    }

    public Task<bool> NavigateToProtocolSettingsViewAsync()
    {
        return NavigateToAsync<ProtocolSettingsPageViewModel>();
    }

    public Task<bool> NavigateToVpnAcceleratorSettingsViewAsync()
    {
        return NavigateToAsync<VpnAcceleratorSettingsPageViewModel>();
    }

    public Task<bool> NavigateToCustomDnsSettingsViewAsync()
    {
        return NavigateToAsync<CustomDnsServersViewModel>();
    }

    public Task<bool> NavigateToAutoStartupSettingsViewAsync()
    {
        return NavigateToAsync<AutoStartupSettingsPageViewModel>();
    }

    public Task<bool> NavigateToDebugLogsSettingsViewAsync()
    {
        return NavigateToAsync<DebugLogsPageViewModel>();
    }

    public Task<bool> NavigateToAboutViewAsync()
    {
        return NavigateToAsync<AboutPageViewModel>();
    }

    public Task<bool> NavigateToLicensingViewAsync()
    {
        return NavigateToAsync<LicensingViewModel>();
    }

    public Task<bool> NavigateToDeveloperToolsViewAsync()
    {
        return NavigateToAsync<DeveloperToolsPageViewModel>();
    }

    public Task<bool> NavigateToCensorshipViewAsync()
    {
        return NavigateToAsync<CensorshipSettingsPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return NavigateToCommonSettingsViewAsync();
    }
}