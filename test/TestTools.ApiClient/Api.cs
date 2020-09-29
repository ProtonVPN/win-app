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
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace TestTools.ApiClient
{
    public class Api
    {
        private readonly string _username;
        private readonly string _password;
        private readonly UserAuth _auth;
        private readonly Client _api;

        public Api(string username, string password)
        {
            _username = username;
            _password = password;

            var tokenStorage = new TokenStorage();
            var userStorage = Substitute.For<IUserStorage>();
            var logger = Substitute.For<ILogger>();

            _api = new Client(logger, new HttpClient
            {
                BaseAddress = new Uri("https://api.protonvpn.ch")
            }, tokenStorage);

            _auth = new UserAuth(_api, null, userStorage, tokenStorage);
        }

        public async Task<bool> Login()
        {
            return (await _auth.AuthAsync(_username, _password)).Success;
        }

        public async Task<string> GetCountry()
        {
            var locationData = await _api.GetLocationDataAsync();
            return locationData.Value.Country;
        }

        public async Task<string> GetIpAddress()
        {
            var locationData = await _api.GetLocationDataAsync();
            return locationData.Value.Ip;
        }

        public async Task DeleteProfiles()
        {
            var profiles = await _api.GetProfiles();
            foreach (var profile in profiles.Value.Profiles)
            {
                await _api.DeleteProfile(profile.Id);
            }
        }
    }

    internal class TokenStorage : ITokenStorage
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Uid { get; set; }
    }
}
