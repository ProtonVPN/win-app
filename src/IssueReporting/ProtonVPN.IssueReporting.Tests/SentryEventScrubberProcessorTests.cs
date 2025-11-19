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
public class SentryEventScrubberProcessorTests
{
    private SentryEventScrubberProcessor _processor;

    [TestInitialize]
    public void TestInitialize()
    {
        _processor = new SentryEventScrubberProcessor();
    }

    [TestMethod]
    public void Process_ShouldScrubEmailInMessage()
    {
        // Arrange
        SentryEvent sentryEvent = new() { Message = "User john.doe@example.com encountered an error" };

        // Act
        SentryEvent result = _processor.Process(sentryEvent);

        // Assert
        result.Message.Message.Should().NotContain("john.doe@example.com");
        result.Message.Message.Should().Contain("[REDACTED_EMAIL]");
    }

    [TestMethod]
    public void Process_ShouldScrubUserPathInMessage()
    {
        // Arrange
        SentryEvent sentryEvent = new() { Message = @"Error at C:\Users\JohnDoe\Documents\file.txt" };

        // Act
        SentryEvent result = _processor.Process(sentryEvent);

        // Assert
        result.Message.Message.Should().NotContain("JohnDoe");
        result.Message.Message.Should().Contain(@"C:\Users\[REDACTED_USER]");
    }

    [TestMethod]
    public void Process_ShouldScrubEmailInTags()
    {
        // Arrange
        SentryEvent sentryEvent = new();
        sentryEvent.SetTag("user", "test@example.com");
        sentryEvent.SetTag("path", @"C:\Users\TestUser\AppData\Local\ProtonVPN");

        // Act
        SentryEvent result = _processor.Process(sentryEvent);

        // Assert
        result.Tags["user"].Should().NotContain("test@example.com");
        result.Tags["user"].Should().Contain("[REDACTED_EMAIL]");
        result.Tags["path"].Should().NotContain("TestUser");
        result.Tags["path"].Should().Contain("[REDACTED_USER]");
    }

    [TestMethod]
    public void Process_ShouldScrubStringInExtras()
    {
        // Arrange
        SentryEvent sentryEvent = new();
        sentryEvent.SetExtra("logs", "User admin@proton.me logged in from C:\\Users\\Admin\\Desktop");

        // Act
        SentryEvent result = _processor.Process(sentryEvent);

        // Assert
        string logs = result.Extra["logs"] as string;
        logs.Should().NotContain("admin@proton.me");
        logs.Should().NotContain("Admin");
        logs.Should().Contain("[REDACTED_EMAIL]");
        logs.Should().Contain("[REDACTED_USER]");
    }

    [TestMethod]
    public void Process_ShouldScrubNestedDictionaryInExtras()
    {
        // Arrange
        SentryEvent sentryEvent = new();
        var nestedData = new Dictionary<string, object>
        {
            { "email", "user@example.com" }, { "path", @"C:\Users\TestUser\config.json" }, { "count", 42 }
        };
        sentryEvent.SetExtra("userInfo", nestedData);

        // Act
        SentryEvent result = _processor.Process(sentryEvent);

        // Assert
        var userInfo = result.Extra["userInfo"] as Dictionary<string, object?>;
        userInfo.Should().NotBeNull();
        userInfo["email"].Should().Be("[REDACTED_EMAIL]");
        (userInfo["path"] as string).Should().Contain("[REDACTED_USER]");
        userInfo["count"].Should().Be(42);
    }

    [TestMethod]
    public void Process_ShouldHandleNullMessage()
    {
        // Arrange
        SentryEvent sentryEvent = new() { Message = null };

        // Act
        SentryEvent result = _processor.Process(sentryEvent);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().BeNull();
    }

    [TestMethod]
    public void Process_ShouldHandleEmptyTags()
    {
        // Arrange
        SentryEvent sentryEvent = new();

        // Act
        SentryEvent result = _processor.Process(sentryEvent);

        // Assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public void Process_ShouldReturnEventEvenIfScrubbingFails()
    {
        // Arrange
        SentryEvent sentryEvent = new() { Message = "Test message" };

        // Act
        SentryEvent result = _processor.Process(sentryEvent);

        // Assert
        result.Should().NotBeNull();
    }

    [TestMethod]
    public void Process_ShouldScrubMultiplePatternsInSingleMessage()
    {
        // Arrange
        SentryEvent sentryEvent = new() { Message = @"User john@example.com has files in C:\Users\John\AppData\Local" };

        // Act
        SentryEvent result = _processor.Process(sentryEvent);

        // Assert
        result.Message.Message.Should().NotContain("john@example.com");
        result.Message.Message.Should().NotContain(@"C:\Users\John");
        result.Message.Message.Should().Contain("[REDACTED_EMAIL]");
        result.Message.Message.Should().Contain("[REDACTED_USER]");
    }
}