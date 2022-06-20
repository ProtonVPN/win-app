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
using System.Security.Cryptography;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.UserLogs;
using ProtonVPN.Common.Logging.Categorization.Events.UserPlanLogs;
using ProtonVPN.Core.OS.Crypto;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Storage;
using ProtonVPN.Core.User;
using CoreUser = ProtonVPN.Core.Models.User;

namespace ProtonVPN.Settings
{
    internal class UserStorage : IUserStorage
    {
        private const string FREE_VPN_PLAN = "free";

        private readonly ILogger _logger;
        private readonly ISettingsStorage _storage;
        private readonly UserSettings _userSettings;

        public event EventHandler UserDataChanged;
        public event EventHandler<VpnPlanChangedEventArgs> VpnPlanChanged;

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

        public CoreUser User()
        {
            try
            {
                return UnsafeUser();
            }
            catch (CryptographicException e)
            {
                _logger.Error<UserLog>("Failed to get user from storage", e);
            }

            return CoreUser.EmptyUser();
        }

        public void SaveLocation(UserLocation location)
        {
            _storage.Set("Ip", location.Ip.Encrypt());
            _storage.Set("Country", location.Country.Encrypt());
            _storage.Set("Isp", location.Isp.Encrypt());
        }

        public UserLocation Location()
        {
            try
            {
                return UnsafeLocation();
            }
            catch (CryptographicException ex)
            {
                _logger.Error<UserLog>("Failed to get location from storage", ex);
            }

            return UserLocation.Empty;
        }

        public void StoreVpnInfo(VpnInfoWrapperResponse vpnInfoWrapper)
        {
            sbyte maxTier = vpnInfoWrapper.Vpn.MaxTier;
            string vpnPlan = vpnInfoWrapper.Vpn.PlanName;

            if (CoreUser.IsDelinquent(vpnInfoWrapper.Delinquent))
            {
                maxTier = ServerTiers.Free;
                vpnPlan = FREE_VPN_PLAN;
            }

            CacheUser(new CoreUser
            {
                MaxTier = maxTier,
                Services = vpnInfoWrapper.Services,
                VpnPlan = vpnPlan,
                VpnPassword = vpnInfoWrapper.Vpn.Password,
                VpnUsername = vpnInfoWrapper.Vpn.Name,
                Delinquent = vpnInfoWrapper.Delinquent,
                MaxConnect = vpnInfoWrapper.Vpn.MaxConnect,
                OriginalVpnPlan = vpnInfoWrapper.Vpn.PlanName,
                Subscribed = vpnInfoWrapper.Subscribed,
                HasPaymentMethod = vpnInfoWrapper.HasPaymentMethod,
                Credit = vpnInfoWrapper.Credit,
                VpnPlanName = vpnInfoWrapper.Vpn.PlanTitle,
            });
        }

        private CoreUser UnsafeUser()
        {
            string username = _storage.Get<string>("Username")?.Trim();
            if (string.IsNullOrEmpty(username))
            {
                return CoreUser.EmptyUser();
            }

            username = username.Decrypt();

            string vpnUsername = _userSettings.Get<string>("VpnUsername");
            if (!string.IsNullOrEmpty(vpnUsername))
            {
                vpnUsername = vpnUsername.Decrypt();
            }

            string vpnPassword = _userSettings.Get<string>("VpnPassword");
            if (!string.IsNullOrEmpty(vpnPassword))
            {
                vpnPassword = vpnPassword.Decrypt();
            }

            int delinquent = _userSettings.Get<int>("Delinquent");
            string originalVpnPlan = _userSettings.Get<string>("VpnPlan");
            string vpnPlan = originalVpnPlan;
            if (CoreUser.IsDelinquent(delinquent))
            {
                vpnPlan = FREE_VPN_PLAN;
            }

            return new CoreUser
            {
                Username = username,
                VpnPlan = vpnPlan,
                MaxTier = _userSettings.Get<sbyte>("MaxTier"),
                Delinquent = delinquent,
                MaxConnect = _userSettings.Get<int>("MaxConnect"),
                Services = _userSettings.Get<int>("Services"),
                Subscribed = _userSettings.Get<int>("Subscribed"),
                HasPaymentMethod = _userSettings.Get<int>("HasPaymentMethod"),
                Credit = _userSettings.Get<int>("Credit"),
                VpnUsername = vpnUsername,
                VpnPassword = vpnPassword,
                OriginalVpnPlan = originalVpnPlan,
                VpnPlanName = _userSettings.Get<string>("VpnPlanName")
            };
        }

        public UserLocation UnsafeLocation()
        {
            string ip = _storage.Get<string>("Ip")?.Trim();
            string isp = _storage.Get<string>("Isp");
            string country = _storage.Get<string>("Country");

            if (string.IsNullOrEmpty(ip))
            {
                return UserLocation.Empty;
            }

            return new UserLocation(ip.Decrypt(), isp.Decrypt(), country.Decrypt());
        }

        private void CacheUser(CoreUser user)
        {
            CoreUser previousData = User();
            SaveUserData(user);

            if (!previousData.VpnPlan.IsNullOrEmpty() && previousData.VpnPlan != user.VpnPlan)
            {
                _logger.Info<UserPlanChangeLog>($"User plan changed from '{previousData.VpnPlan}' to '{user.VpnPlan}'.");
                VpnPlanChangedEventArgs eventArgs = new(previousData.VpnPlan, user.VpnPlan);
                VpnPlanChanged?.Invoke(this, eventArgs);
            }

            UserDataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SaveUserData(CoreUser user)
        {
            _userSettings.Set("VpnPlan", user.OriginalVpnPlan);
            _userSettings.Set("MaxTier", user.MaxTier);
            _userSettings.Set("Delinquent", user.Delinquent);
            _userSettings.Set("Subscribed", user.Subscribed);
            _userSettings.Set("HasPaymentMethod", user.HasPaymentMethod);
            _userSettings.Set("Credit", user.Credit);
            _userSettings.Set("MaxConnect", user.MaxConnect);
            _userSettings.Set("Services", user.Services);
            _userSettings.Set("VpnUsername",
                !string.IsNullOrEmpty(user.VpnUsername) ? user.VpnUsername.Encrypt() : string.Empty);
            _userSettings.Set("VpnPassword",
                !string.IsNullOrEmpty(user.VpnPassword) ? user.VpnPassword.Encrypt() : string.Empty);
            _userSettings.Set("VpnPlanName", user.VpnPlanName);
        }
    }
}