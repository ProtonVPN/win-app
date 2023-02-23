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
using Newtonsoft.Json;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;

namespace ProtonVPN.Core.Profiles
{
    public class Profile
    {
        public string Id { get; set; }

        public string ExternalId { get; set; }

        [JsonIgnore]
        public bool IsPredefined { get; set; }

        [JsonIgnore]
        public bool IsTemporary { get; set; }

        public ProfileType ProfileType { get; set; }

        public Features Features { get; set; }

        public string ColorCode { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// For predefined and temporary profiles the <see cref="Common.Networking.VpnProtocol"/> value is set to
        /// Default VpnProtocol from Settings right before connecting/reconnecting.
        /// </summary>
        public VpnProtocol VpnProtocol { get; set; } = VpnProtocol.Smart;

        public string CountryCode { get; set; }

        public string EntryCountryCode { get; set; }

        public string City { get; set; }

        public sbyte? ExactTier { get; set; }

        public string ServerId { get; set; }

        public ProfileStatus Status { get; set; }

        public ProfileSyncStatus SyncStatus { get; set; }

        public DateTime ModifiedAt { get; set; }

        public string OriginalName { get; set; }

        public int UniqueNameIndex { get; set; }

        [JsonIgnore]
        public Server Server;

        public Profile(string id)
        {
            Id = !string.IsNullOrEmpty(id) ? id : Guid.NewGuid().ToString();
        }

        public Profile Clone()
        {
            var clone = (Profile)MemberwiseClone();
            clone.Server = null;
            return clone;
        }
    }
}
