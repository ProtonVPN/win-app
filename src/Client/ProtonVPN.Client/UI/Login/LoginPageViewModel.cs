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
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Enums;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Navigation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Login.Pages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.GuestHoleLogs;

namespace ProtonVPN.Client.UI.Login;

public partial class LoginPageViewModel : PageViewModelBase<IMainWindowViewNavigator, ILoginViewNavigator>,
    IEventMessageReceiver<LoginStateChangedMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IUrls _urls;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IReportIssueWindowActivator _reportIssueWindowActivator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;

    [ObservableProperty]
    private string _message;

    [ObservableProperty]
    private bool _isMessageVisible;

    [ObservableProperty]
    private InfoBarSeverity _messageType = InfoBarSeverity.Error;

    [ObservableProperty]
    private bool _isHelpVisible;

    public LoginPageViewModel(
        IMainWindowViewNavigator parentViewNavigator,
        ILoginViewNavigator childViewNavigator,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IUrls urls,
        IMainWindowActivator mainWindowActivator,
        IReportIssueWindowActivator reportIssueWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator)
        : base(parentViewNavigator, childViewNavigator, localizer, logger, issueReporter)
    {
        _urls = urls;
        _mainWindowActivator = mainWindowActivator;
        _reportIssueWindowActivator = reportIssueWindowActivator;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _message = string.Empty;
    }

    public async void Receive(LoginStateChangedMessage message)
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

            case LoginState.Error:
                HandleAuthError(message);
                break;
        }
    }

    public void Receive(LoggedOutMessage message)
    {
        switch (message.Reason)
        {
            case LogoutReason.UserAction:
            case LogoutReason.NoVpnConnectionsAssigned:
                break;
            case LogoutReason.SessionExpired:
                ExecuteOnUIThread(() => SetErrorMessage(Localizer.Get("Login_Error_SessionExpired")));
                break;
        }
    }

    private void HandleAuthError(LoginStateChangedMessage message)
    {
        switch (message.AuthError)
        {
            case AuthError.MissingGoSrpDll:
                Logger.Fatal<AppCrashLog>("The app is missing GoSrp.dll");
                // VPNWIN-2109 - Add modal about missing file
                _mainWindowActivator.Exit();
                break;

            case AuthError.GuestHoleFailed:
                Logger.Error<GuestHoleLog>("Failed to authenticate using guest hole.");
                _mainWindowOverlayActivator.ShowTroubleshootingOverlayAsync();
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
        _urls.NavigateTo(_urls.ResetPassword);
    }

    [RelayCommand]
    public void ForgotUsername()
    {
        _urls.NavigateTo(_urls.ForgotUsername);
    }

    [RelayCommand]
    public void TroubleSigningIn()
    {
        _urls.NavigateTo(_urls.TroubleSigningIn);
    }

    [RelayCommand]
    public void ReportAnIssue()
    {
        _reportIssueWindowActivator.Activate();
    }

    private void SetErrorMessage(string message)
    {
        SetMessage(message, InfoBarSeverity.Error);
    }

    private void SetMessage(string message, InfoBarSeverity messageType)
    {
        MessageType = messageType;
        Message = message;
        IsMessageVisible = true;
    }

    private void ClearMessage()
    {
        IsMessageVisible = false;
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsKillSwitchEnabled) && ChildViewNavigator.GetCurrentPageContext() is SignInPageViewModel)
        {
            SetMessage(Localizer.Get("SignIn_KillSwitch_Disabled"), InfoBarSeverity.Success);
        }
    }
}