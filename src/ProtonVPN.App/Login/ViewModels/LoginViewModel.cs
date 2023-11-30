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

using System;
using System.ComponentModel;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Api;
using ProtonVPN.BugReporting;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.ErrorHandling;
using ProtonVPN.FlashNotifications;
using ProtonVPN.Modals;
using ProtonVPN.Translations;
using ProtonVPN.Vpn.Connectors;
using ProtonVPN.Login.Enums;
using ProtonVPN.Core.FeatureFlags;

namespace ProtonVPN.Login.ViewModels
{
    public class LoginViewModel : ViewModel, ISettingsAware, IVpnStateAware, IFeatureFlagsAware
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _appConfig;
        private readonly IAppSettings _appSettings;
        private readonly LoginWindowViewModel _loginWindowViewModel;
        private readonly IActiveUrls _urls;
        private readonly IUserAuthenticator _userAuthenticator;
        private readonly IModals _modals;
        private readonly GuestHoleConnector _guestHoleConnector;
        private readonly GuestHoleState _guestHoleState;
        private readonly IApiAvailabilityVerifier _apiAvailabilityVerifier;
        private readonly IFeatureFlagsProvider _featureFlagsProvider;

        public LoginErrorViewModel LoginErrorViewModel { get; }

        public FlashNotificationViewModel FlashNotificationViewModel { get; }

        private string _loginText;
        private SecureString _password;
        private bool _showHelpBalloon;
        private bool _autoAuthFailed;
        private bool _networkBlocked;

        private VpnStatus _lastVpnStatus = VpnStatus.Disconnected;
        private string _twoFactorAuthCode;
        private LoginFormType _formType = LoginFormType.UsernameAndPassword;

        public bool IsToShowUsernameAndPassword => FormType == LoginFormType.UsernameAndPassword;

        public bool IsToShowTwoFactorAuth => FormType == LoginFormType.TwoFactorAuthentication;

        public bool IsToShowSingleSignOn => FormType == LoginFormType.SingleSignOn;

