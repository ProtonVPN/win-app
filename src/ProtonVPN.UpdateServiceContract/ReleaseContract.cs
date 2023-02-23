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
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProtonVPN.UpdateServiceContract
{
    [DataContract]
    public class ReleaseContract
    {
        public ReleaseContract(
            Version version,
            bool earlyAccess,
            bool isNew,
            ICollection<string> changeLog)
        {
            Version = version;
            EarlyAccess = earlyAccess;
            New = isNew;
            ChangeLog = changeLog;
        }

        [DataMember]
        public Version Version { get; private set; }

        [DataMember]
        public bool EarlyAccess { get; private set; }

        [DataMember]
        public bool New { get; private set; }

        [DataMember]
        public ICollection<string> ChangeLog { get; private set; }
    }
}
