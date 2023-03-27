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

using System.Runtime.Serialization;

namespace ProtonVPN.ProcessCommunication.Contracts.Entities.Update
{
    [DataContract]
    public class UpdateStateIpcEntity
    {
        [DataMember(Order = 1)]
        public ReleaseIpcEntity[] ReleaseHistory { get; set; }
        
        [DataMember(Order = 2)]
        public bool IsAvailable { get; set; }

        [DataMember(Order = 3)]
        public bool IsReady { get; set; }

        [DataMember(Order = 4)]
        public UpdateStatusIpcEntity Status { get; set; }

        [DataMember(Order = 5)]
        public string FilePath { get; set; }

        [DataMember(Order = 6)]
        public string FileArguments { get; set; }

        [DataMember(Order = 7)]
        public string Version { get; set; }

        public UpdateStateIpcEntity()
        {
            ReleaseHistory = Array.Empty<ReleaseIpcEntity>();
        }
    }
}