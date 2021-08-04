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
using System.Threading.Tasks;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core
{
    internal class UserLocationService : IVpnStateAware, IUserLocationService
    {
        private static readonly TimeSpan UpdateLocationDelay = TimeSpan.FromSeconds(6);

        private readonly IApiClient _api;
        private readonly IUserStorage _userStorage;
        private readonly SingleAction _updateAction;
        private readonly SingleAction _delayedUpdateAction;
        private readonly GuestHoleState _guestHoleState;

        private bool _disconnected = true;
        private bool _connected;
        private bool _networkAddressChanged;
        private readonly UserAuth _userAuth;

        public UserLocationService(
            IApiClient api,
            IUserStorage userStorage,
            INetworkInterfaces networkInterfaces,
            UserAuth userAuth,
            GuestHoleState guestHoleState)
        {
            _userAuth = userAuth;
            _guestHoleState = guestHoleState;
            _api = api;
            _userStorage = userStorage;

            _updateAction = new SingleAction(UpdateAction);
            _delayedUpdateAction = new SingleAction(DelayedUpdateAction);

            networkInterfaces.NetworkAddressChanged += NetworkInterfaces_NetworkAddressChanged;
        }

        public event EventHandler<UserLocationEventArgs> UserLocationChanged;

        public Task Update()
        {
            return _updateAction.Run();
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            VpnStatus status = e.State.Status;
            _disconnected = status == VpnStatus.Disconnected;
            _connected = status == VpnStatus.Connected;

            if ((status == VpnStatus.Pinging ||
                 status == VpnStatus.Connecting ||
                 status == VpnStatus.Reconnecting ||
                 status == VpnStatus.Disconnected) &&
                _networkAddressChanged)
            {
                _updateAction.Run();
            }

            return Task.CompletedTask;
        }

        public async Task<ApiResponseResult<Api.Contracts.UserLocation>> LocationAsync()
        {
            try
            {
                return await _api.GetLocationDataAsync();
            }
            catch (HttpRequestException)
            {
                return ApiResponseResult<Api.Contracts.UserLocation>.Fail(default, "");
            }
        }

        private async Task UpdateAction()
        {
            if (_connected || !_userAuth.LoggedIn)
            {
                return;
            }

            ApiResponseResult<Api.Contracts.UserLocation> response = await LocationAsync();

            // Extra check in case location request took longer
            if (_connected)
            {
                return;
            }

            if (response.Success)
            {
                _networkAddressChanged = false;

                if (response.Value.Ip == _userStorage.Location().Ip)
                {
                    return;
                }

                UserLocation location = Map(response.Value);
                _userStorage.SaveLocation(location);

                UserLocationChanged?.Invoke(this, new UserLocationEventArgs(UserLocationState.Success, location));
            }
            else
            {
                UserLocationChanged?.Invoke(this,
                    new UserLocationEventArgs(UserLocationState.Failed, UserLocation.Empty));
            }
        }

        private async Task DelayedUpdateAction()
        {
            await Task.Delay(UpdateLocationDelay);

            if (_networkAddressChanged && _disconnected)
            {
                _ = _updateAction.Run();
            }
        }

        private void NetworkInterfaces_NetworkAddressChanged(object sender, EventArgs e)
        {
            if (_guestHoleState.Active)
            {
                return;
            }

            _networkAddressChanged = true;

            if (_disconnected)
            {
                _delayedUpdateAction.Run();
            }
        }

        private UserLocation Map(Api.Contracts.UserLocation contract)
        {
            return new(
                contract.Ip,
                contract.Lat,
                contract.Long,
                contract.Isp,
                contract.Country);
        }
    }
}
