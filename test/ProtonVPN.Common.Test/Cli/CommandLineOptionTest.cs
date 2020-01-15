/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Common.Cli;

// ReSharper disable ObjectCreationAsStatement

namespace ProtonVPN.Common.Test.Cli
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
            var option = new CommandLineOption("zero", new string[0]);

            var result = option.Exists();
                
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Exists_ShouldBeFalse_WhenNoOptionInArgs()
        {
            var args = new [] { "zero", "one", "-two", "--three", "--h", "z", "-z", "--z", "0", "\"day.com\"" };
            var option = new CommandLineOption("zero", args);

            var result = option.Exists();

            result.Should().BeFalse();
        }

        [TestMethod]
        public void Exists_ShouldBeTrue_WhenOptionInArgs_PrecededWithForwardSlash()
        {
            var args = new[] { "/zero", "one", "-two", "--three" };
            var option = new CommandLineOption("zero", args);

            var result = option.Exists();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Exists_ShouldBeTrue_WhenOptionInArgs_PrecededWithMinus()
        {
            var args = new[] { "one", "-zero", "-two", "--three" };
            var option = new CommandLineOption("zero", args);

            var result = option.Exists();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Exists_ShouldBeTrue_WhenOptionInArgs_PrecededWithDoubleMinus()
        {
            var args = new[] { "one", "-two", "--three", "--zero" };
            var option = new CommandLineOption("zero", args);

            var result = option.Exists();

            result.Should().BeTrue();
        }

        [TestMethod]
        public void Params_ShouldBe_Empty()
        {
            var args = new[] { "one", "-two", "--zero" };
            var option = new CommandLineOption("zero", args);

            var result = option.Params();

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Params_ShouldBe_ArgsAfterOption_TillEnd()
        {
            var args = new[] { "--zero", "three", "\"http://daily.com\"" };
            var expected = new[] { "three", "\"http://daily.com\"" };
            var option = new CommandLineOption("zero", args);

            var result = option.Params();

            result.Should().ContainInOrder(expected);
        }
    }
}
