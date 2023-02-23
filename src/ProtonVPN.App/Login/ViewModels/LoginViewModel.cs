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
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.ErrorHandling;
using ProtonVPN.Modals;
using ProtonVPN.Translations;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Login.ViewModels
{
    public class LoginViewModel : ViewModel, ISettingsAware, IServiceSettingsStateAware
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

        public LoginErrorViewModel LoginErrorViewModel { get; }

        private string _loginText;
        private SecureString _password;
        private bool _showHelpBalloon;
        private bool _autoAuthFailed;
        private bool _networkBlocked;

        private VpnStatus _lastVpnStatus = VpnStatus.Disconnected;
        private bool _isToShowUsernameAndPassword = true;
        private bool _isToShowTwoFactorAuth;
        private string _twoFactorAuthCode;

        public bool IsToShowUsernameAndPassword
        {
            get => _isToShowUsernameAndPassword;
            set => Set(ref _isToShowUsernameAndPassword, value);
        }

        public bool IsToShowTwoFactorAuth
        {
            get => _isToShowTwoFactorAuth;
            set => Set(ref _isToShowTwoFactorAuth, value);
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
            IUserAuthenticator userAuthenticator,
            IModals modals,
            GuestHoleConnector guestHoleConnector,
            GuestHoleState guestHoleState,
            IApiAvailabilityVerifier apiAvailabilityVerifier)
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

            LoginErrorViewModel = loginErrorViewModel;
            LoginErrorViewModel.ClearError();

            LoginCommand = new RelayCommand(LoginAction);
            RegisterCommand = new RelayCommand(RegisterAction);
            HelpCommand = new RelayCommand(HelpAction);
            ToggleHelpBalloon = new RelayCommand(ToggleBalloonAction);
            ResetPasswordCommand = new RelayCommand(ResetPasswordAction);
            ForgotUsernameCommand = new RelayCommand(ForgotUsernameAction);
            ReportAnIssueCommand = new RelayCommand(ReportAnIssueAction);
            OpenSignInIssuesWebPageCommand = new RelayCommand(OpenSignInIssuesWebPageAction);
            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitchAction);
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

        public bool IsLoginAllowed => !string.IsNullOrEmpty(LoginText?.Trim()) && Password != null && Password.Length != 0;

        public string TwoFactorAuthCode
        {
            get => _twoFactorAuthCode;
            set => Set(ref _twoFactorAuthCode, value);
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
                        ShowLoginScreenWithTroubleshoot();
                        IsToShowSignUpSpinner = false;
                        break;
                }
            }

            _lastVpnStatus = e.State.Status;
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

        private bool IsLoginDisallowed(string username, SecureString password)
        {
            return string.IsNullOrEmpty(username) || password == null || password.Length == 0;
        }

        private async void LoginAction()
        {
            if (IsToShowTwoFactorAuth)
            {
                await HandleTwoFactorAuthAsync();
                return;
            }

            try
            {
                string username = LoginText?.Trim();

                if (IsLoginDisallowed(username, Password))
                {
                    return;
                }

                LoginErrorViewModel.ClearError();

                AuthResult loginResult = await _userAuthenticator.LoginUserAsync(username, Password);
                await HandleLoginResultAsync(loginResult);
            }
            catch
            {
                if (await DisableGuestHole() || _guestHoleConnector.Servers().Count == 0)
                {
                    ShowLoginScreenWithTroubleshoot();
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
                HandleTwoFactorAuthFailure(result);
            }
            else
            {
                AfterLogin();
            }
        }

        private void HandleTwoFactorAuthFailure(AuthResult result)
        {
            string error = result.Error;
            TwoFactorAuthCode = string.Empty;

            switch (result.Value)
            {
                case AuthError.NoVpnAccess:
                    _modals.Show<AssignVpnConnectionsModalViewModel>();
                    IsToShowTwoFactorAuth = false;
                    IsToShowUsernameAndPassword = true;
                    ClearPasswordField();
                    ShowLoginForm();
                    return;
                case AuthError.IncorrectTwoFactorCode:
                    error = Translation.Get("Login_msg_IncorrectTwoFactorCode");
                    break;
                case AuthError.TwoFactorAuthFailed:
                    error = Translation.Get("Login_msg_TwoFactorAuthFailed");
                    IsToShowTwoFactorAuth = false;
                    IsToShowUsernameAndPassword = true;
                    break;
                case AuthError.Unknown:
                    _modals.Show<TroubleshootModalViewModel>();
                    ShowLoginForm();
                    return;
            }

            LoginErrorViewModel.SetError(error);
            ClearPasswordField();
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

        public void HandleAuthFailure(AuthResult result)
        {
            if (!result.Error.IsNullOrEmpty())
            {
                LoginErrorViewModel.SetError(result.Error);
            }

            HandleAuthError(result.Value);
        }

        private void HandleAuthError(AuthError error)
        {
            switch (error)
            {
                case AuthError.NoVpnAccess:
                    _modals.Show<AssignVpnConnectionsModalViewModel>();
                    break;
                case AuthError.MissingGoSrpDll:
                    _logger.Fatal<AppCrashLog>("The app is missing GoSrp.dll");
                    FatalErrorHandler fatalErrorHandler = new();
                    fatalErrorHandler.Exit("The file \"GoSrp.dll\" is missing.");
                    break;
            }
        }

        private async Task HandleLoginFailureAsync(AuthResult result)
        {
            if (result.Value == AuthError.TwoFactorRequired)
            {
                IsToShowUsernameAndPassword = false;
                IsToShowTwoFactorAuth = true;
                ShowLoginForm();
                return;
            }

            HandleAuthFailure(result);
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

        private void ShowLoginScreenWithTroubleshoot()
        {
            _modals.Show<TroubleshootModalViewModel>();
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
            IsToShowUsernameAndPassword = true;
            IsToShowTwoFactorAuth = false;
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

        private void ReportAnIssueAction()
        {
            _modals.Show<ReportBugModalViewModel>();
        }

        private void OpenSignInIssuesWebPageAction()
        {
            _urls.LoginProblemsUrl.Open();
        }

        private void DisableKillSwitchAction()
        {
            _appSettings.KillSwitchMode = KillSwitchMode.Off;
        }

        public void OnServiceSettingsStateChanged(ServiceSettingsStateChangedEventArgs e)
        {
            SetKillSwitchActive(e.IsNetworkBlocked, e.CurrentState.State.Status);
        }
    }
}