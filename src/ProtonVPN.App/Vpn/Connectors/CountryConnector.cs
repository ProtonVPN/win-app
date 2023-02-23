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
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Vpn.Connectors
{
    public class CountryConnector : BaseConnector
    {
        private readonly IAppSettings _appSettings;
        private readonly IProfileFactory _profileFactory;

        public CountryConnector(
            IAppSettings appSettings,
            IVpnManager vpnManager,
            IProfileFactory profileFactory) :
            base(vpnManager)
        {
            _appSettings = appSettings;
            _profileFactory = profileFactory;
        }

        public async Task Connect(string countryCode)
        {
            Profile profile = CreateProfile(countryCode); 
            await VpnManager.ConnectAsync(profile);
        }

        private Profile CreateProfile(string countryCode)
        {
            Profile profile = _profileFactory.Create();
            profile.IsTemporary = true;
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = _appSettings.SecureCore ? Features.SecureCore : Features.None;
            profile.CountryCode = countryCode;
            return profile;
        }
    }
}
