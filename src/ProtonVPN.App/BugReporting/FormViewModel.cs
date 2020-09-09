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

using System;
using System.Collections.Generic;
using ProtonVPN.Account;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Servers;

namespace ProtonVPN.BugReporting
{
    public class FormViewModel : ViewModel, IUserDataAware, ILogoutAware
    {
        private string _whatWentWrong;
        private string _stepsToReproduce;
        private string _email;
        private bool _includeLogs;

        private readonly IUserStorage _userStorage;
        private readonly Common.Configuration.Config _appConfig;

        public FormViewModel(Common.Configuration.Config appConfig, IUserStorage userStorage)
        {
            _appConfig = appConfig;
            _userStorage = userStorage;
        }

        public string Email
        {
            get => _email;
            set => Set(ref _email, value);
        }

        public string WhatWentWrong
        {
            get => _whatWentWrong;
            set => Set(ref _whatWentWrong, value);
        }

        public string StepsToReproduce
        {
            get => _stepsToReproduce;
            set => Set(ref _stepsToReproduce, value);
        }

        public bool IncludeLogs
        {
            get => _includeLogs;
            set => Set(ref _includeLogs, value);
        }

        public void Load()
        {
            ClearForm();
            LoadEmail();
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Email) &&
                   !string.IsNullOrEmpty(WhatWentWrong) &&
                   !string.IsNullOrEmpty(StepsToReproduce);
        }

        public KeyValuePair<string, string>[] GetFields()
        {
            var user = _userStorage.User();
            var location = _userStorage.Location();
            var country = Countries.GetName(location.Country);
            var isp = location.Isp;

            return new[]
            {
                new KeyValuePair<string, string>("OS", "Windows"),
                new KeyValuePair<string, string>("OSVersion", Environment.OSVersion.ToString()),
                new KeyValuePair<string, string>("Client", "Windows app"),
                new KeyValuePair<string, string>("ClientVersion", _appConfig.AppVersion),
                new KeyValuePair<string, string>("Title", "Windows app form"),
                new KeyValuePair<string, string>("Description", Description),
                new KeyValuePair<string, string>("Username", user.Username),
                new KeyValuePair<string, string>("Plan", VpnPlanHelper.GetPlanName(user.VpnPlan)),
                new KeyValuePair<string, string>("Email", Email),
                new KeyValuePair<string, string>("Country", string.IsNullOrEmpty(country) ? "" : country),
                new KeyValuePair<string, string>("ISP", string.IsNullOrEmpty(isp) ? "" : isp),
                new KeyValuePair<string, string>("ClientType", "2")
            };
        }

        public void OnUserDataChanged()
        {
            LoadEmail();
        }

        public void OnUserLoggedOut()
        {
            Email = string.Empty;
        }

        private string Description => $"What went wrong?\n\n{WhatWentWrong}\n\n" +
                                      $"What are the exact steps you performed?\n\n{StepsToReproduce}";

        private void ClearForm()
        {
            WhatWentWrong = string.Empty;
            StepsToReproduce = string.Empty;
            IncludeLogs = true;
        }

        private void LoadEmail()
        {
            var user = _userStorage.User();

            if (EmailValidator.IsValid(user.Username))
            {
                Email = user.Username;
            }
        }
    }
}
