/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.KillSwitch;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Modals;
using ProtonVPN.Translations;
using ProtonVPN.Vpn.Connectors;

namespace ProtonVPN.Login.ViewModels
{
    public class LoginViewModel : ViewModel, ISettingsAware, IServiceSettingsStateAware
    {
        private string _errorText = "";

        private readonly Common.Configuration.Config _appConfig;
        private readonly IAppSettings _appSettings;
        private readonly LoginWindowViewModel _loginWindowViewModel;
        private readonly IActiveUrls _urls;
        private readonly UserAuth _userAuth;
        private readonly IModals _modals;
        private readonly GuestHoleConnector _guestHoleConnector;
        private readonly GuestHoleState _guestHoleState;

        public LoginErrorViewModel LoginErrorViewModel { get; }

        private string _loginText;
        private SecureString _password;
        private bool _showHelpBalloon;
        private bool _autoAuthFailed;
        private bool _networkBlocked;
        private VpnStatus _lastVpnStatus = VpnStatus.Disconnected;

        public LoginViewModel(
            Common.Configuration.Config appConfig,
            LoginWindowViewModel loginWindowViewModel,
            IActiveUrls urls,
            IAppSettings appSettings,
            LoginErrorViewModel loginErrorViewModel,
            UserAuth userAuth,
            IModals modals,
            GuestHoleConnector guestHoleConnector,
            GuestHoleState guestHoleState)
        {
            _appConfig = appConfig;
            _userAuth = userAuth;
            _appSettings = appSettings;
            _urls = urls;
            _modals = modals;
            _loginWindowViewModel = loginWindowViewModel;
            _guestHoleConnector = guestHoleConnector;
            _guestHoleState = guestHoleState;
            LoginErrorViewModel = loginErrorViewModel;

            LoginErrorViewModel.ClearError();

            LoginCommand = new RelayCommand(LoginAction);
            RegisterCommand = new RelayCommand(RegisterAction);
            HelpCommand = new RelayCommand(HelpAction);
            ToggleHelpBalloon = new RelayCommand(ToggleBalloonAction);
            ResetPasswordCommand = new RelayCommand(ResetPasswordAction);
            ForgotUsernameCommand = new RelayCommand(ForgotUsernameAction);
            DisableKillSwitchCommand = new RelayCommand(DisableKillSwitchAction);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand ToggleHelpBalloon { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand ForgotUsernameCommand { get; }
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
                    Password = new SecureString();
                }

                if (!Set(ref _loginText, value))
                    return;

                OnPropertyChanged();
                OnPropertyChanged(nameof(FieldsFilledIn));
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
                OnPropertyChanged(nameof(FieldsFilledIn));
            }
        }

        public bool FieldsFilledIn => !string.IsNullOrEmpty(LoginText?.Trim()) && Password != null && Password.Length != 0;

        public string ErrorText
        {
            get => _errorText;
            set
            {
                if (value == _errorText) return;
                _errorText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FieldsFilledIn));
            }
        }

        public bool StartOnStartup
        {
            get => _appSettings.StartOnStartup;
            set => _appSettings.StartOnStartup = value;
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
            if (e.PropertyName.Equals(nameof(IAppSettings.StartOnStartup)))
            {
                OnPropertyChanged(nameof(StartOnStartup));
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
                if (e.State.Status == VpnStatus.Connected)
                {
                    LoginAction();
                }
                else if (e.State.Status == VpnStatus.Disconnected)
                {
                    _guestHoleState.SetState(false);
                    ShowLoginScreenWithTroubleshoot();
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

        private void RegisterAction()
        {
            _urls.RegisterUrl.Open();
        }

        private bool IsLoginDisallowed(string username, SecureString password)
        {
            return string.IsNullOrEmpty(username) || password == null || password.Length == 0;
        }

        private async void LoginAction()
        {
            try
            {
                string username = LoginText?.Trim();

                if (IsLoginDisallowed(username, Password))
                {
                    return;
                }

                LoginErrorViewModel.ClearError();

                ApiResponseResult<AuthResponse> loginResult = await _userAuth.LoginUserAsync(username, Password);
                await HandleLoginResultAsync(loginResult);
            }
            catch (HttpRequestException)
            {
                if (await DisableGuestHole() || _guestHoleConnector.Servers().Count == 0)
                {
                    ShowLoginScreenWithTroubleshoot();
                    return;
                }

                _guestHoleState.SetState(true);
                await _guestHoleConnector.Connect();
            }
        }

        private async Task HandleLoginResultAsync(ApiResponseResult<AuthResponse> loginResult)
        {
            if (loginResult.Success)
            {
                AfterLogin();
            }
            else
            {
                await HandleLoginFailureAsync(loginResult);
            }
        }

        private async Task HandleLoginFailureAsync(ApiResponseResult<AuthResponse> loginResult)
        {
            if (loginResult.Actions.IsNullOrEmpty()) // If Actions exist, it should be handled by ActionableFailureApiResultEventHandler
            {
                string error = loginResult.Error;
                if (loginResult.StatusCode == HttpStatusCode.Unauthorized)
                {
                    error = Translation.Get("Login_Error_msg_Unauthorized");
                }

                LoginErrorViewModel.SetError(error);
            }

            Password = new SecureString();
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
            Password = new SecureString();
            ShowLoginForm();
        }

        private void ShowLoginForm()
        {
            _loginWindowViewModel.CurrentPageViewModel = this;
        }

        private void AfterLogin()
        {
            LoginText = "";
            Password = new SecureString();
            ErrorText = "";
            LoginErrorViewModel.ClearError();
            _autoAuthFailed = false;
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