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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Vpn.OpenVpn.Arguments;

namespace ProtonVPN.Vpn.Tests.OpenVpn.Arguments
{
    [TestClass]
    public class CommandLineArgumentsTest
    {
        [TestMethod]
        public void ToString_ShouldBe_JoinedStringOfAllArguments()
        {
            // Arrange
            string[] args1 = new[] { "arg1", "arg3" };
            string[] args2 = new[] { "arg2" };
            string[] args3 = new[] { "arg10", "\"arg11\"", "--arg20" };

            CommandLineArguments subject = new CommandLineArguments()
                                           .Add(args1)
                                           .Add(args2)
                                           .Add(args3);

            // Act
            string result = subject.ToString();

            // Assert
            result.Should().Be("arg1 arg3 arg2 arg10 \"arg11\" --arg20");
        }
    }
}
