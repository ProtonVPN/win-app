/*
 * Copyright (c) 2022 Proton Technologies AG
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
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.BugReporting.FormElements;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.OS;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.OS;
using ProtonVPN.Core.Settings;
using ProtonVPN.Servers;
using UserLocation = ProtonVPN.Core.User.UserLocation;

namespace ProtonVPN.BugReporting
{
    public class ReportFieldProvider : IReportFieldProvider
    {
        private readonly IUserStorage _userStorage;
        private readonly Common.Configuration.Config _config;
        private readonly ISystemState _systemState;
        private readonly IDeviceInfoProvider _deviceInfoProvider;

        public ReportFieldProvider(IUserStorage userStorage, Common.Configuration.Config config,
            ISystemState systemState, IDeviceInfoProvider deviceInfoProvider)
        {
            _config = config;
            _userStorage = userStorage;
            _systemState = systemState;
            _deviceInfoProvider = deviceInfoProvider;
        }

        public KeyValuePair<string, string>[] GetFields(SendReportAction message)
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
                new KeyValuePair<string, string>("Description", GetDescription(message)),
                new KeyValuePair<string, string>("Username", user.Username),
                new KeyValuePair<string, string>("Plan", user.VpnPlanName),
                new KeyValuePair<string, string>("Email", GetEmail(message.FormElements)),
                new KeyValuePair<string, string>("Country", string.IsNullOrEmpty(country) ? "" : country),
                new KeyValuePair<string, string>("ISP", string.IsNullOrEmpty(isp) ? "" : isp),
                new KeyValuePair<string, string>("ClientType", "2")
            };
        }

        private string GetDescription(SendReportAction message)
        {
            StringBuilder stringBuilder = new();
            AppendCategoryToDescription(stringBuilder, message);
            AppendFormElementsToDescription(stringBuilder, message);
            AppendAdditionalInformationToDescription(stringBuilder);

            return stringBuilder.ToString();
        }

        private void AppendCategoryToDescription(StringBuilder stringBuilder, SendReportAction message)
        {
            stringBuilder.AppendLine($"Category: {message.Category}").AppendLine();
        }

        private void AppendFormElementsToDescription(StringBuilder stringBuilder, SendReportAction message)
        {
            foreach (FormElement element in message.FormElements)
            {
                if (!element.Value.IsNullOrEmpty() && !element.IsEmailField())
                {
                    stringBuilder.AppendLine(element.SubmitLabel).AppendLine(element.Value).AppendLine();
                }
            }
        }

        private void AppendAdditionalInformationToDescription(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("Additional info")
                .AppendLine($"Pending reboot: {_systemState.PendingReboot().ToYesNoString()}")
                .AppendLine($"DeviceID: {_deviceInfoProvider.GetDeviceId()}");
        }

        private string GetEmail(IList<FormElement> formElements)
        {
            FormElement emailField = formElements.GetEmailField();
            return emailField != null ? emailField.Value : string.Empty;
        }
    }
}