        public LoginFormType FormType
        {
            get => _formType;
            set
            {
                if (Set(ref _formType, value))
                {
                    OnPropertyChanged(nameof(IsToShowUsernameAndPassword));
                    OnPropertyChanged(nameof(IsToShowTwoFactorAuth));
                    OnPropertyChanged(nameof(IsToShowSingleSignOn));
                    OnPropertyChanged(nameof(IsLoginAllowed));

                    (SwitchToSingleSignOnCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (SwitchToUsernameAndPasswordCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isToShowSignUpSpinner;
        public bool IsToShowSignUpSpinner
        {
            get => _isToShowSignUpSpinner;
            set => Set(ref _isToShowSignUpSpinner, value);
        }

        public LoginViewModel(
            ILogger logger,
            IConfiguration appConfig,
            LoginWindowViewModel loginWindowViewModel,
            IActiveUrls urls,
            IAppSettings appSettings,
            LoginErrorViewModel loginErrorViewModel,
            FlashNotificationViewModel flashNotificationViewModel,
            IUserAuthenticator userAuthenticator,
            IModals modals,
            GuestHoleConnector guestHoleConnector,
            GuestHoleState guestHoleState,
            IApiAvailabilityVerifier apiAvailabilityVerifier,
            IFeatureFlagsProvider featureFlagsProvider)
        {
            _logger = logger;
            _appConfig = appConfig;
            _userAuthenticator = userAuthenticator;
            _appSettings = appSettings;
            _urls = urls;
            _modals = modals;
            _loginWindowViewModel = loginWindowViewModel;
            _guestHoleConnector = guestHoleConnector;
            _guestHoleState = guestHoleState;
            _apiAvailabilityVerifier = apiAvailabilityVerifier;
            _featureFlagsProvider = featureFlagsProvider;

            LoginErrorViewModel = loginErrorViewModel;
            LoginErrorViewModel.ClearError();
            FlashNotificationViewModel = flashNotificationViewModel;

            LoginCommand = new RelayCommand(LoginAction);
            RegisterCommand = new RelayCommand(RegisterAction);
            HelpCommand = new RelayCommand(HelpAction);
            ToggleHelpBalloon = new RelayCommand(ToggleBalloonAction);
            ResetPasswordCommand = new RelayCommand(ResetPasswordAction);
            ForgotUsernameCommand = new RelayCommand(ForgotUsernameAction);
            ReportAnIssueCommand = new RelayCommand(ReportAnIssueAction);
            OpenSignInIssuesWebPageCommand = new RelayCommand(OpenSignInIssuesWebPageAction);
            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitchAction);
            SwitchToSingleSignOnCommand = new RelayCommand(SwitchToSingleSignOnAction, CanSwitchToSingleSignOnAction);
            SwitchToUsernameAndPasswordCommand = new RelayCommand(SwitchToUsernameAndPasswordAction, CanSwitchToUsernameAndPasswordAction);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand ToggleHelpBalloon { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand ForgotUsernameCommand { get; }
        public ICommand ReportAnIssueCommand { get; }
        public ICommand OpenSignInIssuesWebPageCommand { get; }
        public ICommand DisableKillSwitchCommand { get; }
        public ICommand SwitchToSingleSignOnCommand { get; }
        public ICommand SwitchToUsernameAndPasswordCommand { get; }

        public string AppVersion => $"v.{_appConfig.AppVersion}";

        public bool ShowHelpBalloon
        {
            get => _showHelpBalloon;
            set => Set(ref _showHelpBalloon, value);
        }

        public string LoginText
        {
            get => _loginText;
            set
            {
                if (_loginText != null && _loginText != value && _autoAuthFailed)
                {
                    _autoAuthFailed = false;
                    ClearPasswordField();
                }

                if (!Set(ref _loginText, value))
                {
                    return;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLoginAllowed));
            }
        }

        public SecureString Password
        {
            get => _password;
            set
            {
                if (value != null && value.Length != 0 && _autoAuthFailed)
                {
                    _autoAuthFailed = false;
                }

                Set(ref _password, value);

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLoginAllowed));
            }
        }

        public bool IsLoginAllowed => FormType switch
        {
            LoginFormType.UsernameAndPassword => !string.IsNullOrWhiteSpace(LoginText) && Password != null && Password.Length != 0,
            LoginFormType.TwoFactorAuthentication => !string.IsNullOrWhiteSpace(TwoFactorAuthCode),
            LoginFormType.SingleSignOn => !string.IsNullOrWhiteSpace(LoginText) && _featureFlagsProvider.IsSsoEnabled,
            _ => false
        };
            
            

        public string TwoFactorAuthCode
        {
            get => _twoFactorAuthCode;
            set
            {
                if (Set(ref _twoFactorAuthCode, value))
                {
                    OnPropertyChanged(nameof(IsLoginAllowed));
                }
            }
        }

        public bool StartAndConnectOnBoot
        {
            get => _appSettings.ConnectOnAppStart && _appSettings.StartOnBoot;
            set
            {
                _appSettings.ConnectOnAppStart = value;
                _appSettings.StartOnBoot = value;
                OnPropertyChanged();
            }
        }

        public bool KillSwitchActive
        {
            get => _networkBlocked;
            set => Set(ref _networkBlocked, value);
        }

        public KillSwitchMode KillSwitchMode => _appSettings.KillSwitchMode;

        public void OnSessionExpired()
        {
            LoginErrorViewModel.SetError(Translation.Get("Login_Error_msg_SessionExpired"));
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(IAppSettings.StartOnBoot) or nameof(IAppSettings.ConnectOnAppStart))
            {
                OnPropertyChanged(nameof(StartAndConnectOnBoot));
            }
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            SetKillSwitchActive(e.NetworkBlocked, e.State.Status);

            if (_lastVpnStatus == e.State.Status)
            {
                return;
            }

            if (_guestHoleState.Active)
            {
                switch (e.State.Status)
                {
                    case VpnStatus.Connected when IsToShowSignUpSpinner:
                        IsToShowSignUpSpinner = false;
                        _urls.RegisterUrl.Open();
                        break;
                    case VpnStatus.Connected:
                        LoginAction();
                        break;
                    case VpnStatus.Disconnected:
                        _guestHoleState.SetState(false);
                        await ShowLoginScreenWithTroubleshootAsync();
                        IsToShowSignUpSpinner = false;
                        break;
                }
            }

            _lastVpnStatus = e.State.Status;
        }

        public void OnFeatureFlagsChanged()
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                OnPropertyChanged(nameof(IsLoginAllowed));

                (SwitchToSingleSignOnCommand as RelayCommand)?.RaiseCanExecuteChanged();

                // If currently on SSO login page but SSO feature flag is not enabled, switch back to User & Password
                if (!_featureFlagsProvider.IsSsoEnabled && IsToShowSingleSignOn)
                {
                    FormType = LoginFormType.UsernameAndPassword;
                }
            });
        }

        private void SetKillSwitchActive(bool isNetworkBlocked, VpnStatus vpnStatus)
        {
            if (isNetworkBlocked && vpnStatus == VpnStatus.Disconnecting ||
                vpnStatus == VpnStatus.Disconnected)
            {
                KillSwitchActive = true;
            }

            if (!isNetworkBlocked)
            {
                KillSwitchActive = false;
            }
        }

        private void HelpAction()
        {
            _urls.HelpUrl.Open();
        }

        private async void RegisterAction()
        {
            IsToShowSignUpSpinner = true;
            bool isSignUpPageAccessible = await _apiAvailabilityVerifier.IsSignUpPageAccessibleAsync();
            if (isSignUpPageAccessible)
            {
                IsToShowSignUpSpinner = false;
                _urls.RegisterUrl.Open();
            }
            else
            {
                await ConnectToGuestHoleAsync();
            }
        }

        private async Task ConnectToGuestHoleAsync()
        {
            _guestHoleState.SetState(true);
            await _guestHoleConnector.Connect();
        }

        private async void LoginAction()
        {
            switch (FormType)
            {
                case LoginFormType.UsernameAndPassword:
                    await HandleLoginAsync();
                    break;
                case LoginFormType.TwoFactorAuthentication:
                    await HandleTwoFactorAuthAsync();
                    break;
                case LoginFormType.SingleSignOn:
                    await HandleSingleSignOnAsync();
                    break;
                default:
                    break;
            }
        }

        private void SwitchToSingleSignOnAction()
        {
            FormType = LoginFormType.SingleSignOn;
        }

        private bool CanSwitchToSingleSignOnAction()
        {
            return IsToShowUsernameAndPassword && _featureFlagsProvider.IsSsoEnabled;
        }

        private void SwitchToUsernameAndPasswordAction()
        {
            FormType = LoginFormType.UsernameAndPassword;
        }

        private bool CanSwitchToUsernameAndPasswordAction()
        {
            return IsToShowSingleSignOn;
        }

        private async Task HandleLoginAsync()
        {
            try
            {
                if (!IsLoginAllowed)
                {
                    return;
                }

                LoginErrorViewModel.ClearError();

                AuthResult loginResult = await _userAuthenticator.LoginUserAsync(LoginText?.Trim(), Password);
                await HandleLoginResultAsync(loginResult);
            }
            catch
            {
                if (await DisableGuestHole() || _guestHoleConnector.Servers().Count == 0)
                {
                    await ShowLoginScreenWithTroubleshootAsync();
                    return;
                }

                await ConnectToGuestHoleAsync();
            }
        }

        private async Task HandleTwoFactorAuthAsync()
        {
            AuthResult result = await SendTwoFactorAuthRequestAsync();
            if (result.Failure)
            {
                await HandleTwoFactorAuthFailureAsync(result);
            }
            else
            {
                AfterLogin();
            }
        }

        private async Task HandleSingleSignOnAsync()
        {
            try
            {
                if (!IsLoginAllowed)
                {
                    return;
                }

                LoginErrorViewModel.ClearError();

                AuthResult loginResult = await _userAuthenticator.SingleSignOnUserAsync(LoginText?.Trim());
                await HandleLoginResultAsync(loginResult);
            }
            catch
            { }
        }

        private async Task HandleTwoFactorAuthFailureAsync(AuthResult result)
        {
            string error = result.Error;
            TwoFactorAuthCode = string.Empty;

            switch (result.Value)
            {
                case AuthError.NoVpnAccess:
                    await _modals.ShowAsync<AssignVpnConnectionsModalViewModel>();
                    FormType = LoginFormType.UsernameAndPassword;
                    ClearPasswordField();
                    ShowLoginForm();
                    return;
                case AuthError.IncorrectTwoFactorCode:
                    error = Translation.Get("Login_msg_IncorrectTwoFactorCode");
                    break;
                case AuthError.TwoFactorAuthFailed:
                    error = Translation.Get("Login_msg_TwoFactorAuthFailed");
                    FormType = LoginFormType.UsernameAndPassword;
                    ClearPasswordField();
                    break;
                case AuthError.Unknown:
                    await _modals.ShowAsync<TroubleshootModalViewModel>();
                    ShowLoginForm();
                    return;
            }

            LoginErrorViewModel.SetError(error);
            ShowLoginForm();
        }

        private async Task<AuthResult> SendTwoFactorAuthRequestAsync()
        {
            try
            {
                return await _userAuthenticator.SendTwoFactorCodeAsync(TwoFactorAuthCode);
            }
            catch (Exception ex)
            {
                _logger.Error<AppLog>("Failed to send two factor auth code.", ex);
                return AuthResult.Fail(AuthError.Unknown);
            }
        }

        private async Task HandleLoginResultAsync(AuthResult result)
        {
            if (result.Success)
            {
                AfterLogin();
            }
            else
            {
                await HandleLoginFailureAsync(result);
            }
        }

        public async Task HandleAuthFailureAsync(AuthResult result)
        {
            if (!result.Error.IsNullOrEmpty())
            {
                LoginErrorViewModel.SetError(result.Error);
            }

            await HandleAuthErrorAsync(result.Value);
        }

        private async Task HandleAuthErrorAsync(AuthError error)
        {
            switch (error)
            {
                case AuthError.NoVpnAccess:
                    await _modals.ShowAsync<AssignVpnConnectionsModalViewModel>();
                    break;
                case AuthError.MissingGoSrpDll:
                    _logger.Fatal<AppCrashLog>("The app is missing GoSrp.dll");
                    FatalErrorHandler fatalErrorHandler = new();
                    fatalErrorHandler.Exit("The file \"GoSrp.dll\" is missing.");
                    break;
                case AuthError.SwitchToSSO:
                    FormType = _featureFlagsProvider.IsSsoEnabled 
                        ? LoginFormType.SingleSignOn
                        : LoginFormType.UsernameAndPassword;
                    break;
                case AuthError.SwitchToSRP:
                    FormType = LoginFormType.UsernameAndPassword;
                    break;
            }
        }

        private async Task HandleLoginFailureAsync(AuthResult result)
        {
            if (result.Value == AuthError.TwoFactorRequired)
            {
                FormType = LoginFormType.TwoFactorAuthentication;
                ShowLoginForm();
                return;
            }

            await HandleAuthFailureAsync(result);
            ClearPasswordField();
            ShowLoginForm();
            await DisableGuestHole();
        }

        private async Task<bool> DisableGuestHole()
        {
            if (!_guestHoleState.Active)
            {
                return false;
            }

            await _guestHoleConnector.Disconnect();
            _guestHoleState.SetState(false);

            return true;
        }

        private async Task ShowLoginScreenWithTroubleshootAsync()
        {
            await _modals.ShowAsync<TroubleshootModalViewModel>();
            ClearPasswordField();
            ShowLoginForm();
        }

        private void ClearPasswordField()
        {
            Password = new SecureString();
        }

        private void ShowLoginForm()
        {
            _loginWindowViewModel.CurrentPageViewModel = this;
        }

        private void AfterLogin()
        {
            LoginText = "";
            ClearPasswordField();
            LoginErrorViewModel.ClearError();
            _autoAuthFailed = false;
            FormType = LoginFormType.UsernameAndPassword;
            TwoFactorAuthCode = string.Empty;
        }

        private void ToggleBalloonAction()
        {
            ShowHelpBalloon = !ShowHelpBalloon;
        }

        private void ResetPasswordAction()
        {
            _urls.PasswordResetUrl.Open();
        }

        private void ForgotUsernameAction()
        {
            _urls.ForgetUsernameUrl.Open();
        }

        private async void ReportAnIssueAction()
        {
            await _modals.ShowAsync<ReportBugModalViewModel>();
        }

        private void OpenSignInIssuesWebPageAction()
        {
            _urls.LoginProblemsUrl.Open();
        }

        private void DisableKillSwitchAction()
        {
            _appSettings.KillSwitchMode = KillSwitchMode.Off;
        }
    }
}