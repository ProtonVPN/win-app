/*
 * Copyright (c) 2021 Proton Technologies AG
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProtonVPN.UI.Test.Results
{
    public class ConnectionResult
    {
        public void CheckIfDnsIsResolved()
        {
            try
            {
                int ipAddressCount = Dns.GetHostEntry("www.ip.me").AddressList.Length;
                Assert.IsTrue(ipAddressCount > 0);
            }
            catch(SocketException ex)
            {
                //Intentionally fail assertion if DNS was not resolved
                Assert.IsTrue(false, "DNS was not resolved. \n" + ex.Message);
            }
        }
    }
}
