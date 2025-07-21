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
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Services.Mapping;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Navigation.Bases;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.UI.Main.Settings.Connection;
using ProtonVPN.Client.UI.Main.Settings.Pages;
using ProtonVPN.Client.UI.Main.Settings.Pages.About;
using ProtonVPN.Client.UI.Main.Settings.Pages.Advanced;
using ProtonVPN.Client.UI.Main.Settings.Pages.Connection;
using ProtonVPN.Client.UI.Main.Settings.Pages.DefaultConnections;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Navigation;

public class SettingsViewNavigator : ViewNavigatorBase, ISettingsViewNavigator,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    public override bool IsNavigationStackEnabled => true;

    public SettingsViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher)
        : base(logger, pageViewMapper, uiThreadDispatcher)
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

    public Task<bool> NavigateToFeatureViewAsync(ConnectionFeature feature, bool isDirectNavigation = false)
    {
        return feature switch
        {
            ConnectionFeature.NetShield => NavigateToNetShieldSettingsViewAsync(isDirectNavigation),
            ConnectionFeature.KillSwitch => NavigateToKillSwitchSettingsViewAsync(isDirectNavigation),
            ConnectionFeature.PortForwarding => NavigateToPortForwardingSettingsViewAsync(isDirectNavigation),
            ConnectionFeature.SplitTunneling => NavigateToSplitTunnelingSettingsViewAsync(isDirectNavigation),
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

    public Task<bool> NavigateToDefaultConnectionSettingsViewAsync(bool isDirectNavigation = false)
    {
        return NavigateToAsync<DefaultConnectionSettingsPageViewModel>(parameter: isDirectNavigation);
    }

    public Task<bool> NavigateToProtocolSettingsViewAsync(bool isDirectNavigation = false)
    {
        return NavigateToAsync<ProtocolSettingsPageViewModel>(parameter: isDirectNavigation);
    }

    public Task<bool> NavigateToNetShieldSettingsViewAsync(bool isDirectNavigation = false)
    {
        return NavigateToAsync<NetShieldPageViewModel>(parameter: isDirectNavigation);
    }

    public Task<bool> NavigateToKillSwitchSettingsViewAsync(bool isDirectNavigation = false)
    {
        return NavigateToAsync<KillSwitchPageViewModel>(parameter: isDirectNavigation);
    }

    public Task<bool> NavigateToPortForwardingSettingsViewAsync(bool isDirectNavigation = false)
    {
        return NavigateToAsync<PortForwardingPageViewModel>(parameter: isDirectNavigation);
    }

    public Task<bool> NavigateToSplitTunnelingSettingsViewAsync(bool isDirectNavigation = false)
    {
        return NavigateToAsync<SplitTunnelingPageViewModel>(parameter: isDirectNavigation);
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
            case AuthenticationStatus.LoggedIn:
                // Force navigation to automatically discard any unsaved changes
                UIThreadDispatcher.TryEnqueue(async () => await NavigateToCommonSettingsViewAsync(forceNavigation: true));
                break;
            default:
                break;
        }
    }
}