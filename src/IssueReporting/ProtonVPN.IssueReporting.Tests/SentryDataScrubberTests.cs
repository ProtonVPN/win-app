/*
 * Copyright (c) 2025 Proton AG
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
using ProtonVPN.IssueReporting.DataScrubbing;

namespace ProtonVPN.IssueReporting.Tests;

[TestClass]
public class SentryDataScrubberTests
{
    private SentryDataScrubber _scrubber;

    [TestInitialize]
    public void TestInitialize()
    {
        _scrubber = new SentryDataScrubber();
    }

    [TestMethod]
    public void Scrub_ShouldRedactEmailAddresses()
    {
        // Arrange
        string input = "User email is john.doe@example.com and contact is support@protonvpn.com";

        // Act
        string result = _scrubber.Scrub(input);

        // Assert
        result.Should().Be("User email is [REDACTED_EMAIL] and contact is [REDACTED_EMAIL]");
    }

    [TestMethod]
    public void Scrub_ShouldRedactUserProfileDirectory()
    {
        // Arrange
        string input = @"File path: C:\Users\JohnDoe\Documents\file.txt";

        // Act
        string result = _scrubber.Scrub(input);

        // Assert
        result.Should().Be(@"File path: C:\Users\[REDACTED_USER]\Documents\file.txt");
    }

    [TestMethod]
    public void Scrub_ShouldRedactAppDataLocalPath()
    {
        // Arrange
        string input = @"Config at C:\Users\JohnDoe\AppData\Local\ProtonVPN\config.json";

        // Act
        string result = _scrubber.Scrub(input);

        // Assert
        result.Should().Be(@"Config at C:\Users\[REDACTED_USER]\AppData\Local\ProtonVPN\config.json");
    }

    [TestMethod]
    public void Scrub_ShouldHandleMultiplePatterns()
    {
        // Arrange
        string input = @"User john@example.com has files in C:\Users\JohnDoe\AppData\Local";

        // Act
        string result = _scrubber.Scrub(input);

        // Assert
        result.Should().Be(@"User [REDACTED_EMAIL] has files in C:\Users\[REDACTED_USER]\AppData\Local");
    }

    [TestMethod]
    public void Scrub_ShouldReturnNullForNullInput()
    {
        // Act
        string result = _scrubber.Scrub(null);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void Scrub_ShouldReturnEmptyForEmptyInput()
    {
        // Arrange
        string input = string.Empty;

        // Act
        string result = _scrubber.Scrub(input);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void Scrub_ShouldRedactUserProfilePathOnAlternativeDrive()
    {
        // Arrange
        string input = @"Log file at E:\Users\Alice\Documents\logs\app.log";

        // Act
        string result = _scrubber.Scrub(input);

        // Assert
        result.Should().Be(@"Log file at E:\Users\[REDACTED_USER]\Documents\logs\app.log");
    }

    [TestMethod]
    public void Scrub_ShouldRedactEnvironmentVariablePaths()
    {
        // Arrange
        string input = @"Config at %USERPROFILE%\JohnDoe\settings.json";

        // Act
        string result = _scrubber.Scrub(input);

        // Assert
        result.Should().Be(@"Config at %USERPROFILE%\[REDACTED_USER]\settings.json");
    }

    [TestMethod]
    public void Scrub_ShouldRedactTildePaths()
    {
        // Arrange
        string input = @"Config at ~\BobSmith\.config\app.conf";

        // Act
        string result = _scrubber.Scrub(input);

        // Assert
        result.Should().Be(@"Config at ~\[REDACTED_USER]\.config\app.conf");
    }

    [TestMethod]
    public void Scrub_ShouldPreserveDriveLetterWhenRedactingUsername()
    {
        // Arrange
        string input = @"Files at F:\Users\TestUser\Downloads and G:\Users\AnotherUser\Desktop";

        // Act
        string result = _scrubber.Scrub(input);

        // Assert
        result.Should().Be(@"Files at F:\Users\[REDACTED_USER]\Downloads and G:\Users\[REDACTED_USER]\Desktop");
    }
}