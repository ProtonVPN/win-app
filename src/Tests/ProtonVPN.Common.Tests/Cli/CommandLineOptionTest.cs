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
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Cli;

// ReSharper disable ObjectCreationAsStatement

namespace ProtonVPN.Common.Tests.Cli
{
    [TestClass]
    public class CommandLineOptionTest
    {
        [TestMethod]
        public void Option_ShouldThrow_WhenArgsIsNull()
        {
            Action action = () => new CommandLineOption("zero", null);

            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Exists_ShouldBeFalse_WhenEmptyArgs()
        {
            CommandLineOption option = new("zero", new string[0]);

            bool result = option.Exists();
                
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Exists_ShouldBeFalse_WhenNoOptionInArgs()
        {
            string[] args = new [] { "zero", "one", "-two", "--three", "--h", "z", "-z", "--z", "0", "\"day.com\"" };
            CommandLineOption option = new("zero", args);

            bool result = option.Exists();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Exists_ShouldBeTrue_WhenOptionInArgs_PrecededWithForwardSlash()
        {
            string[] args = new[] { "/zero", "one", "-two", "--three" };
            CommandLineOption option = new("zero", args);

            bool result = option.Exists();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Exists_ShouldBeTrue_WhenOptionInArgs_PrecededWithMinus()
        {
            string[] args = new[] { "one", "-zero", "-two", "--three" };
            CommandLineOption option = new("zero", args);

            bool result = option.Exists();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Exists_ShouldBeTrue_WhenOptionInArgs_PrecededWithDoubleMinus()
        {
            string[] args = new[] { "one", "-two", "--three", "--zero" };
            CommandLineOption option = new("zero", args);

            bool result = option.Exists();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Params_ShouldBe_Empty()
        {
            string[] args = new[] { "one", "-two", "--zero" };
            CommandLineOption option = new("zero", args);

            IReadOnlyList<string> result = option.Params();

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Params_ShouldBe_ArgsAfterOption_TillEnd()
        {
            string[] args = new[] { "--zero", "three", "\"http://daily.com\"" };
            string[] expected = new[] { "three", "\"http://daily.com\"" };
            CommandLineOption option = new("zero", args);

            IReadOnlyList<string> result = option.Params();

            result.Should().ContainInOrder(expected);
        }
    }
}
