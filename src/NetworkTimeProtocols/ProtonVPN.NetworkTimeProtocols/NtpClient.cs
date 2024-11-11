/*
 * Copyright (c) 2024 Proton AG
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

using System.Net;
using System.Net.Sockets;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.NetworkLogs;
using ProtonVPN.NetworkTimeProtocols.Contracts;

namespace ProtonVPN.NetworkTimeProtocols;

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

    private readonly ILogger _logger;
    private readonly IDnsManager _dnsManager;
    private readonly string _serverUrlIdnHost;

    public NtpClient(ILogger logger, IDnsManager dnsManager, IConfiguration config)
    {
        _logger = logger;
        _dnsManager = dnsManager;
        _serverUrlIdnHost = config.NtpServerUrl;
    }

    public async Task<DateTime?> GetNetworkUtcTimeAsync(CancellationToken cancellationToken)
    {
        try
        {
            IPEndPoint ipEndPoint = await GetNtpServerIPEndPointAsync(cancellationToken);
            if (ipEndPoint is null)
            {
                _logger.Error<NetworkLog>("Failed to get the NTP server IP address.");
                return null;
            }

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
            _logger.Error<NetworkLog>("Failed to get the network time", e);
            return null;
        }
    }

    private async Task<IPEndPoint> GetNtpServerIPEndPointAsync(CancellationToken cancellationToken)
    {
        IList<IpAddress> ipAddresses = await _dnsManager.GetAsync(_serverUrlIdnHost, cancellationToken);
        return ipAddresses.Count == 0 ? null : new(ipAddresses.First().GetSystemType(), UDP_PORT);
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
        DateTime utcTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestampInMilliseconds);
        return HandleNtpRollover(utcTime);
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

    /// The NTP 64-bit timestamp will roll over on 7 February 2036.
    /// After this date, timestamps will appear to reset to 1900.
    private DateTime HandleNtpRollover(DateTime utcTime)
    {
        if (utcTime.Year < 2024)
        {
            utcTime = utcTime.AddYears(136); // 136 years = 2 ^ 32 seconds
        }

        return utcTime;
    }
}
