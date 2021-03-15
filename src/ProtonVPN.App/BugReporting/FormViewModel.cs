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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Translations;
using ProtonVPN.Validation;

namespace ProtonVPN.BugReporting
{
    public class FormViewModel : ValidationViewModel, IUserDataAware, ILogoutAware
    {
        private string _whatWentWrong;
        private string _stepsToReproduce;
        private string _email;
        private bool _includeLogs;

        private readonly IUserStorage _userStorage;
        private readonly IReportFieldProvider _reportFieldProvider;

        public FormViewModel(IUserStorage userStorage, IReportFieldProvider reportFieldProvider)
        {
            _userStorage = userStorage;
            _reportFieldProvider = reportFieldProvider;
        }

        public string Email
        {
            get => _email;
            set
            {
                Set(ref _email, value);
                Validate();
            }
        }

        public string WhatWentWrong
        {
            get => _whatWentWrong;
            set
            {
                Set(ref _whatWentWrong, value);
                Validate();
            }
        }

        public string StepsToReproduce
        {
            get => _stepsToReproduce;
            set
            {
                Set(ref _stepsToReproduce, value);
                Validate();
            }
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

        public KeyValuePair<string, string>[] GetFields()
        {
            return _reportFieldProvider.GetFields(Description, Email);
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
            User user = _userStorage.User();

            if (EmailValidator.IsValid(user.Username))
            {
                Email = user.Username;
            }
        }

        public new bool HasErrors => base.HasErrors || string.IsNullOrEmpty(Email);

        private void Validate([CallerMemberName] string field = null)
        {
            switch (field)
            {
                case nameof(Email):
                    if (!EmailValidator.IsValid(Email))
                    {
                        SetError(nameof(Email), Translation.Get("BugReport_msg_EmailNotValid"));
                    }
                    else
                    {
                        ClearError(field);
                    }

                    break;
                case nameof(WhatWentWrong):
                    if (string.IsNullOrEmpty(WhatWentWrong))
                    {
                        SetError(nameof(WhatWentWrong), "empty");
                    }
                    else
                    {
                        ClearError(field);
                    }

                    break;
                case nameof(StepsToReproduce):
                    if (string.IsNullOrEmpty(StepsToReproduce))
                    {
                        SetError(nameof(StepsToReproduce), "empty");
                    }
                    else
                    {
                        ClearError(field);
                    }

                    break;
            }
        }
    }
}