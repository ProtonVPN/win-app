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

using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Mapping;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.Contracts.Services.Navigation.Bases;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.UI.Main.Settings.Connection;
using ProtonVPN.Client.UI.Main.Settings.Pages;
using ProtonVPN.Client.UI.Main.Settings.Pages.About;
using ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;
using ProtonVPN.Client.UI.Main.Settings.Pages.Connection;
using ProtonVPN.Client.UI.Main.Settings.Pages.DefaultConnections;
using ProtonVPN.Client.UI.Main.Settings.Pages.DeveloperTools;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Navigation;

public class SettingsViewNavigator : ViewNavigatorBase, ISettingsViewNavigator,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    public override bool IsNavigationStackEnabled => true;

    public SettingsViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper)
        : base(logger, pageViewMapper)
    { }

    protected override void OnFrameNavigated(NavigationEventArgs e)
    {
        base.OnFrameNavigated(e);

        if (GetCurrentPageContext() is CommonSettingsPageViewModel)
        {
            ClearBackStack();
        }
    }

    public override FrameLoadedBehavior LoadBehavior { get; protected set; } = FrameLoadedBehavior.NavigateToDefaultViewIfEmpty;

    public Task<bool> NavigateToFeatureViewAsync(ConnectionFeature feature)
    {
        return feature switch
        {
            ConnectionFeature.NetShield => NavigateToNetShieldSettingsViewAsync(),
            ConnectionFeature.KillSwitch => NavigateToKillSwitchSettingsViewAsync(),
            ConnectionFeature.PortForwarding => NavigateToPortForwardingSettingsViewAsync(),
            ConnectionFeature.SplitTunneling => NavigateToSplitTunnelingSettingsViewAsync(),
            _ => Task.FromResult(false),
        };
    }

    public Task<bool> NavigateToAdvancedSettingsViewAsync()
    {
        return NavigateToAsync<AdvancedSettingsPageViewModel>();
    }

    public Task<bool> NavigateToCommonSettingsViewAsync(bool forceNavigation = false)
    {
        return NavigateToAsync<CommonSettingsPageViewModel>(forceNavigation: forceNavigation);
    }

    public Task<bool> NavigateToDefaultConnectionSettingsViewAsync()
    {
        return NavigateToAsync<DefaultConnectionSettingsPageViewModel>();
    }

    public Task<bool> NavigateToProtocolSettingsViewAsync()
    {
        return NavigateToAsync<ProtocolSettingsPageViewModel>();
    }

    public Task<bool> NavigateToNetShieldSettingsViewAsync()
    {
        return NavigateToAsync<NetShieldPageViewModel>();
    }

    public Task<bool> NavigateToKillSwitchSettingsViewAsync()
    {
        return NavigateToAsync<KillSwitchPageViewModel>();
    }

    public Task<bool> NavigateToPortForwardingSettingsViewAsync()
    {
        return NavigateToAsync<PortForwardingPageViewModel>();
    }

    public Task<bool> NavigateToSplitTunnelingSettingsViewAsync()
    {
        return NavigateToAsync<SplitTunnelingPageViewModel>();
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

    public void Receive(AuthenticationStatusChanged message)
    {
        switch (message.AuthenticationStatus)
        {
            case AuthenticationStatus.LoggingOut:
            case AuthenticationStatus.LoggingIn:
                // Force navigation to automatically discard any unsaved changes
                NavigateToCommonSettingsViewAsync(forceNavigation: true);
                break;
            default:
                break;
        }
    }
}