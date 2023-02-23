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

namespace ProtonVPN.Vpn.Management
{
    /// <summary>
    /// Collection of predefined messages to be send to OpenVPN management interface.
    /// </summary>
    internal class ManagementMessages
    {
        public ReceivedManagementMessage ReceivedMessage(string messageText)
        {
            return new ReceivedManagementMessage(messageText ?? "");
        }

        public ManagementMessage EchoOn()
        {
            return ManagementMessage("echo on");
        }

        public ManagementMessage StateOn()
        {
            return ManagementMessage("state on");
        }

        public ManagementMessage Bytecount()
        {
            return ManagementMessage("bytecount 1");
        }

        public ManagementMessage LogOn()
        {
            return ManagementMessage("log on");
        }

        public ManagementMessage HoldRelease()
        {
            return ManagementMessage("hold release");
        }

        public ManagementMessage Username(string username)
        {
            return ManagementMessage($"username 'Auth' {EscapedString(username)}");
        }

        public ManagementMessage Password(string password)
        {
            return ManagementMessage($"password 'Auth' {EscapedString(password)}");
        }

        public ManagementMessage Disconnect()
        {
            return ManagementMessage("signal SIGTERM");
        }

        public ManagementMessage Exit()
        {
            return ManagementMessage("exit");
        }


        private ManagementMessage ManagementMessage(string messageText)
        {
            return new ManagementMessage(messageText);
        }

        private static string EscapedString(string value)
        {
            return "\"" + value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace(" ", "\\ ")
                + "\"";
        }
    }
}
