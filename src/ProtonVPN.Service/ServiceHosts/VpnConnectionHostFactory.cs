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
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Service;
using ProtonVPN.Common.Service.Validation;
using ProtonVPN.Common.ServiceModel.Server;
using ProtonVPN.Service.Contract.Vpn;

namespace ProtonVPN.Service.ServiceHosts
{
    internal class VpnConnectionHostFactory : ServiceHostFactory
    {
        private readonly ILogger _logger;
        private readonly VpnConnectionHandler _proxy;
        private readonly IConfiguration _config;

        public VpnConnectionHostFactory(ILogger logger, VpnConnectionHandler proxy, IConfiguration config)
        {
            _logger = logger;
            _proxy = proxy;
            _config = config;
        }

        public override SafeServiceHost Create()
        {
            SafeServiceHost serviceHost =
                new SafeServiceHost(_proxy, new Uri("net.pipe://localhost/protonvpn-service"));

            serviceHost.AddServiceEndpoint(
                typeof(IVpnConnectionContract),
                BuildNamedPipe(),
                "connection");

            serviceHost.Description.Behaviors.Add(new ErrorLoggingBehavior(_logger));
            serviceHost.Description.Behaviors.Add(
                new ParameterValidatingBehavior(
                    new ValidatingParameterInspector(new List<IObjectValidator>
                    {
                        new ValidatableObjectValidator(_config)
                    })));

            return serviceHost;
        }
    }
}