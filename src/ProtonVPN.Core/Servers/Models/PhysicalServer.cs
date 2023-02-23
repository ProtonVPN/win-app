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

namespace ProtonVPN.Core.Servers.Models
{
    public class PhysicalServer
    {
        public string Id { get; }
        public string EntryIp { get; }
        public string ExitIp { get; }
        public string Domain { get; }
        public string Label { get; }
        public sbyte Status { get; }
        public string X25519PublicKey { get; }
        public string Signature { get; }

        public PhysicalServer(string id, string entryIp, string exitIp, string domain, string label, sbyte status,
            string x25519PublicKey, string signature)
        {
            Id = id;
            EntryIp = entryIp;
            ExitIp = exitIp;
            Domain = domain;
            Label = label;
            Status = status;
            X25519PublicKey = x25519PublicKey;
            Signature = signature;
        }
    }
}