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
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Geographical;
using ProtonVPN.Api.Contracts.Profiles;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Settings;

namespace TestTools.ApiClient
{
    public class Api
    {
        private readonly string _username;
        private readonly string _password;
        private readonly UserAuthenticator _userAuthenticator;
        private readonly Client _api;

        public Api(string username, string password)
        {
            _username = username;
            _password = password;

            IAppSettings appSettings = Substitute.For<IAppSettings>();
            IConfiguration config = new Config { ApiVersion = "3" };
            IUserStorage userStorage = Substitute.For<IUserStorage>();
            ILogger logger = Substitute.For<ILogger>();
            IAuthCertificateManager authCertificateManager = Substitute.For<IAuthCertificateManager>();
            IAppLanguageCache appLanguageCache = Substitute.For<IAppLanguageCache>();
            appLanguageCache.GetCurrentSelectedLanguageIetfTag().Returns("en");

            _api = new Client(config, logger, new HttpClient
            {
                BaseAddress = new Uri("https://api.protonvpn.ch")
            }, appSettings, appLanguageCache);

            _userAuthenticator = new UserAuthenticator(_api, null, userStorage, appSettings, authCertificateManager);
        }

        public async Task<bool> Login()
        {
            return (await _userAuthenticator.AuthAsync(_username, ToSecureString(_password))).Success;
        }

        public async Task<string> GetCountry()
        {
            ApiResponseResult<UserLocationResponse> locationData = await _api.GetLocationDataAsync();
            return locationData.Value.Country;
        }

        public async Task<string> GetIpAddress()
        {
            ApiResponseResult<UserLocationResponse> locationData = await _api.GetLocationDataAsync();
            return locationData.Value.Ip;
        }

        public async Task DeleteProfiles()
        {
            ApiResponseResult<ProfilesResponse> profiles = await _api.GetProfiles();
            foreach (ProfileResponse profile in profiles.Value.Profiles)
            {
                await _api.DeleteProfile(profile.Id);
            }
        }

        private SecureString ToSecureString(string value)
        {
            SecureString result = new();
            foreach (char c in value)
            {
                result.AppendChar(c);
            }

            return result;
        }
    }
}