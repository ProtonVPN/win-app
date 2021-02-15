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

using System.IO;
using ProtonVPN.Common.OS.Net.NetworkInterface;

namespace ProtonVPN.BugReporting.Diagnostic
{
    internal class NetworkAdapterLog : BaseLog
    {
        private readonly INetworkInterfaces _networkInterfaces;

        public NetworkAdapterLog(INetworkInterfaces networkInterfaces, string path) : base(path, "NetworkAdapters.txt")
        {
            _networkInterfaces = networkInterfaces;
        }

        public override void Write()
        {
            File.WriteAllText(Path, Content);
        }

        private string Content
        {
            get
            {
                var str = string.Empty;
                var interfaces = _networkInterfaces.GetInterfaces();
                foreach (var networkInterface in interfaces)
                {
                    str += GetInterfaceDetails(networkInterface);
                }

                return str;
            }
        }

        private string GetInterfaceDetails(INetworkInterface networkInterface)
        {
            var active = networkInterface.IsActive ? "true" : "false";

            return $"Name: {networkInterface.Name}\n" +
                   $"Description: {networkInterface.Description}\n" +
                   $"Active: {active}\n\n";
        }
    }
}
