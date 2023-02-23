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

using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Api.Handlers.TlsPinning;

namespace ProtonVPN.Api.Tests.Handlers.TlsPinning
{
    [TestClass]
    public class PublicKeyInfoHashTest
    {
        [TestMethod]
        [DataRow("TestData\\rsa4096.badssl.com.cer", "W8/42Z0ffufwnHIOSndT+eVzBJSC0E8uTIC8O6mEliQ=")]
        [DataRow("TestData\\self-signed.badssl.com.cer", "9SLklscvzMYj8f+52lp5ze/hY0CFHyLSPQzSpYYIBm8=")]
        public void Value_ShouldReturnCorrectPin(string certFile, string key)
        {
            X509Certificate cert = X509Certificate.CreateFromCertFile(certFile);
            PublicKeyInfoHash keyHash = new(cert);
            keyHash.Value().Should().Be(key);
        }
    }
}