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
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ConnectionLogs;
using ProtonVPN.Common.PortForwarding;
using ProtonVPN.Vpn.Gateways;
using ProtonVPN.Vpn.PortMapping.Messages;
using ProtonVPN.Vpn.PortMapping.Serializers.Common;
using ProtonVPN.Vpn.PortMapping.UdpClients;

namespace ProtonVPN.Vpn.PortMapping
{
    // Network Address Translation Port Mapping Protocol (NAT-PMP) - RFC 6886: https://datatracker.ietf.org/doc/html/rfc6886
    public class PortMappingProtocolClient : IPortMappingProtocolClient
    {
        private const ushort NAT_PMP_PORT = 5351;
        private const ushort MIN_TIMEOUT_MILLISECONDS = 250;
        private const ushort MAX_TIMEOUT_MILLISECONDS = 64000;
        private const uint REQUESTED_LEASE_TIME_SECONDS = 7200;
        private const byte OPERATION = (byte)TransportProtocol.TCP;

        private readonly ILogger _logger;
        private readonly IUdpClientWrapper _udpClientWrapper;
        private readonly IMessageSerializerProxy _messageSerializerProxy;
        private readonly IGatewayCache _gatewayCache;

        private IPEndPoint _endpoint;
        private HelloReplyMessage _helloReply;
        private TemporaryMappedPort _mappedPort;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _stopCancellationTokenSource;
        private PortForwardingState _lastState;

        public event EventHandler<EventArgs<PortForwardingState>> StateChanged;

        public PortMappingProtocolClient(ILogger logger,
            IUdpClientWrapper udpClientWrapper,
            IMessageSerializerProxy messageSerializerProxy,
            IGatewayCache gatewayCache)
        {
            _logger = logger;
            _udpClientWrapper = udpClientWrapper;
            _messageSerializerProxy = messageSerializerProxy;
            _gatewayCache = gatewayCache;
        }

        public async Task StartAsync()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _logger.Warn<ConnectionLog>("Can't start port mapping because it is already running.");
                return;
            }

