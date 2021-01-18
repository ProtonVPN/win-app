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
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;

namespace ProtonVPN.Core.Profiles
{
    public class Profile
    {
        private static readonly Regex _hexColorRegex = new Regex("^#(?:[0-9a-fA-F]{3}){1,2}$");
        private static readonly ColorProvider _colorProvider = new ColorProvider();

        public Profile() : this(null)
        { }

        public Profile(string id)
        {
            Id = !string.IsNullOrEmpty(id) ? id : Guid.NewGuid().ToString();
            _colorCode = GetRandomColor();
        }

        public string Id { get; set; }

        public string ExternalId { get; set; }

        [JsonIgnore]
        public bool IsPredefined { get; set; }

        [JsonIgnore]
        public bool IsTemporary { get; set; }

        public ProfileType ProfileType { get; set; }

        public Features Features { get; set; }

        private string _colorCode;
        public string ColorCode
        {
            get => _colorCode;
            set => _colorCode = GetValidColorCode(value);
        }

        public string Name { get; set; }

        /// <summary>
        /// For predefined and temporary profiles the <see cref="Protocol"/> value is set to
        /// Default Protocol from Settings right before connecting/reconnecting.
        /// </summary>
        public Protocol Protocol { get; set; } = Protocol.Auto;

        public string CountryCode { get; set; }

        public string ServerId { get; set; }

        public ProfileStatus Status { get; set; }

        public ProfileSyncStatus SyncStatus { get; set; }

        public DateTime ModifiedAt { get; set; }

        public string OriginalName { get; set; }

        public int UniqueNameIndex { get; set; }

        [JsonIgnore]
        public Server Server;

        public Profile Clone()
        {
            var clone = (Profile)MemberwiseClone();
            clone.Server = null;
            return clone;
        }

        private string GetValidColorCode(string colorCode)
        {
            return IsColorCodeValid(colorCode) ? colorCode : GetRandomColor();
        }

        private bool IsColorCodeValid(string colorCode)
        {
            return colorCode != null && _hexColorRegex.IsMatch(colorCode);
        }

        private string GetRandomColor()
        {
            return _colorProvider.RandomColor();
        }

        public bool IsColorCodeValid()
        {
            return IsColorCodeValid(ColorCode);
        }
    }
}
