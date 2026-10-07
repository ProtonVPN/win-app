/*
 * Copyright (c) 2026 Proton AG
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

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtonVPN.Common.Legacy.OS.Processes;

namespace ProtonVPN.Common.Tests.OS.Processes;

[TestClass]
public class CommandLineArgumentsParserTest
{
    [TestMethod]
    // An empty command line yields no arguments at all
    [DataRow(null, new string[0])]
    [DataRow("", new string[0])]
    [DataRow(" ", new string[0])]
    [DataRow("\t", new string[0])]

    // Spaces and tabs separate arguments, repeated or surrounding ones produce no empty arguments
    [DataRow("/qb", new[] { "/qb" })]
    [DataRow("/qb /norestart", new[] { "/qb", "/norestart" })]
    [DataRow("/qb\t/norestart", new[] { "/qb", "/norestart" })]
    [DataRow("/qb  /norestart", new[] { "/qb", "/norestart" })]
    [DataRow(" /qb /norestart ", new[] { "/qb", "/norestart" })]

    // Quotes keep the spaces of a value, whether standalone or attached to its switch
    [DataRow("--path \"C:\\Program Files\\App\"", new[] { "--path", "C:\\Program Files\\App" })]
    [DataRow("/dir=\"C:\\Program Files\"", new[] { "/dir=C:\\Program Files" })]

    // Backslashes are literal unless they precede a quote
    [DataRow("C:\\path\\to", new[] { "C:\\path\\to" })]

    // A backslash pair before the closing quote is how ProcessStartInfo.ArgumentList escapes a value
    // ending in a backslash, so it has to survive the round trip
    [DataRow("\"C:\\Program Files\\App\\\\\"", new[] { "C:\\Program Files\\App\\" })]

    // A quote is literal when escaped by a backslash, or when doubled inside a quoted value
    [DataRow("--name \\\"quoted\\\"", new[] { "--name", "\"quoted\"" })]
    [DataRow("\"a\"\"b\"", new[] { "a\"b" })]

    // Empty quotes are an intentional empty argument
    [DataRow("\"\"", new[] { "" })]
    public void TestParse(string commandLine, string[] expectedArguments)
    {
        IReadOnlyList<string> result = CommandLineArgumentsParser.Parse(commandLine);

        result.Should().Equal(expectedArguments);
    }
}