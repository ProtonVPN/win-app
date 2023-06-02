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

using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.ProcessCommunication.Common;
using ProtonVPN.ProcessCommunication.Common.Channels;
using ProtonVPN.ProcessCommunication.Common.Registration;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Settings;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.ProcessCommunication.Contracts.Registration;

namespace ProtonVPN.UI.Tests.TestsHelper
{
    public class VPNServiceHelper : GrpcClientBase
    {
        private readonly IServiceServerPortRegister _serviceServerPortRegister;
        private static readonly ILogger _logger = Substitute.For<ILogger>();
        private static readonly GrpcChannelWrapperFactory _grpcChannelWrapperFactory = new(_logger);

        public IVpnController VpnController { get; private set; }

        public VPNServiceHelper()
            : base(_logger, _grpcChannelWrapperFactory)
        {
            _serviceServerPortRegister = new ServiceServerPortRegister(_logger);
        }

        protected override void RegisterServices(IGrpcChannelWrapper channel)
        {
            VpnController = channel.CreateService<IVpnController>();
        }

        public async Task Disconnect()
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                int serviceServerPort = await _serviceServerPortRegister.ReadAsync(cts.Token);
                await CreateWithPortAsync(serviceServerPort);
                if (VpnController is not null)
                {
                    DisconnectionRequestIpcEntity disconnectionRequestIpcEntity = CreateDisconnectionRequestIpcEntity();
                    await VpnController.Disconnect(disconnectionRequestIpcEntity);
                }
            }
            catch
            {
                //Ignore, because user might be disconnect by UI before
            }
        }

        private DisconnectionRequestIpcEntity CreateDisconnectionRequestIpcEntity()
        {
            return new DisconnectionRequestIpcEntity()
            {
                Settings = new MainSettingsIpcEntity()
                {
                    KillSwitchMode = KillSwitchModeIpcEntity.Off,
                    SplitTunnel = new SplitTunnelSettingsIpcEntity
                    {
                        Mode = SplitTunnelModeIpcEntity.Disabled,
                        AppPaths = new string[] { },
                        Ips = new string[] { },
                    },
                    NetShieldMode = 0,
                    SplitTcp = true,
                    Ipv6LeakProtection = true,
                    VpnProtocol = VpnProtocolIpcEntity.Smart,
                    OpenVpnAdapter = OpenVpnAdapterIpcEntity.Tun,
                },
                ErrorType = VpnErrorTypeIpcEntity.None
            };
        }
    }
}