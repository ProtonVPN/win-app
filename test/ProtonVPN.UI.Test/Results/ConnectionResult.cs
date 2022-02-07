/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProtonVPN.UI.Test.Results
{
    public class ConnectionResult
    {
        public void CheckIfDnsIsResolved()
        {
            WaitUntilInternetConnectionIsRestored();
            Assert.IsTrue(IsConnectedToInternet(), "User was not connected to internet.");
        }

        public static void WaitUntilInternetConnectionIsRestored()
        {
            for (int i = 0; i < 10; i++)
            {
                if (IsConnectedToInternet())
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }

        private static bool IsConnectedToInternet()
        {
            bool isConnected = true;
            try
            {
                Dns.GetHostEntry("www.google.com");
            }
            catch (SocketException ex)
            {
                isConnected = false;
            }
            return isConnected;
        }
    }
}
