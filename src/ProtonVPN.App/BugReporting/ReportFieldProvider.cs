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
using ProtonVPN.Account;
using ProtonVPN.BugReporting.Diagnostic;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Models;
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

        public ReportFieldProvider(IUserStorage userStorage, Common.Configuration.Config config, ISystemState systemState)
        {
            _config = config;
            _userStorage = userStorage;
            _systemState = systemState;
        }

        public KeyValuePair<string, string>[] GetFields(string description, string email)
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
                new KeyValuePair<string, string>("Description", GetDescription(description)),
                new KeyValuePair<string, string>("Username", user.Username),
                new KeyValuePair<string, string>("Plan", VpnPlanHelper.GetPlanName(user.VpnPlan)),
                new KeyValuePair<string, string>("Email", email),
                new KeyValuePair<string, string>("Country", string.IsNullOrEmpty(country) ? "" : country),
                new KeyValuePair<string, string>("ISP", string.IsNullOrEmpty(isp) ? "" : isp),
                new KeyValuePair<string, string>("ClientType", "2")
            };
        }

        private string GetDescription(string description)
        {
            return $"{description}\n\nAdditional info:\n" +
                   $"Pending reboot: {_systemState.PendingReboot().ToYesNoString()}\n" +
                   $"DeviceID: {_config.DeviceId}";
        }
    }
}