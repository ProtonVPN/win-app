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

using GalaSoft.MvvmLight.Command;
using Polly.Timeout;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Resources;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Windows.Input;

namespace ProtonVPN.Login.ViewModels
{
    public class LoginViewModel : ViewModel, ISettingsAware
    {
        private string _errorText = "";

        private readonly Common.Configuration.Config _appConfig;
        private readonly IAppSettings _appSettings;
        private readonly LoginWindowViewModel _loginWindowViewModel;
        private readonly IActiveUrls _urls;
        private readonly UserAuth _userAuth;

        public LoginErrorViewModel LoginErrorViewModel { get; }

        private string _loginText;
        private string _password;
        private bool _showHelpBalloon;
        private bool _autoAuthFailed;
        private bool _forcedLogout;

        public LoginViewModel(
            Common.Configuration.Config appConfig,
            LoginWindowViewModel loginWindowViewModel,
            IActiveUrls urls,
            IAppSettings appSettings,
            LoginErrorViewModel loginErrorViewModel,
            UserAuth userAuth)
        {
            _appConfig = appConfig;
            _userAuth = userAuth;
            _appSettings = appSettings;
            _urls = urls;
            _loginWindowViewModel = loginWindowViewModel;
            LoginErrorViewModel = loginErrorViewModel;

            LoginCommand = new RelayCommand(LoginAction);
            RegisterCommand = new RelayCommand(RegisterAction);
            HelpCommand = new RelayCommand(HelpAction);
            ToggleHelpBalloon = new RelayCommand(ToggleBalloonAction);
            ResetPasswordCommand = new RelayCommand(ResetPasswordAction);
            ForgotUsernameCommand = new RelayCommand(ForgotUsernameAction);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand ToggleHelpBalloon { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand ForgotUsernameCommand { get; }

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
                    Password = "";
                }

                if (!Set(ref _loginText, value))
                    return;

                OnPropertyChanged();
                OnPropertyChanged(nameof(FieldsFilledIn));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (!string.IsNullOrEmpty(value) && _password != value && _autoAuthFailed)
                {
                    _autoAuthFailed = false;
                }

                if (!Set(ref _password, value))
                    return;

                OnPropertyChanged();
                OnPropertyChanged(nameof(FieldsFilledIn));
            }
        }

        public bool FieldsFilledIn => !string.IsNullOrEmpty(LoginText?.Trim()) && !string.IsNullOrEmpty(Password);

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

        public bool RememberLogin
        {
            get => _appSettings.RememberLogin;
            set => _appSettings.RememberLogin = value;
        }

        public bool StartOnStartup
        {
            get => _appSettings.StartOnStartup;
            set => _appSettings.StartOnStartup = value;
        }

        public void OnSessionExpired()
        {
            LoginErrorViewModel.SetStandardError(StringResources.Get("Login_Error_msg_SessionExpired"));
        }

        public void OnForcedLogout(string message)
        {
            LoginErrorViewModel.SetOutdatedError(message);
            _forcedLogout = true;
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(IAppSettings.StartOnStartup)))
                OnPropertyChanged(nameof(StartOnStartup));
        }

        private void HelpAction()
        {
            _urls.HelpUrl.Open();
        }

        private void RegisterAction()
        {
            _urls.RegisterUrl.Open();
        }

        private bool AllowLogin()
        {
            LoginText = LoginText?.Trim();

            if (string.IsNullOrEmpty(LoginText) || string.IsNullOrEmpty(Password))
            {
                return false;
            }

            return true;
        }

        private async void LoginAction()
        {
            try
            {
                if (!AllowLogin())
                    return;

                var loginResult = await _userAuth.LoginUserAsync(LoginText, Password);
                if (loginResult.Success)
                {
                    AfterLogin();
                }
                else
                {
                    if (!_forcedLogout)
                    {
                        var error = loginResult.Error;
                        if (loginResult.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            error = StringResources.Get("Login_Error_msg_Unauthorized");
                        }

                        LoginErrorViewModel.SetStandardError(error);
                    }

                    Password = "";
                    ShowLoginForm();
                }
            }
            catch (HttpRequestException e)
            {
                LoginErrorViewModel.SetDetailedError(GetLoginError(e));
                Password = "";
                ShowLoginForm();
            }
        }

        private string GetLoginError(Exception e)
        {
            if (e.InnerException is TimeoutRejectedException)
            {
                return StringResources.Get("Login_Error_msg_Timeout");
            }

            return $"{e.Message} {e.MostInner()}";
        }

        private void ShowLoginForm()
        {
            _loginWindowViewModel.CurrentPageViewModel = this;
        }

        private void AfterLogin()
        {
            LoginText = "";
            Password = "";
            ErrorText = "";
            LoginErrorViewModel.ClearError();
            _autoAuthFailed = false;
            _forcedLogout = false;
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
    }
}
