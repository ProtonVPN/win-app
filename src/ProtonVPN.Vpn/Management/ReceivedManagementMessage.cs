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

using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Extensions;

namespace ProtonVPN.Vpn.Management
{
    /// <summary>
    /// Message received over OpenVPN management interface.
    /// </summary>
    internal class ReceivedManagementMessage
    {
        private readonly string _messageText;

        public ReceivedManagementMessage(string messageText)
        {
            _messageText = messageText ?? "";
        }

        public override string ToString() => _messageText;

        public bool IsChannelDisconnected => _messageText.IsNullOrEmpty();

        public bool IsWaitingHoldRelease => _messageText.StartsWithIgnoringCase(">HOLD");

        public bool IsEchoSet => _messageText.StartsWithIgnoringCase("SUCCESS: real-time echo notification set");

        public bool IsStateSet => _messageText.StartsWithIgnoringCase("SUCCESS: real-time state notification set");

        public bool IsByteCountSet => _messageText.StartsWithIgnoringCase("SUCCESS: bytecount");

        public bool IsUsernameNeeded => _messageText.StartsWithIgnoringCase(">PASSWORD:Need");

        public bool IsPasswordNeeded => _messageText.StartsWithIgnoringCase("SUCCESS: 'Auth' username entered");

        public bool IsLogSet => _messageText.StartsWithIgnoringCase("SUCCESS: real-time log notification set");

        public bool IsByteCount => _messageText.StartsWithIgnoringCase(">BYTECOUNT");

        public bool IsState => _messageText.StartsWithIgnoringCase(">STATE");

        public bool IsError => ManagementError.ContainsError(_messageText);

        public bool IsDisconnectReceived => _messageText.StartsWithIgnoringCase("SUCCESS: signal SIGTERM thrown");

        public bool IsControlMessage => _messageText.ContainsIgnoringCase("PUSH: Received control message");

        public NetworkTraffic Bandwidth()
        {
            string[] byteCountArr = _messageText.Split(':')[1].Split(',');
            if (byteCountArr.Length <= 1)
            {
                return NetworkTraffic.Zero;
            }

            if (!ulong.TryParse(byteCountArr[0], out ulong bytesIn))
            {
                return NetworkTraffic.Zero;
            }

            if (!ulong.TryParse(byteCountArr[1], out ulong bytesOut))
            {
                return NetworkTraffic.Zero;
            }

            return new NetworkTraffic(bytesIn, bytesOut);
        }

        public ManagementState State()
        {
            string[] messageParts = _messageText.Split(',');
            if (messageParts.Length <= 1)
            {
                return ManagementState.Null;
            }

            string stateText = messageParts[1];
            string statusText = messageParts.Length > 2 ? messageParts[2] : "";
            string localIpAddress = messageParts.Length > 3 ? messageParts[3] : "";
            string remoteIpAddress = messageParts.Length > 4 ? messageParts[4] : "";

            return new ManagementState(stateText, statusText, localIpAddress, remoteIpAddress);
        }

        public ManagementError Error()
        {
            return new ManagementError(_messageText);
        }
    }
}