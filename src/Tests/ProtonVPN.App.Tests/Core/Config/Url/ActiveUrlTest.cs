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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Config.Url;

namespace ProtonVPN.App.Tests.Core.Config.Url
{
    [TestClass]
    public class ActiveUrlTest
    {
        private IOsProcesses _processes;

        [TestInitialize]
        public void TestInitialize()
        {
            _processes = Substitute.For<IOsProcesses>();
        }

        [TestMethod]
        public void Uri_ShouldBe_FromUrl()
        {
            const string url = "https://whatsup.com/best/things.html";
            ActiveUrl item = new ActiveUrl(_processes, url);

            Uri result = item.Uri;

            result.OriginalString.Should().Be(url);
        }

        [TestMethod]
        public void Open_ShouldCall_Processes_Open()
        {
            const string url = "https://whatsup.com/best/things.html";
            ActiveUrl item = new ActiveUrl(_processes, url);

            item.Open();

            _processes.Received().Open(url);
        }
    }
}
