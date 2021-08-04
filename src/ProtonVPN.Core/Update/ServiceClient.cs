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
using System.ServiceModel;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Service;
using ProtonVPN.UpdateServiceContract;

namespace ProtonVPN.Core.Update
{
    public class ServiceClient
    {
        private readonly ServiceChannelFactory _channelFactory;
        private readonly ILogger _logger;
        private ServiceChannel<IUpdateContract> _channel;
        private readonly UpdateEvents _updateEvents;

        public ServiceClient(ILogger logger, ServiceChannelFactory channelFactory, UpdateEvents updateEvents)
        {
            _channelFactory = channelFactory;
            _logger = logger;
            _updateEvents = updateEvents;
        }

        public event EventHandler<UpdateStateContract> UpdateStateChanged
        {
            add => _updateEvents.UpdateStateChanged += value;
            remove => _updateEvents.UpdateStateChanged -= value;
        }

        public Task CheckForUpdate(bool earlyAccess) => Invoke(p => p.CheckForUpdate(earlyAccess).Wrap());

        public Task StartUpdating(bool auto) => Invoke(p => p.Update(auto).Wrap());

        private async Task<T> Invoke<T>(Func<IUpdateContract, Task<T>> serviceCall,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            int retryCount = 1;
            while (true)
            {
                try
                {
                    ServiceChannel<IUpdateContract> channel = GetChannel();
                    return await serviceCall(channel.Proxy);
                }
                catch (Exception exception) when (IsCommunicationException(exception))
                {
                    CloseChannel();
                    if (retryCount <= 0)
                    {
                        LogError(exception, memberName, isToRetry: false);
                        throw;
                    }
                    LogError(exception, memberName, isToRetry: true);
                }

                retryCount--;
            }
        }

        private ServiceChannel<IUpdateContract> GetChannel()
        {
            return _channel ?? (_channel = NewChannel());
        }

        private ServiceChannel<IUpdateContract> NewChannel()
        {
            ServiceChannel<IUpdateContract> channel = _channelFactory.Create<IUpdateContract>(
                "protonvpn-update-service/update",
                _updateEvents);

            RegisterCallback(channel);

            return channel;
        }

        private void RegisterCallback(ServiceChannel<IUpdateContract> channel)
        {
            try
            {
                channel.Proxy.RegisterCallback();
            }
            catch (Exception)
            {
                channel.Dispose();
                throw;
            }
        }

        private void CloseChannel()
        {
            _channel?.Dispose();
            _channel = null;
        }

        private static bool IsCommunicationException(Exception ex) =>
            ex is CommunicationException ||
            ex is TimeoutException ||
            ex is ObjectDisposedException ode && ode.ObjectName == "System.ServiceModel.Channels.ClientFramingDuplexSessionChannel";

        private void LogError(Exception exception, string callerMemberName, bool isToRetry)
        {
            _logger.Error(
                $"The invocation of '{callerMemberName}' on Update Service channel returned an exception and will " +
                (isToRetry ? string.Empty : "not ") +
                $"be retried. Exception message: {exception.Message}");
        }
    }
}
