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
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Legacy;
using ProtonVPN.Common.Legacy.PortForwarding;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ConnectionLogs;
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
        private const byte TCP_OPERATION = (byte)TransportProtocol.TCP;
        private const byte UDP_OPERATION = (byte)TransportProtocol.UDP;

        private readonly ILogger _logger;
        private readonly IUdpClientWrapper _udpClientWrapper;
        private readonly IMessageSerializerProxy _messageSerializerProxy;
        private readonly IGatewayCache _gatewayCache;
        private readonly IIssueReporter _issueReporter;

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
            IGatewayCache gatewayCache,
            IIssueReporter issueReporter)
        {
            _logger = logger;
            _udpClientWrapper = udpClientWrapper;
            _messageSerializerProxy = messageSerializerProxy;
            _gatewayCache = gatewayCache;
            _issueReporter = issueReporter;
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
                await SendPortMappingMessagesAsync(cancellationToken);
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
            state ??= PortForwardingState.Default;
            StateChanged?.Invoke(this, new(state));
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
            IPAddress gatewayIPAddress = _gatewayCache.Get() ?? throw new Exception("The default gateway is missing and NAT-PMP can't start without it.");
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
            task.FireAndForget();
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

        private async Task SendPortMappingMessagesAsync(CancellationToken cancellationToken, PortMappingQueryMessages queryMessages = null)
        {
            ChangeState(PortMappingStatus.PortMappingCommunication);

            queryMessages ??= new();
            queryMessages.TcpQuery ??= CreateTcpPortMappingQueryMessage();
            queryMessages.UdpQuery ??= CreateUdpPortMappingQueryMessage();

            PortMappingReplyMessage tcpReply = await SendPortMappingMessageAsync(queryMessages.TcpQuery, cancellationToken);
            PortMappingReplyMessage udpReply = await SendPortMappingMessageAsync(queryMessages.UdpQuery, cancellationToken);

            HandlePortMappingResponses(tcpReply: tcpReply, udpReply: udpReply, cancellationToken);
        }

        private PortMappingQueryMessage CreateTcpPortMappingQueryMessage()
        {
            return CreatePortMappingQueryMessage(TCP_OPERATION);
        }

        private PortMappingQueryMessage CreatePortMappingQueryMessage(byte operation)
        {
            return new()
            {
                Operation = operation,
                RequestedLeaseTimeSecond = REQUESTED_LEASE_TIME_SECONDS
            };
        }

        private PortMappingQueryMessage CreateUdpPortMappingQueryMessage()
        {
            return CreatePortMappingQueryMessage(UDP_OPERATION);
        }

        private async Task<PortMappingReplyMessage> SendPortMappingMessageAsync(PortMappingQueryMessage queryMessage, CancellationToken cancellationToken)
        {
            try
            {
                byte[] serializedQuery = _messageSerializerProxy.ToBytes(queryMessage);
                byte[] serializedReply = await SendMessageWithTimeoutAsync(serializedQuery, cancellationToken);
                return _messageSerializerProxy.FromBytes<PortMappingReplyMessage>(serializedReply);
            }
            catch (Exception ex)
            {
                _logger.Error<ConnectionLog>("An exception occurred when sending a NAT-PMP request or receiving the response.", ex);
                return null;
            }
        }

        private void HandlePortMappingResponses(PortMappingReplyMessage tcpReply, PortMappingReplyMessage udpReply, CancellationToken cancellationToken)
        {
            if (HasRequestFailed(tcpReply) && HasRequestFailed(udpReply))
            {
                HandlePortMappingUnsuccessfulResponses(tcpReply: tcpReply, udpReply: udpReply);
                return;
            }

            if (HasRequestFailed(tcpReply))
            {
                _logger.Error<ConnectionLog>($"Port mapping TCP response was not successful. " +
                    $"[ResultCode: {tcpReply?.ResultCode}, Operation: {tcpReply?.Operation}]");
                tcpReply = udpReply;
            }
            if (HasRequestFailed(udpReply))
            {
                _logger.Error<ConnectionLog>($"Port mapping UDP response was not successful. " +
                    $"[ResultCode: {udpReply?.ResultCode}, Operation: {udpReply?.Operation}]");
                udpReply = tcpReply;
            }

            TemporaryMappedPort mappedTcpPort = CreateTemporaryMappedPort(tcpReply);
            TemporaryMappedPort mappedUdpPort = CreateTemporaryMappedPort(udpReply);

            if (mappedTcpPort.MappedPort != mappedUdpPort.MappedPort)
            {
                _logger.Error<ConnectionLog>($"The NAT-PMP ports of the TCP and UDP replies do not match. " +
                    $"The logic will proceed by using the TCP ports. [TCP: {mappedTcpPort.MappedPort}, UDP: {mappedUdpPort.MappedPort}]");
                _issueReporter.CaptureMessage("NAT-PMP TCP and UDP ports don't match.",
                    $"[NAT-PMP] TCP: {mappedTcpPort.MappedPort}, UDP: {mappedUdpPort.MappedPort}");
            }

            int portDurationInSeconds = (int)Math.Truncate(tcpReply.LifetimeSeconds / 2.0);

            // Both TCP and UDP mapped ports should be the same (Although they might not if the server is not correctly configured),
            // TCP is saved because it is the most relevant protocol between the two
            SavePortMappingAndScheduleRenewal(mappedTcpPort, portDurationInSeconds, cancellationToken);
        }

        private bool HasRequestFailed(PortMappingReplyMessage reply)
        {
            return reply is null || !reply.IsSuccess();
        }

        private void HandlePortMappingUnsuccessfulResponses(PortMappingReplyMessage tcpReply, PortMappingReplyMessage udpReply)
        {
            _logger.Error<ConnectionLog>($"Port mapping responses were not successful. " +
                $"[TCP ResultCode: {tcpReply?.ResultCode}, Operation: {tcpReply?.Operation}]" +
                $"[UDP ResultCode: {udpReply?.ResultCode}, Operation: {udpReply?.Operation}]");
            SetMappedPort(null);
            ChangeState(PortMappingStatus.Error);
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

        private void SavePortMappingAndScheduleRenewal(TemporaryMappedPort mappedPort, int portDurationInSeconds, CancellationToken cancellationToken)
        {
            SetMappedPort(mappedPort);

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

        private void SetMappedPort(TemporaryMappedPort mappedPort)
        {
            _mappedPort = mappedPort;
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
                    PortMappingQueryMessage tcpQuery = CreateTcpPortMappingQueryMessage();
                    tcpQuery.InternalPort = (ushort)mappedPort.InternalPort;
                    tcpQuery.ExternalPort = (ushort)mappedPort.ExternalPort;

                    PortMappingQueryMessage udpQuery = CreateUdpPortMappingQueryMessage();
                    udpQuery.InternalPort = (ushort)mappedPort.InternalPort;
                    udpQuery.ExternalPort = (ushort)mappedPort.ExternalPort;

                    PortMappingQueryMessages queryMessages = new() { TcpQuery = tcpQuery, UdpQuery = udpQuery };

                    _logger.Info<ConnectionLog>($"Port mapping renewal started for pair {mappedPort}.");
                    await SendPortMappingMessagesAsync(cancellationToken, queryMessages: queryMessages);
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
                    await SendDestroyPortMappingMessagesAsync(mappedPort, stopCancellationToken);
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

        private async Task SendDestroyPortMappingMessagesAsync(MappedPort mappedPort, CancellationToken cancellationToken)
        {
            ChangeState(PortMappingStatus.DestroyPortMappingCommunication);

            _logger.Info<ConnectionLog>($"Requesting to destroy mapped TCP port pair {mappedPort}.");
            await SendDestroyPortMappingMessageAsync(CreateDestroyTcpPortMappingQueryMessage(mappedPort), mappedPort, cancellationToken);

            _logger.Info<ConnectionLog>($"Requesting to destroy mapped UDP port pair {mappedPort}.");
            await SendDestroyPortMappingMessageAsync(CreateDestroyUdpPortMappingQueryMessage(mappedPort), mappedPort, cancellationToken);
        }

        private PortMappingQueryMessage CreateDestroyTcpPortMappingQueryMessage(MappedPort mappedPort)
        {
            return CreateDestroyPortMappingQueryMessage(mappedPort, TCP_OPERATION);
        }

        private PortMappingQueryMessage CreateDestroyPortMappingQueryMessage(MappedPort mappedPort, byte operation)
        {
            return new()
            {
                Operation = operation,
                RequestedLeaseTimeSecond = 0,
                InternalPort = (ushort)mappedPort.InternalPort,
                ExternalPort = 0,
            };
        }

        private PortMappingQueryMessage CreateDestroyUdpPortMappingQueryMessage(MappedPort mappedPort)
        {
            return CreateDestroyPortMappingQueryMessage(mappedPort, UDP_OPERATION);
        }

        private async Task SendDestroyPortMappingMessageAsync(PortMappingQueryMessage query, MappedPort mappedPort, CancellationToken cancellationToken)
        {
            byte[] serializedQuery = _messageSerializerProxy.ToBytes(query);
            byte[] serializedReply = await SendMessageWithSingleTryAsync(serializedQuery, cancellationToken);
            PortMappingReplyMessage reply = _messageSerializerProxy.FromBytes<PortMappingReplyMessage>(serializedReply);

            if (!HasRequestFailed(reply) && reply.InternalPort == mappedPort.InternalPort &&
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

        public void RepeatState()
        {
            InvokeState(_lastState);
        }
    }
}
