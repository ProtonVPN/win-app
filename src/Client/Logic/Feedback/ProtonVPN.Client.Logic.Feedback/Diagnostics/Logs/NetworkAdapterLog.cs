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

using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;

public class NetworkAdapterLog : LogBase
{
    private readonly ISystemNetworkInterfaces _networkInterfaces;

    protected string Content
    {
        get
        {
            string str = string.Empty;
            INetworkInterface[] interfaces = _networkInterfaces.GetInterfaces();
            foreach (INetworkInterface networkInterface in interfaces)
            {
                str += GetInterfaceDetails(networkInterface);
            }

            return str;
        }
    }

    public NetworkAdapterLog(ISystemNetworkInterfaces networkInterfaces, IStaticConfiguration config)
        : base(config.DiagnosticLogsFolder, "NetworkAdapters.txt")
    {
        _networkInterfaces = networkInterfaces;
    }

    public override void Write()
    {
        File.WriteAllText(Path, Content);
    }

    private string GetInterfaceDetails(INetworkInterface networkInterface)
    {
        return $"Name: {networkInterface.Name}\n" +
               $"Description: {networkInterface.Description}\n" +
               $"Active: {networkInterface.IsActive.ToYesNoString()}\n\n";
    }
}