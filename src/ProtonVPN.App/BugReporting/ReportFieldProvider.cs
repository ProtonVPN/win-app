/*
 * Copyright (c) 2021 Proton Technologies AG
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
using System.Text;
using ProtonVPN.Account;
using ProtonVPN.BugReporting.FormElements;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.OS;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Servers;

namespace ProtonVPN.BugReporting
{
    public class ReportFieldProvider : IReportFieldProvider
    {
        private readonly IUserStorage _userStorage;
        private readonly Common.Configuration.Config _config;
        private readonly ISystemState _systemState;

        public ReportFieldProvider(IUserStorage userStorage, Common.Configuration.Config config,
            ISystemState systemState)
        {
            _config = config;
            _userStorage = userStorage;
            _systemState = systemState;
        }

        public KeyValuePair<string, string>[] GetFields(string category, IList<FormElement> formElements)
        {
            User user = _userStorage.User();
            UserLocation location = _userStorage.Location();
            string country = Countries.GetName(location.Country);
            string isp = location.Isp;

            return new[]
            {
                new KeyValuePair<string, string>("OS", "Windows"),
                new KeyValuePair<string, string>("OSVersion", Environment.OSVersion.ToString()),
                new KeyValuePair<string, string>("Client", "Windows app"),
                new KeyValuePair<string, string>("ClientVersion", _config.AppVersion),
                new KeyValuePair<string, string>("Title", "Windows app form"),
                new KeyValuePair<string, string>("Description", GetDescription(category, formElements)),
                new KeyValuePair<string, string>("Username", user.Username),
                new KeyValuePair<string, string>("Plan", VpnPlanHelper.GetPlanName(user.VpnPlan)),
                new KeyValuePair<string, string>("Email", GetEmail(formElements)),
                new KeyValuePair<string, string>("Country", string.IsNullOrEmpty(country) ? "" : country),
                new KeyValuePair<string, string>("ISP", string.IsNullOrEmpty(isp) ? "" : isp),
                new KeyValuePair<string, string>("ClientType", "2")
            };
        }

        private string GetDescription(string category, IList<FormElement> formElements)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"Category: {category}");
            stringBuilder.AppendLine();

            foreach (FormElement element in formElements)
            {
                if (!element.Value.IsNullOrEmpty() && !element.IsEmailField())
                {
                    stringBuilder.AppendLine(element.SubmitLabel);
                    stringBuilder.AppendLine(element.Value);
                    stringBuilder.AppendLine();
                }
            }

            stringBuilder.AppendLine("Additional info");
            stringBuilder.AppendLine($"Pending reboot: {_systemState.PendingReboot().ToYesNoString()}");
            stringBuilder.AppendLine($"DeviceID: {_config.DeviceId}");

            return stringBuilder.ToString();
        }

        private string GetEmail(IList<FormElement> formElements)
        {
            FormElement emailField = formElements.GetEmailField();
            return emailField != null ? emailField.Value : string.Empty;
        }
    }
}