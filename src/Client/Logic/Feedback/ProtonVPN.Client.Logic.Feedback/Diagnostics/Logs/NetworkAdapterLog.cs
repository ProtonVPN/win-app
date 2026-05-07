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

using System.Text;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.OperatingSystems.Network.Contracts.NetworkInterfaces;

namespace ProtonVPN.Client.Logic.Feedback.Diagnostics.Logs;

public class NetworkAdapterLog : LogBase
{
    private readonly INetworkInterfacesProvider _networkInterfacesProvider;

    protected string Content
    {
        get
        {
            StringBuilder stringBuilder = new();
            IReadOnlyList<NetworkInterfaceInfo> interfaces = _networkInterfacesProvider.Get();
            foreach (NetworkInterfaceInfo networkInterfaceInfo in interfaces)
            {
                GetInterfaceDetails(stringBuilder, networkInterfaceInfo);
            }
            return stringBuilder.ToString();
        }
    }

    public NetworkAdapterLog(INetworkInterfacesProvider networkInterfacesProvider, IStaticConfiguration config)
        : base(config.DiagnosticLogsFolder, "NetworkAdapters.txt")
    {
        _networkInterfacesProvider = networkInterfacesProvider;
    }

    public override void Write()
    {
        File.WriteAllText(Path, Content);
    }

    private void GetInterfaceDetails(StringBuilder stringBuilder, NetworkInterfaceInfo networkInterfaceInfo)
    {
        stringBuilder
            .AppendLine($"Name: {networkInterfaceInfo.Name}")
            .AppendLine($"Description: {networkInterfaceInfo.Description}")
            .AppendLine($"Operational status: {networkInterfaceInfo.OperationalStatus}")
            .AppendLine($"Guid: {networkInterfaceInfo.Guid}")
            .AppendLine($"Type: {(int)networkInterfaceInfo.Type} ({networkInterfaceInfo.Type})");

        if (networkInterfaceInfo.Driver is null)
        {
            stringBuilder.AppendLine("Driver: n/a");
        }
        else
        {
            stringBuilder
                .AppendLine("Driver:")
                .AppendLine($"    FileName: {networkInterfaceInfo.Driver.FileName}")
                .AppendLine($"    Provider: {networkInterfaceInfo.Driver.Provider}")
                .AppendLine($"    Description: {networkInterfaceInfo.Driver.Description}")
                .AppendLine($"    ComponentId: {networkInterfaceInfo.Driver.ComponentId}");
        }

        stringBuilder.AppendLine();
    }
}