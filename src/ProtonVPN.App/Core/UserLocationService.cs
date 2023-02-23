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

using System;
using System.Threading.Tasks;
using ProtonVPN.Api;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.Core
{
    internal class UserLocationService : IVpnStateAware, IUserLocationService, IConnectionDetailsAware
    {
        private static readonly TimeSpan UpdateLocationDelay = TimeSpan.FromSeconds(6);

        private readonly IApiClient _api;
        private readonly IUserStorage _userStorage;
        private readonly SingleAction _updateAction;
        private readonly GuestHoleState _guestHoleState;

        private bool _isUpdateWithDelayInProgress;
        private bool _disconnected = true;
        private readonly IUserAuthenticator _userAuthenticator;

        public UserLocationService(
            IApiClient api,
            IUserStorage userStorage,
            INetworkInterfaces networkInterfaces,
            IUserAuthenticator userAuthenticator,
            GuestHoleState guestHoleState)
        {
            _userAuthenticator = userAuthenticator;
            _guestHoleState = guestHoleState;
            _api = api;
            _userStorage = userStorage;

            _updateAction = new SingleAction(UpdateAction);

            networkInterfaces.NetworkAddressChanged += NetworkInterfaces_NetworkAddressChanged;
        }

        public event EventHandler<UserLocationEventArgs> UserLocationChanged;

        public Task Update()
        {
            return _updateAction.Run();
        }

        public async Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            VpnStatus status = e.State.Status;
            _disconnected = status == VpnStatus.Disconnected;

            if (status == VpnStatus.Disconnected)
            {
                await UpdateWithDelay();
            }
        }

        public async Task<ApiResponseResult<UserLocationResponse>> LocationAsync()
        {
            try
            {
                return await _api.GetLocationDataAsync();
            }
            catch
            {
                return ApiResponseResult<UserLocationResponse>.Fail(default, "");
            }
        }

        private async Task UpdateAction()
        {
            if (!_disconnected || !_userAuthenticator.IsLoggedIn)
            {
                return;
            }

            ApiResponseResult<UserLocationResponse> response = await LocationAsync();

            // Extra check in case location request took longer
            if (!_disconnected)
            {
                return;
            }

            if (response.Success)
            {
                TriggerLocationUpdate(Map(response.Value));
            }
            else
            {
                UserLocationChanged?.Invoke(this,
                    new UserLocationEventArgs(UserLocationState.Failed, UserLocation.Empty));
            }
        }

        private void TriggerLocationUpdate(UserLocation newLocation)
        {
            UserLocation userLocation = _userStorage.GetLocation();
            if (newLocation.Ip != userLocation.Ip || newLocation.Isp != userLocation.Isp)
            {
                _userStorage.SaveLocation(newLocation);
                UserLocationChanged?.Invoke(this, new UserLocationEventArgs(UserLocationState.Success, newLocation));
            }
        }

        private async Task UpdateWithDelay()
        {
            if (_isUpdateWithDelayInProgress)
            {
                return;
            }

            _isUpdateWithDelayInProgress = true;
            await Task.Delay(UpdateLocationDelay);
            await _updateAction.Run();
            _isUpdateWithDelayInProgress = false;
        }

        private async void NetworkInterfaces_NetworkAddressChanged(object sender, EventArgs e)
        {
            if (_guestHoleState.Active || !_disconnected)
            {
                return;
            }

            await UpdateWithDelay();
        }

        private UserLocation Map(UserLocationResponse contract)
        {
            return new(
                contract.Ip,
                contract.Isp,
                contract.Country);
        }

        public async Task OnConnectionDetailsChanged(ConnectionDetails connectionDetails)
        {
            if (!connectionDetails.ClientIpAddress.IsNullOrEmpty())
            {
                UserLocation currentLocation = _userStorage.GetLocation();
                string isp = currentLocation.Ip != connectionDetails.ClientIpAddress ? string.Empty : currentLocation.Isp;
                UserLocation location = new(connectionDetails.ClientIpAddress, isp, connectionDetails.ClientCountryIsoCode);
                TriggerLocationUpdate(location);
            }
        }
    }
}