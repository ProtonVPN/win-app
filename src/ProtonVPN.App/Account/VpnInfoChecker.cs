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
using System.Net.Http;
using System.Threading;
using Caliburn.Micro;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Window;

namespace ProtonVPN.Account
{
    public class VpnInfoChecker : IHandle<WindowStateMessage>
    {
        private readonly TimeSpan _checkInterval;
        private readonly IApiClient _api;
        private readonly IUserStorage _userStorage;
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private DateTime _lastCheck = DateTime.Now;

        public VpnInfoChecker(TimeSpan checkInterval, IEventAggregator eventAggregator, IApiClient api, IUserStorage userStorage)
        {
            eventAggregator.Subscribe(this);

            _checkInterval = checkInterval;
            _api = api;
            _userStorage = userStorage;
        }

        public async void Handle(WindowStateMessage message)
        {
            if (!message.Active || DateTime.Now.Subtract(_lastCheck) < _checkInterval)
            {
                return;
            }

            _lastCheck = DateTime.Now;
            await Semaphore.WaitAsync();

            try
            {
                var response = await _api.GetVpnInfoResponse();
                if (response.Success)
                    _userStorage.StoreVpnInfo(response.Value);
            }
            catch (HttpRequestException)
            {
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}
