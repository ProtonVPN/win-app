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
using System.Net.Sockets;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.NetworkLogs;

namespace ProtonVPN.Core.OS
{
    // NTP implementation can be read on RFC 2030: https://datatracker.ietf.org/doc/html/rfc2030
    public class NtpClient : INtpClient
    {
        private const int UDP_PORT = 123;
        private const int SOCKET_TIMEOUT_IN_MILLISECONDS = 3000;

        /// <summary>Bytes 40-43 (32-bit uint) - Seconds since 1900-01-01 00:00:00 UTC</summary>
        private const byte RESPONSE_SECONDS_FIRST_BYTE = 40;

        /// <summary>Bytes 44-47 (32-bit uint) - Fractional seconds</summary>
        private const byte RESPONSE_FRACTIONAL_SECONDS_FIRST_BYTE = 44;

        /// <remarks>
        /// 0x1B = 00 011 011
        /// [Leap Indicator] 0 - No warning of impending leap seconds
        /// [Version Number] 3 - Version 3 (IPv4 only)
        /// [Mode] 3 - Client (In unicast and anycast the client sets this to 3 in the request and the server to 4 (server) in the reply)
        /// </remarks>
        private const byte REQUEST_FIRST_BYTE_CONTENT = 0x1B;

        private readonly string _ntpServer;
        private readonly ILogger _logger;

        public NtpClient(string ntpServer, ILogger logger)
        {
            _ntpServer = ntpServer;
            _logger = logger;
        }

        public DateTime? GetNetworkUtcTime()
        {
            try
            {
                IPEndPoint ipEndPoint = GetNtpServerIPEndPoint();
                byte[] ntpData = CreateNtpRequestData();

                using (Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Connect(ipEndPoint);
                    socket.ReceiveTimeout = SOCKET_TIMEOUT_IN_MILLISECONDS;
                    socket.Send(ntpData);
                    socket.Receive(ntpData);
                    socket.Close();
                }

                return GetUtcTime(ntpData);
            }
            catch (SocketException e)
            {
                _logger.Error<NetworkLog>("Failed to get network time", e);
                return null;
            }
        }

        private IPEndPoint GetNtpServerIPEndPoint()
        {
            IPAddress[] addresses = System.Net.Dns.GetHostEntry(_ntpServer).AddressList;
            return new(addresses[0], UDP_PORT);
        }

        private byte[] CreateNtpRequestData()
        {
            byte[] ntpData = new byte[48];
            ntpData[0] = REQUEST_FIRST_BYTE_CONTENT;
            return ntpData;
        }

        private DateTime GetUtcTime(byte[] ntpData)
        {
            long timestampInMilliseconds = GetTimestampInMilliseconds(ntpData);
            return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestampInMilliseconds);
        }

        private long GetTimestampInMilliseconds(byte[] ntpData)
        {
            ulong intPart = BitConverter.ToUInt32(ntpData, RESPONSE_SECONDS_FIRST_BYTE);
            ulong fractionalPart = BitConverter.ToUInt32(ntpData, RESPONSE_FRACTIONAL_SECONDS_FIRST_BYTE);
            intPart = SwapEndian(intPart);
            fractionalPart = SwapEndian(fractionalPart);
            ulong timestampInMilliseconds = (intPart * 1000) + (fractionalPart * 1000 / 0x100000000L);
            return (long)timestampInMilliseconds;
        }

        private uint SwapEndian(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                          ((x & 0x0000ff00) << 8) +
                          ((x & 0x00ff0000) >> 8) +
                          ((x & 0xff000000) >> 24));
        }
    }
}