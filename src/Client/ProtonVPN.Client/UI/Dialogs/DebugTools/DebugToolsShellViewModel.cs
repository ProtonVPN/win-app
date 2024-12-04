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

using System.Reflection;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Updaters;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;
using ProtonVPN.Client.UI.Dialogs.DebugTools.Models;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.UI.Dialogs.DebugTools;

public partial class DebugToolsShellViewModel : ShellViewModelBase<IDebugToolsWindowActivator>
{
    private readonly IServersUpdater _serversUpdater;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IRequiredReconnectionSettings _requiredReconnectionSettings;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly ISettings _settings;
    private readonly ISettingsConflictResolver _settingsConflictResolver;
    private readonly IConnectionManager _connectionManager;
    private readonly IEventMessageSender _eventMessageSender;

    [ObservableProperty]
    private Overlay? _selectedOverlay;

    [ObservableProperty]
    private VpnErrorTypeIpcEntity _selectedError = VpnErrorTypeIpcEntity.None;

    public List<Overlay> OverlaysList { get; }

    public DebugToolsShellViewModel(
        IDebugToolsWindowActivator windowActivator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IServersUpdater serversUpdater,
        IUserAuthenticator userAuthenticator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IEventMessageSender eventMessageSender)
        : base(windowActivator,
               localizer,
               logger,
               issueReporter)
    {
        _serversUpdater = serversUpdater;
        _userAuthenticator = userAuthenticator;
        _requiredReconnectionSettings = requiredReconnectionSettings;
        _mainViewNavigator = mainViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _settings = settings;
        _settingsConflictResolver = settingsConflictResolver;
        _connectionManager = connectionManager;
        _eventMessageSender = eventMessageSender;

        OverlaysList =
        [
            ..typeof(IMainWindowOverlayActivator).GetMethods()
                    .Where(m => m.GetParameters().Length == 0)
                    .Select(m => new Overlay
                    {
                        Id =  m.Name,
                        Name = GenerateOverlayDisplayName(m.Name)
                    })
            .ToList()
        ];
    }

    [RelayCommand]
    public void TriggerAppCrash()
    {
        throw new StackOverflowException("Intentional crash test");
    }

    [RelayCommand]
    public async Task TriggerLogicalsRefreshAsync()
    {
        await _serversUpdater.UpdateAsync(ServersRequestParameter.ForceFullUpdate, true);
    }

    [RelayCommand]
    public async Task LogoutUserWithClientOutdatedReasonAsync()
    {
        await _userAuthenticator.LogoutAsync(LogoutReason.ClientOutdated);
    }

    [RelayCommand]
    public void ShowOverlay()
    {
        if (SelectedOverlay != null)
        {
            MethodInfo? methodInfo = _mainWindowOverlayActivator.GetType().GetMethod(SelectedOverlay.Id);
            methodInfo?.Invoke(_mainWindowOverlayActivator, null);
        }
    }

    [RelayCommand]
    public void ResetInfoBanners()
    {
        _settings.IsGatewayInfoBannerDismissed = false;
        _settings.IsP2PInfoBannerDismissed = false;
        _settings.IsSecureCoreInfoBannerDismissed = false;
        _settings.IsTorInfoBannerDismissed = false;
    }

    [RelayCommand]
    public void SimulatePlanChangedToPlus()
    {
        VpnPlan oldPlan = _settings.VpnPlan;
        VpnPlan newPlan = new("VPN Plus (simulation)", "vpnplus", 1);

        _settings.VpnPlan = newPlan;
        _eventMessageSender.Send(new VpnPlanChangedMessage(oldPlan, newPlan));
    }

    [RelayCommand]
    public void SimulatePlanChangedToFree()
    {
        VpnPlan oldPlan = _settings.VpnPlan;
        VpnPlan newPlan = new("VPN Free (simulation)", "vpnfree", 0);

        _settings.VpnPlan = newPlan;
        _eventMessageSender.Send(new VpnPlanChangedMessage(oldPlan, newPlan));
    }

    private string GenerateOverlayDisplayName(string methodName)
    {
        string displayName = Regex.Replace(methodName, "^(Show)", "", RegexOptions.IgnoreCase);
        displayName = Regex.Replace(displayName, "(OverlayAsync)$", "", RegexOptions.IgnoreCase);

        // Insert spaces before uppercase letters, handling acronyms properly (e.g., VPN, B2B)
        displayName = Regex.Replace(displayName, "(?<=[a-z])([A-Z])", " $1");

        return displayName.Trim();
    }

    [RelayCommand]
    private void TriggerConnectionError()
    {
        _eventMessageSender.Send(new VpnStateIpcEntity()
        {
            Error = SelectedError
        });
    }
}