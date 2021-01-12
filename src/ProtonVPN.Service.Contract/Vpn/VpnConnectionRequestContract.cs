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

using System.Runtime.Serialization;
using ProtonVPN.Service.Contract.Settings;

namespace ProtonVPN.Service.Contract.Vpn
{
    [DataContract]
    public class VpnConnectionRequestContract
    {
        [DataMember(IsRequired = true)]
        public VpnHostContract[] Servers { get; set; }

        [DataMember(IsRequired = true)]
        public VpnProtocolContract Protocol { get; set; }

        [DataMember(IsRequired = true)]
        public VpnConfigContract VpnConfig { get; set; }

        [DataMember(IsRequired = true)]
        public VpnCredentialsContract Credentials { get; set; }

        [DataMember(IsRequired = true)]
        public SettingsContract Settings { get; set; }
    }
}