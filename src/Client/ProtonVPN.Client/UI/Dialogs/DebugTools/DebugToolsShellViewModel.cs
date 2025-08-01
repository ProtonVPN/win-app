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
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Lifecycle;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Dialogs.DebugTools.Models;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace ProtonVPN.Client.UI.Dialogs.DebugTools;

public partial class DebugToolsShellViewModel : ShellViewModelBase<IDebugToolsWindowActivator>
{
    private readonly IServersUpdater _serversUpdater;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly INpsSurveyWindowActivator _npsSurveyWindowActivator;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IAppExitInvoker _appExitInvoker;

    [ObservableProperty]
    private Overlay _selectedOverlay;

    [ObservableProperty]
    private VpnErrorTypeIpcEntity _selectedError = VpnErrorTypeIpcEntity.None;

    [ObservableProperty]
    private VpnPlan _selectedVpnPlan;

    [ObservableProperty]
    private int _xPosition; 
    [ObservableProperty]
    private int _yPosition;
    [ObservableProperty]
    private int _windowWidth;
    [ObservableProperty]
    private int _windowHeight;

    public List<Overlay> OverlaysList { get; }

    public List<VpnPlan> VpnPlans { get; } =
    [
        new("VPN Free", "vpnfree", 0),
        new("VPN Plus", "vpnplus", 1),
        new("Proton Unlimited", "bundle2022", 1),
        new("Proton Visionary", "visionary2022", 1),
        new("Proton Business", "vpnpro2023", 1),
        new("Proton Duo", "duo2024", 1),
    ];

    public DebugToolsShellViewModel(
        IVpnServiceCaller vpnServiceCaller,
        IServersUpdater serversUpdater,
        IUserAuthenticator userAuthenticator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        INpsSurveyWindowActivator npsSurveyWindowActivator,
        ISettings settings,
        IEventMessageSender eventMessageSender,
        IDebugToolsWindowActivator windowActivator,
        IViewModelHelper viewModelHelper,
        IAppExitInvoker appExitInvoker)
        : base(windowActivator, viewModelHelper)
    {
        _serversUpdater = serversUpdater;
        _vpnServiceCaller = vpnServiceCaller;
        _userAuthenticator = userAuthenticator;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _npsSurveyWindowActivator = npsSurveyWindowActivator;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
        _appExitInvoker = appExitInvoker;

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
        SelectedOverlay = OverlaysList.First();

        SelectedVpnPlan = VpnPlans.First();
    }

    [RelayCommand]
    public async Task TriggerRestartAsync()
    {
        await _appExitInvoker.RestartAsync();
    }

    [RelayCommand]
    public void TriggerClientCrash()
    {
        throw new StackOverflowException("Intentional crash test");
    }

    [RelayCommand]
    public async Task TriggerLogicalsRefreshAsync()
    {
        await _serversUpdater.ForceUpdateAsync();
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

    [RelayCommand]
    public void SimulatePlanChanged()
    {
        VpnPlan oldPlan = _settings.VpnPlan;
        VpnPlan newPlan = SelectedVpnPlan;

        _settings.VpnPlan = newPlan;
        _eventMessageSender.Send(new VpnPlanChangedMessage(oldPlan, newPlan));
    }

    [RelayCommand]
    public void DisconnectWithSessionLimitReachedError()
    {
        _vpnServiceCaller.DisconnectAsync(new DisconnectionRequestIpcEntity()
        {
            RetryId = Guid.NewGuid(),
            ErrorType = VpnErrorTypeIpcEntity.SessionLimitReachedPlus
        });
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

    [RelayCommand]
    public void ShowNpsSurvey()
    {
        _npsSurveyWindowActivator.Activate();
    }

    [RelayCommand]
    public void SetWindowPosition()
    {
        (App.Current as App)?.MainWindow?.MoveAndResize(
            new WindowPositionParameters()
            {
                XPosition = XPosition,
                YPosition = YPosition,
                Width = WindowWidth,
                Height = WindowHeight
            });
    }

    [RelayCommand]
    public void ResetWindowPosition()
    {
        (App.Current as App)?.MainWindow?.MoveAndResize(
            new WindowPositionParameters()
            {
                XPosition = null,
                YPosition = null,
                Width = DefaultSettings.WindowWidth,
                Height = DefaultSettings.WindowHeight
            });
    }
}