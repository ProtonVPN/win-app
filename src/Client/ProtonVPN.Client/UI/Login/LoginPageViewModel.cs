/*
 * Copyright (c) 2025 Proton AG
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
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Login.Pages;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.GuestHoleLogs;

namespace ProtonVPN.Client.UI.Login;

public partial class LoginPageViewModel : PageViewModelBase<IMainWindowViewNavigator, ILoginViewNavigator>,
    IEventMessageReceiver<LoginStateChangedMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IReportIssueWindowActivator _reportIssueWindowActivator;
    private readonly ITroubleshootingWindowActivator _troubleshootingWindowActivator;
    private readonly ISettings _settings;
    private readonly IDebugToolsWindowActivator _debugToolsWindowActivator;
    private readonly IApplicationIconSelector _applicationIconSelector;

    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private bool _isMessageVisible;

    [ObservableProperty]
    private InfoBarSeverity _messageType = InfoBarSeverity.Error;

    [ObservableProperty]
    private bool _isHelpVisible;

    public bool IsDebugModeEnabled => _settings.IsDebugModeEnabled;

    public LoginPageViewModel(
        IMainWindowViewNavigator parentViewNavigator,
        ILoginViewNavigator childViewNavigator,
        IUrlsBrowser urlsBrowser,
        IMainWindowActivator mainWindowActivator,
        IReportIssueWindowActivator reportIssueWindowActivator,
        ITroubleshootingWindowActivator troubleshootingWindowActivator,
        ISettings settings,
        IDebugToolsWindowActivator debugToolsWindowActivator,
        IApplicationIconSelector applicationIconSelector,
        IViewModelHelper viewModelHelper)
        : base( parentViewNavigator, childViewNavigator, viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _mainWindowActivator = mainWindowActivator;
        _reportIssueWindowActivator = reportIssueWindowActivator;
        _troubleshootingWindowActivator = troubleshootingWindowActivator;
        _settings = settings;
        _debugToolsWindowActivator = debugToolsWindowActivator;
        _applicationIconSelector = applicationIconSelector;

        _message = string.Empty;
    }

    public void Receive(LoginStateChangedMessage message)
    {
        ExecuteOnUIThread(async () =>
        {
            switch (message.Value)
            {
                case LoginState.Authenticating:
                    ClearMessage();
                    break;

                case LoginState.Success:
                    ClearMessage();
                    break;

                case LoginState.TwoFactorRequired:
                    ClearMessage();
                    await ChildViewNavigator.NavigateToTwoFactorViewAsync();
                    break;

                case LoginState.TwoFactorFailed:
                    switch (message.AuthError)
                    {
                        case AuthError.IncorrectTwoFactorCode:
                            SetErrorMessage(Localizer.Get("Login_Error_IncorrectTwoFactorCode"));
                            await ChildViewNavigator.NavigateToTwoFactorViewAsync();
                            break;

                        case AuthError.TwoFactorAuthFailed:
                            SetErrorMessage(Localizer.Get("Login_Error_TwoFactorFailed"));
                            await ChildViewNavigator.NavigateToSignInViewAsync();
                            break;

                        case AuthError.Unknown:
                            SetErrorMessage(message.ErrorMessage);
                            break;
                    }
                    break;

                case LoginState.TwoFactorCancelled:
                    await ChildViewNavigator.NavigateToSignInViewAsync();
                    break;

                case LoginState.Error:
                    await ChildViewNavigator.NavigateToSignInViewAsync();
                    HandleAuthError(message);
                    break;
            }
        });
    }

    public void Receive(LoggedOutMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            switch (message.Reason)
            {
                case LogoutReason.UserAction:
                case LogoutReason.NoVpnConnectionsAssigned:
                    break;
                case LogoutReason.SessionExpired:
                    SetErrorMessage(Localizer.Get("Login_Error_SessionExpired"));
                    break;
            }
        });
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (message.PropertyName == nameof(ISettings.IsKillSwitchEnabled) &&
                !_settings.IsKillSwitchEnabled &&
                ParentViewNavigator.GetCurrentPageContext() is LoginPageViewModel && 
                ChildViewNavigator.GetCurrentPageContext() is SignInPageViewModel)
            {
                SetMessage(Localizer.Get("SignIn_KillSwitch_Disabled"), InfoBarSeverity.Success);
            }
        });
    }

    private void HandleAuthError(LoginStateChangedMessage message)
    {
        if (message.AuthError == AuthError.None)
        {
            return;
        }

        switch (message.AuthError)
        {
            case AuthError.MissingGoSrpDll:
                Logger.Fatal<AppCrashLog>("The app is missing GoSrp.dll");
                // VPNWIN-2109 - Add modal about missing file
                _mainWindowActivator.Exit();
                break;

            case AuthError.GuestHoleFailed:
                Logger.Error<GuestHoleLog>("Failed to authenticate using guest hole.");
                _troubleshootingWindowActivator.Activate();
                break;

            case AuthError.SsoAuthFailed:
                SetErrorMessage(Localizer.Get("Login_Error_SsoAuthFailed"));
                break;

            case AuthError.GetSessionDetailsFailed:
                SetErrorMessage(Localizer.Get("Login_Error_GetSessionDetailsFailed"));
                break;

            default:
                SetErrorMessage(message.ErrorMessage);
                break;
        }
    }

    protected override void OnChildNavigation(NavigationEventArgs e)
    {
        base.OnChildNavigation(e);

        IsHelpVisible = e.Content is not LoadingPageView;
    }

    [RelayCommand]
    public void ResetPassword()
    {
        _urlsBrowser.BrowseTo(_urlsBrowser.ResetPassword);
    }

    [RelayCommand]
    public void ForgotUsername()
    {
        _urlsBrowser.BrowseTo(_urlsBrowser.ForgotUsername);
    }

    [RelayCommand]
    public void TroubleSigningIn()
    {
        _urlsBrowser.BrowseTo(_urlsBrowser.TroubleSigningIn);
    }

    [RelayCommand]
    public Task ReportAnIssueAsync()
    {
        return _reportIssueWindowActivator.ActivateAsync();
    }

    [RelayCommand(CanExecute = nameof(CanShowDebugTools))]
    private void ShowDebugTools()
    {
        _debugToolsWindowActivator.Activate();
    }

    private bool CanShowDebugTools()
    {
        return IsDebugModeEnabled;
    }

    private void SetErrorMessage(string message)
    {
        SetMessage(message, InfoBarSeverity.Error);
    }

    private void SetMessage(string message, InfoBarSeverity messageType)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Logger.Warn<AppLog>($"Tried to set an empty error message on login page. Stack trace: {Environment.StackTrace}");

            ClearMessage();
            return;
        }

        MessageType = messageType;
        Message = message;
        IsMessageVisible = true;
    }

    private void ClearMessage()
    {
        IsMessageVisible = false;
    }

    partial void OnIsMessageVisibleChanged(bool value)
    {
        if (IsMessageVisible && MessageType == InfoBarSeverity.Error)
        {
            _applicationIconSelector.OnAuthenticationErrorTriggered();
        }
        else
        {
            _applicationIconSelector.OnAuthenticationErrorDismissed();
        }
    }
}