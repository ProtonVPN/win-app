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

using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Settings;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.ConnectionInfo
{
    public class ConnectionErrorResolver : IVpnStateAware
    {
        private readonly IUserStorage _userStorage;
        private readonly IApiClient _api;
        private readonly ServerManager _serverManager;
        private readonly IServerUpdater _serverUpdater;
        private Server _server = Server.Empty();

        public ConnectionErrorResolver(
            IUserStorage userStorage,
            IApiClient api,
            ServerManager serverManager,
            IServerUpdater serverUpdater)
        {
            _serverUpdater = serverUpdater;
            _serverManager = serverManager;
            _userStorage = userStorage;
            _api = api;
        }

        public async Task<VpnError> ResolveError()
        {
            var oldUserInfo = _userStorage.User();
            if (oldUserInfo.Delinquent > 2)
            {
                return VpnError.Unpaid;
            }

            if (await GetSessionCount() >= oldUserInfo.MaxConnect)
            {
                return VpnError.SessionLimitReached;
            }

            await UpdateUserInfo();
            var newUserInfo = _userStorage.User();

            if (oldUserInfo.VpnPassword != newUserInfo.VpnPassword)
            {
                return VpnError.PasswordChanged;
            }

            if (newUserInfo.MaxTier < oldUserInfo.MaxTier)
            {
                return VpnError.UserTierTooLowError;
            }

            await _serverUpdater.Update();
            var server = _serverManager.GetServer(new ServerById(_server.Id));
            if (server == null)
            {
                _serverManager.MarkServerUnderMaintenance(_server.ExitIp);
                return VpnError.ServerRemoved;
            }

            if (server.Status == 0)
            {
                _serverManager.MarkServerUnderMaintenance(_server.ExitIp);
                return VpnError.ServerOffline;
            }

            return VpnError.Unknown;
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            _server = e.State.Server;
            return Task.CompletedTask;
        }

        private async Task<int> GetSessionCount()
        {
            try
            {
                var response = await _api.GetSessions();
                if (response.Success)
                    return response.Value.Sessions.Count;
            }
            catch (HttpRequestException)
            {
            }

            return 0;
        }

        private async Task UpdateUserInfo()
        {
            try
            {
                var result = await _api.GetVpnInfoResponse();
                if (result.Success)
                {
                    _userStorage.StoreVpnInfo(result.Value);
                }
            }
            catch (HttpRequestException)
            {
            }
        }
    }
}
