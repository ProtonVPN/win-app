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

using ProtonVPN.Common.Logging;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.OS.Crypto;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Storage;
using System;
using System.Globalization;
using System.Security.Cryptography;

namespace ProtonVPN.Core.Settings
{
    internal class UserStorage : IUserStorage
    {
        private readonly ILogger _logger;
        private readonly ISettingsStorage _storage;
        private readonly UserSettings _userSettings;

        public event EventHandler UserDataChanged;
        public event EventHandler<string> VpnPlanChanged;

        public UserStorage(
            ILogger logger,
            ISettingsStorage storage,
            UserSettings userSettings)
        {
            _logger = logger;
            _storage = storage;
            _userSettings = userSettings;
        }

        public void SaveUsername(string username)
        {
            _storage.Set("Username", username.Encrypt());
        }

        public void SetFreePlan()
        {
            _userSettings.Set("VpnPlan", "free");
            _userSettings.Set("ExpirationTime", 0);
            _userSettings.Set("MaxTier", ServerTiers.Free);

            VpnPlanChanged?.Invoke(this, "free");
            UserDataChanged?.Invoke(this, EventArgs.Empty);
        }

        public Models.User User()
        {
            try
            {
                return UnsafeUser();
            }
            catch (CryptographicException e)
            {
                _logger.Error(e);
            }

            return Models.User.EmptyUser();
        }

        public void SaveLocation(UserLocation location)
        {
            _storage.Set("Ip", location.Ip.Encrypt());
            _storage.Set("Country", location.Country.Encrypt());
            _storage.Set("Isp", location.Isp.Encrypt());
            _storage.Set("Latitude", location.Latitude.ToString(CultureInfo.InvariantCulture).Encrypt());
            _storage.Set("Longitude", location.Longitude.ToString(CultureInfo.InvariantCulture).Encrypt());
        }

        public UserLocation Location()
        {
            try
            {
                return UnsafeLocation();
            }
            catch (CryptographicException ex)
            {
                _logger.Error(ex);
            }

            return UserLocation.Empty;
        }

        public void ClearLogin()
        {
            _storage.Set("Username", "");
        }

        public void StoreVpnInfo(VpnInfoResponse vpnInfo)
        {
            CacheUser(new Models.User
            {
                ExpirationTime = vpnInfo.Vpn.ExpirationTime,
                MaxTier = vpnInfo.Vpn.MaxTier,
                Services = vpnInfo.Services,
                VpnPlan = vpnInfo.Vpn.PlanName,
                VpnPassword = vpnInfo.Vpn.Password,
                VpnUsername = vpnInfo.Vpn.Name,
                Delinquent = vpnInfo.Delinquent,
                MaxConnect = vpnInfo.Vpn.MaxConnect
            });
        }

        private Models.User UnsafeUser()
        {
            var username = _storage.Get<string>("Username")?.Trim();
            if (string.IsNullOrEmpty(username))
                return Models.User.EmptyUser();

            username = username.Decrypt();

            var vpnUsername = _userSettings.Get<string>("VpnUsername");
            if (!string.IsNullOrEmpty(vpnUsername))
                vpnUsername = vpnUsername.Decrypt();

            var vpnPassword = _userSettings.Get<string>("VpnPassword");
            if (!string.IsNullOrEmpty(vpnPassword))
                vpnPassword = vpnPassword.Decrypt();

            return new Models.User
            {
                Username = username,
                VpnPlan = _userSettings.Get<string>("VpnPlan"),
                MaxTier = _userSettings.Get<sbyte>("MaxTier"),
                Delinquent = _userSettings.Get<int>("Delinquent"),
                ExpirationTime = _userSettings.Get<int>("ExpirationTime"),
                MaxConnect = _userSettings.Get<int>("MaxConnect"),
                Services = _userSettings.Get<int>("Services"),
                VpnUsername = vpnUsername,
                VpnPassword = vpnPassword
            };
        }

        public UserLocation UnsafeLocation()
        {
            var ip = _storage.Get<string>("Ip")?.Trim();
            var latitude = _storage.Get<string>("Latitude")?.Trim();
            var longitude = _storage.Get<string>("Longitude")?.Trim();
            var isp = _storage.Get<string>("Isp");
            var country = _storage.Get<string>("Country");

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
            {
                return UserLocation.Empty;
            }

            var latitudeFloat = float.Parse(latitude.Decrypt(), CultureInfo.InvariantCulture.NumberFormat);
            var longitudeFloat = float.Parse(longitude.Decrypt(), CultureInfo.InvariantCulture.NumberFormat);
            return new UserLocation(ip.Decrypt(), latitudeFloat, longitudeFloat, isp.Decrypt(), country.Decrypt());
        }

        private void SaveUserData(Models.User user)
        {
            _userSettings.Set("VpnPlan", user.VpnPlan);
            _userSettings.Set("MaxTier", user.MaxTier);
            _userSettings.Set("Delinquent", user.Delinquent);
            _userSettings.Set("ExpirationTime", user.ExpirationTime);
            _userSettings.Set("MaxConnect", user.MaxConnect);
            _userSettings.Set("Services", user.Services);
            _userSettings.Set("VpnUsername", !string.IsNullOrEmpty(user.VpnUsername) ? user.VpnUsername.Encrypt() : string.Empty);
            _userSettings.Set("VpnPassword", !string.IsNullOrEmpty(user.VpnPassword) ? user.VpnPassword.Encrypt() : string.Empty);
        }

        private void CacheUser(Models.User user)
        {
            var previousData = User();
            SaveUserData(user);

            if (previousData.VpnPlan != user.VpnPlan)
                VpnPlanChanged?.Invoke(this, user.VpnPlan);

            UserDataChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
