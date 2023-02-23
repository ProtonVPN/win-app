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

using System.Threading.Tasks;
using ProtonVPN.Common.OS.Processes;

namespace ProtonVPN.Account
{
    public class SubscriptionManager : ISubscriptionManager
    {
        public const string REDIRECT_ENDPOINT = "refresh-account";

        private readonly IOsProcesses _processes;
        private readonly IWebAuthenticator _webAuthenticator;

        public SubscriptionManager(IOsProcesses processes, IWebAuthenticator webAuthenticator)
        {
            _processes = processes;
            _webAuthenticator = webAuthenticator;
        }

        public async Task UpgradeAccountAsync()
        {
            await OpenLoginUrl("upgrade");
        }

        public async Task ManageSubscriptionAsync()
        {
            await OpenLoginUrl("manage-subscription");
        }

        private async Task OpenLoginUrl(string type)
        {
            LoginUrlParams urlParams = new()
            {
                Action = "subscribe-account",
                Fullscreen = "off",
                Redirect = REDIRECT_ENDPOINT,
                Start = "compare",
                Type = type,
            };
            _processes.Open(await _webAuthenticator.GetLoginUrlAsync(urlParams));
        }
    }
}