            ChangeState(PortMappingStatus.Starting);
            _stopCancellationTokenSource?.Cancel();
            CancellationToken cancellationToken = GenerateNewCancellationToken();
            try
            {
                InitializeUdpClient();
                await SendHelloMessageAsync(cancellationToken);
                await SendPortMappingMessageAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Error<ConnectionLog>("NAT-PMP start failed.", e);
                ChangeState(PortMappingStatus.Error);
            }
        }

        private CancellationToken GenerateNewCancellationToken()
        {
            CancellationTokenSource cancellationTokenSource = new();
            _cancellationTokenSource = cancellationTokenSource;
            return cancellationTokenSource.Token;
        }

        private void ChangeState(PortMappingStatus status)
        {
            PortForwardingState state = CreatePortForwardingState(status);
            _lastState = state;
            InvokeState(state);
            StringBuilder logMessage = new StringBuilder().Append($"State changed to Status '{state.Status}' at '{state.TimestampUtc}'");
            if (state.MappedPort?.MappedPort is not null)
            {
                TemporaryMappedPort mappedPort = state.MappedPort;
                logMessage.Append($", Port pair {mappedPort.MappedPort}, expiring after " +
                                  $"{mappedPort.Lifetime} around {mappedPort.ExpirationDateUtc}");
            }
            _logger.Info<ConnectionLog>(logMessage.ToString());
        }

        private void InvokeState(PortForwardingState state)
        {
            if (state == null)
            {
                _logger.Warn<ConnectionLog>("Can't invoke null state.");
            }
            else
            {
                StateChanged?.Invoke(this, new(state));
            }
        }

        private PortForwardingState CreatePortForwardingState(PortMappingStatus status)
        {
            return new()
            {
                MappedPort = _mappedPort,
                Status = status
            };
        }

        private void InitializeUdpClient()
        {
            IPAddress gatewayIPAddress = _gatewayCache.Get();
            if (gatewayIPAddress == null)
            {
                throw new Exception("The default gateway is missing and NAT-PMP can't start without it.");
            }
            _endpoint = new IPEndPoint(gatewayIPAddress, NAT_PMP_PORT);
            _udpClientWrapper.Start(_endpoint);
            _logger.Info<ConnectionLog>($"Starting NAT-PMP communication with gateway {_endpoint}.");
        }

        private async Task SendHelloMessageAsync(CancellationToken cancellationToken)
        {
            ChangeState(PortMappingStatus.HelloCommunication);
            HelloQueryMessage query = new();
            byte[] serializedQuery = _messageSerializerProxy.ToBytes(query);
            byte[] serializedReply = await SendMessageWithTimeoutAsync(serializedQuery, cancellationToken);
            _helloReply = _messageSerializerProxy.FromBytes<HelloReplyMessage>(serializedReply);
        }

        private async Task<byte[]> SendMessageWithTimeoutAsync(byte[] serializedMessage, CancellationToken cancellationToken)
        {
            byte[] serializedReply = null;
            Exception exception = new("The serialized reply received is empty.");
            for (int timeoutInMilliseconds = MIN_TIMEOUT_MILLISECONDS; timeoutInMilliseconds <= MAX_TIMEOUT_MILLISECONDS; timeoutInMilliseconds *= 2)
            {
                try
                {
                    _udpClientWrapper.Send(serializedMessage);
                    serializedReply = await GetReplyOrTimeoutAsync(timeoutInMilliseconds, cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.Error<ConnectionLog>("An error occurred when sending a message " +
                        $"or receiving a response ({timeoutInMilliseconds}ms).", e);
                    exception = e;
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        _udpClientWrapper.Reset();
                        continue;
                    }
                }
                break;
            }
            if (serializedReply is null)
            {
                _logger.Error<ConnectionLog>("All retries were used for the current communication. Last exception in annex.", exception);
                throw exception;
            }

            return serializedReply;
        }

        private async Task<byte[]> GetReplyOrTimeoutAsync(int timeoutInMilliseconds, CancellationToken cancellationToken)
        {
            Task<byte[]> task = Task.Run(GetReply, cancellationToken);
            if (await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds, cancellationToken)) == task)
            {
                ThrowIfReplyAwaitWasCancelled(cancellationToken);
                return await task;
            }
            task.IgnoreExceptions();
            ThrowIfReplyAwaitWasCancelled(cancellationToken);
            throw new TimeoutException($"The remote endpoint '{_endpoint}' did not reply to the query in time ({timeoutInMilliseconds}ms).");
        }

        private void ThrowIfReplyAwaitWasCancelled(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("The wait for the reply was cancelled.");
            }
        }

        private byte[] GetReply()
        {
            return _udpClientWrapper.Receive();
        }

        private async Task SendPortMappingMessageAsync(CancellationToken cancellationToken, PortMappingQueryMessage query = null)
        {
            ChangeState(PortMappingStatus.PortMappingCommunication);
            query ??= CreatePortMappingQueryMessage();
            byte[] serializedQuery = _messageSerializerProxy.ToBytes(query);
            byte[] serializedReply = await SendMessageWithTimeoutAsync(serializedQuery, cancellationToken);
            PortMappingReplyMessage reply = _messageSerializerProxy.FromBytes<PortMappingReplyMessage>(serializedReply);
            if (reply.IsSuccess())
            {
                SavePortMappingAndScheduleRenewal(reply, cancellationToken);
            }
            else
            {
                HandlePortMappingUnsuccessfulResponse(reply);
            }
        }

        private void HandlePortMappingUnsuccessfulResponse(PortMappingReplyMessage reply)
        {
            _logger.Error<ConnectionLog>("Port mapping response was not successful. " +
                $"ResultCode: {reply.ResultCode}, Operation: {reply.Operation}.");
            SetMappedPort(null);
            ChangeState(PortMappingStatus.Error);
        }

        private void SetMappedPort(TemporaryMappedPort mappedPort)
        {
            _mappedPort = mappedPort;
        }

        private PortMappingQueryMessage CreatePortMappingQueryMessage()
        {
            return new()
            {
                Operation = OPERATION,
                RequestedLeaseTimeSecond = REQUESTED_LEASE_TIME_SECONDS
            };
        }

        private void SavePortMappingAndScheduleRenewal(PortMappingReplyMessage reply, CancellationToken cancellationToken)
        {
            TemporaryMappedPort mappedPort = CreateTemporaryMappedPort(reply);
            SetMappedPort(mappedPort);
            int portDurationInSeconds = (int)Math.Truncate(reply.LifetimeSeconds / 2.0);

            try
            {
                Task.Delay(TimeSpan.FromSeconds(portDurationInSeconds), cancellationToken)
                    .ContinueWith(async t => await RenewPortMappingAsync(mappedPort.MappedPort, cancellationToken));
            }
            catch (Exception e)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info<ConnectionLog>("The scheduled renewal of port mapping was cancelled with an exception.", e);
                    return;
                }

                _logger.Error<ConnectionLog>("An error occurred on a NAT-PMP scheduled renewal.", e);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                ChangeState(PortMappingStatus.SleepingUntilRefresh);
            }
        }

        private TemporaryMappedPort CreateTemporaryMappedPort(PortMappingReplyMessage reply)
        {
            return new()
            {
                MappedPort = new(internalPort: reply.InternalPort, externalPort: reply.ExternalPort),
                Lifetime = TimeSpan.FromSeconds(reply.LifetimeSeconds),
                ExpirationDateUtc = DateTime.UtcNow.AddSeconds(reply.LifetimeSeconds)
            };
        }

        private async Task RenewPortMappingAsync(MappedPort mappedPort, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.Info<ConnectionLog>("Port mapping renewal ignored due to cancelled process.");
            }
            else
            {
                try
                {
                    PortMappingQueryMessage query = CreatePortMappingQueryMessage();
                    query.InternalPort = (ushort)mappedPort.InternalPort;
                    query.ExternalPort = (ushort)mappedPort.ExternalPort;
                    _logger.Info<ConnectionLog>($"Port mapping renewal started for pair {mappedPort}.");
                    await SendPortMappingMessageAsync(cancellationToken, query);
                }
                catch (Exception e)
                {
                    _logger.Error<ConnectionLog>("NAT-PMP renewal failed.", e);
                    ChangeState(PortMappingStatus.Error);
                }
            }
        }

        public async Task StopAsync()
        {
            if (IsStopPossible())
            {
                await ExecuteStopAsync();
            }
        }

        private bool IsStopPossible()
        {
            if (_lastState == null ||
                _lastState.Status == PortMappingStatus.Stopped)
            {
                _logger.Debug<ConnectionLog>("Can't stop port mapping because it is already stopped " +
                                             $"(LastState: {_lastState?.Status}).");
                return false;
            }
            if (_stopCancellationTokenSource != null && !_stopCancellationTokenSource.IsCancellationRequested)
            {
                _logger.Warn<ConnectionLog>("Can't stop port mapping because it is already stopping.");
                return false;
            }

            return true;
        }

        private async Task ExecuteStopAsync()
        {
            _logger.Info<ConnectionLog>("Stopping NAT-PMP.");
            _cancellationTokenSource?.Cancel();
            await DestroyMappedPortAsync();

            try
            {
                _udpClientWrapper.Stop();
                ChangeStateToStopped();
                _stopCancellationTokenSource?.Cancel();
            }
            catch (Exception e)
            {
                _logger.Error<ConnectionLog>("Error when stopping the UdpClient and finishing the NAT-PMP stop.", e);
            }
        }

        private void ChangeStateToStopped()
        {
            SetMappedPort(null);
            ChangeState(PortMappingStatus.Stopped);
        }

        private async Task DestroyMappedPortAsync()
        {
            MappedPort mappedPort = _mappedPort?.MappedPort;
            CancellationToken stopCancellationToken = GenerateNewStopCancellationToken();
            try
            {
                if (mappedPort != null)
                {
                    await SendDestroyPortMappingMessageAsync(mappedPort, stopCancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.Error<ConnectionLog>("Error when destroying port mapping.", e);
            }
        }

        private CancellationToken GenerateNewStopCancellationToken()
        {
            CancellationTokenSource cancellationTokenSource = new();
            _stopCancellationTokenSource = cancellationTokenSource;
            return cancellationTokenSource.Token;
        }

        private async Task SendDestroyPortMappingMessageAsync(MappedPort mappedPort, CancellationToken cancellationToken)
        {
            ChangeState(PortMappingStatus.DestroyPortMappingCommunication);
            _logger.Info<ConnectionLog>($"Requesting to destroy mapped port pair {mappedPort}.");
            PortMappingQueryMessage query = CreateDestroyPortMappingQueryMessage(mappedPort);
            byte[] serializedQuery = _messageSerializerProxy.ToBytes(query);
            byte[] serializedReply = await SendMessageWithSingleTryAsync(serializedQuery, cancellationToken);
            PortMappingReplyMessage reply = _messageSerializerProxy.FromBytes<PortMappingReplyMessage>(serializedReply);
            if (reply.IsSuccess() && reply.InternalPort == mappedPort.InternalPort && 
                reply.ExternalPort == 0 && reply.LifetimeSeconds == 0)
            {
                _logger.Info<ConnectionLog>($"Successful port mapping destruction. Operation: {reply.Operation}.");
            }
            else
            {
                _logger.Error<ConnectionLog>($"Unsuccessful port mapping destruction. ResultCode: {reply.ResultCode}, " +
                    $"Operation: {reply.Operation}, InternalPort: {reply.InternalPort}, " +
                    $"ExternalPort: {reply.ExternalPort}, LifetimeSeconds: {reply.LifetimeSeconds}, .");
            }
        }

        private async Task<byte[]> SendMessageWithSingleTryAsync(byte[] serializedMessage, CancellationToken cancellationToken)
        {
            byte[] serializedReply = null;
            Exception exception = new("The serialized reply received is empty.");
            try
            {
                _udpClientWrapper.Send(serializedMessage);
                serializedReply = await GetReplyOrTimeoutAsync(MIN_TIMEOUT_MILLISECONDS, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Error<ConnectionLog>("An error occurred when sending a message " +
                    $"or receiving a response ({MIN_TIMEOUT_MILLISECONDS}ms).", e);
                exception = e;
            }
            if (serializedReply is null)
            {
                HandleSendMessageWithSingleTryFailed(serializedMessage, exception);
            }

            return serializedReply;
        }

        private void HandleSendMessageWithSingleTryFailed(byte[] serializedMessage, Exception exception)
        {
            try
            {
                _udpClientWrapper.Send(serializedMessage);
            }
            catch (Exception e)
            {
                throw new Exception("An exception occurred when retrying to send the message.", e);
            }

            throw new Exception("The single try message failed to get a reply. A new message was sent.", exception);
        }

        private PortMappingQueryMessage CreateDestroyPortMappingQueryMessage(MappedPort mappedPort)
        {
            return new()
            {
                Operation = OPERATION,
                RequestedLeaseTimeSecond = 0,
                InternalPort = (ushort)mappedPort.InternalPort,
                ExternalPort = 0,
            };
        }

        public void RepeatState()
        {
            InvokeState(_lastState);
        }
    }
}
