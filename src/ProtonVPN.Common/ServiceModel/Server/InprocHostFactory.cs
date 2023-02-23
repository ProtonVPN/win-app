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

using ProtonVPN.Common.Helpers;
using System;
using System.ServiceModel;

namespace ProtonVPN.Common.ServiceModel.Server
{
    public class InprocHostFactory
    {
        /// <summary>
        /// Creates service host listening to net.pipe://localhost/serviceEndpoint/contractAddress>
        /// </summary>
        /// <typeparam name="T">Interface of service</typeparam>
        /// <param name="serviceInstance">Instance of service object</param>
        /// <param name="serviceEndpoint">endpoint name of the hosting process</param>
        /// <param name="contractAddress">contract address</param>
        public SafeServiceHost Create<T>(T serviceInstance, string serviceEndpoint, string contractAddress) where T : class
        {
            Ensure.NotNull(serviceInstance, nameof(serviceInstance));
            Ensure.NotEmpty(serviceEndpoint, nameof(serviceEndpoint));
            Ensure.NotEmpty(contractAddress, nameof(contractAddress));

            var serviceHost = new SafeServiceHost(
                serviceInstance,
                new Uri($"net.pipe://localhost/{serviceEndpoint}"));

            serviceHost.AddServiceEndpoint(
                typeof(T),
                BuildNamedPipe(),
                contractAddress);

            return serviceHost;
        }

        /// <summary>
        /// Creates service host listening to net.pipe://localhost/endpoint>
        /// </summary>
        /// <typeparam name="T">Class of service</typeparam>
        /// <param name="endpoint">contract address</param>
        public SafeServiceHost Create<T>(string endpoint) where T : class
        {
            Ensure.NotEmpty(endpoint, nameof(endpoint));
            return new SafeServiceHost(typeof(T), new Uri($"net.pipe://localhost/{endpoint}"));
        }

        /// <summary>
        /// Builds named pipe. By default creates default pipe binding
        /// with ReceiveTimoue = TimeSpan.MaxValue
        /// </summary>
        /// <returns></returns>
        protected virtual NetNamedPipeBinding BuildNamedPipe() => new NetNamedPipeBinding
        {
            ReceiveTimeout = TimeSpan.MaxValue
        };
    }
}
