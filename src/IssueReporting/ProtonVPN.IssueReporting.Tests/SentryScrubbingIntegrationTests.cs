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
using NSubstitute;
using ProtonVPN.IssueReporting.DataScrubbing;
using ProtonVPN.Logging.Contracts;
using Sentry.Extensibility;
using Sentry.Protocol.Envelopes;

namespace ProtonVPN.IssueReporting.Tests;

[TestClass]
public class SentryScrubbingIntegrationTests
{
    private const string FAKE_DSN = "https://fakekey@fake-host.ingest.sentry.io/1234567";
    private SentryEvent? _capturedEvent;

    private ManualResetEventSlim _capturedSignal = null!;
    private ILogger _mockLogger = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _capturedSignal = new ManualResetEventSlim(false);
        _capturedEvent = null;

        _mockLogger = Substitute.For<ILogger>();
        _mockLogger.GetRecentLogs().Returns(new List<string>
        {
            "[2025-01-30 10:00:00] User testuser@protonvpn.com connected",
            @"[2025-01-30 10:01:00] Config at C:\Users\JohnDoe\AppData\Local\ProtonVPN\config.json"
        });

        SentrySdk.Close();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        SentrySdk.Close();

        _capturedSignal.Dispose();
    }

    [TestMethod]
    public void IntegrationTest_MessageWithEmailAndPath_ShouldBeScrubbed()
    {
        InitializeSentry();

        SentrySdk.CaptureMessage("User admin@proton.me accessed C:\\Users\\AdminUser\\Documents\\secret.txt");

        WaitForCapture();

        _capturedEvent.Should().NotBeNull();
        _capturedEvent!.Message!.Message.Should().NotContain("admin@proton.me");
        _capturedEvent.Message.Message.Should().Contain("[REDACTED_EMAIL]");
        _capturedEvent.Message.Message.Should().NotContain("AdminUser");
        _capturedEvent.Message.Message.Should().Contain(@"C:\Users\[REDACTED_USER]");
    }

    [TestMethod]
    public void IntegrationTest_TagsAndExtras_ShouldBeScrubbed()
    {
        InitializeSentry();

        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("user_email", "sensitive@protonmail.com");
            scope.SetExtra("file_path", @"C:\Users\TestUser\Desktop\data.txt");
            scope.SetExtra("safe_value", 42);
        });

        SentrySdk.CaptureMessage("Test event");

        WaitForCapture();

        _capturedEvent.Should().NotBeNull();
        _capturedEvent!.Tags!["user_email"].Should().Be("[REDACTED_EMAIL]");
        _capturedEvent.Tags["user_email"].Should().NotContain("sensitive@protonmail.com");

        string filePath = _capturedEvent.Extra["file_path"] as string;
        filePath.Should().Contain("[REDACTED_USER]");
        filePath.Should().NotContain("TestUser");

        _capturedEvent.Extra["safe_value"].Should().Be(42);
    }

    [TestMethod]
    public void IntegrationTest_LogsInExtras_ShouldBeScrubbed()
    {
        InitializeSentry();

        SentrySdk.CaptureMessage("Test - logs should be scrubbed");

        WaitForCapture();

        _capturedEvent.Should().NotBeNull();
        _capturedEvent!.Extra.Should().ContainKey("logs");

        string logs = _capturedEvent.Extra["logs"] as string;
        logs.Should().NotBeNull();
        logs!.Should().NotContain("testuser@protonvpn.com");
        logs.Should().Contain("[REDACTED_EMAIL]");
        logs.Should().NotContain("JohnDoe");
        logs.Should().Contain("[REDACTED_USER]");
        logs.Should().Contain(@"C:\Users\[REDACTED_USER]\AppData\Local");
    }

    private void InitializeSentry()
    {
        SentryOptions options = new()
        {
            Dsn = FAKE_DSN,
            Debug = false,
            AttachStacktrace = false,
            AutoSessionTracking = false,
            SampleRate = 1.0f,
            Transport = new NoOpTransport()
        };

        options.AddEventProcessor(new SentryEventScrubberProcessor());

        options.SetBeforeSend(e =>
        {
            SentryDataScrubber scrubber = new();

            e.SetTag("ProcessName", "TestProcess");
            e.User.Id = "test-device-id";

            string logs = string.Join("\n", _mockLogger.GetRecentLogs());
            e.SetExtra("logs", scrubber.Scrub(logs));

            _capturedEvent = e;
            _capturedSignal.Set();

            return e;
        });

        SentrySdk.Init(options);
    }

    private void WaitForCapture()
    {
        _capturedSignal.Wait(2000).Should().BeTrue("BeforeSend should be called");
    }

    private sealed class NoOpTransport : ITransport
    {
        public Task SendEnvelopeAsync(Envelope envelope, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}