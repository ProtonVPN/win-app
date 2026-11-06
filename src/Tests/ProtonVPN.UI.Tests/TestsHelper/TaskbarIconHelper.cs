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

using System;
using System.IO;
using System.Drawing;
using System.Threading;
using FlaUI.Core.Capturing;
using FlaUI.Core.Tools;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Robots;

namespace ProtonVPN.UI.Tests.TestsHelper;

// The taskbar overlay badge is set without an accessibility description, so it can only be verified visually.
// Set UpdateScreenshots to true to (re)create the reference images instead of asserting.
public static class TaskbarIconHelper
{
    public static bool UpdateScreenshots { get; set; } = false;

    // TODO: recreate with UpdateScreenshots = true once VPNWIN-3345 is fixed (app disconnected and window closed, not exited), or take the screenshot manually in that state
    public const string NO_BADGE_WINDOW_CLOSED = "TaskbarIconNoBadgeWindowClosed.png";
    // TODO: recreate with UpdateScreenshots = true once VPNWIN-3345 is fixed (app disconnected and exited, only the pinned icon left), or take the screenshot manually in that state
    public const string NO_BADGE_APP_EXITED = "TaskbarIconNoBadgeAppExited.png";
    // TODO: create with UpdateScreenshots = true once VPNWIN-3345 is fixed (app connected), or take the screenshot manually in that state
    public const string PROTECTED_BADGE = "TaskbarIconProtected.png";

    private const double REQUIRED_SIMILARITY = 0.95;
    private const int PIXEL_TOLERANCE = 15;

    private static readonly string _screenshotsDir = Path.Combine(TestConstants.SolutionRoot, @"src\Tests\ProtonVPN.UI.Tests\Screenshots\Taskbar");

    public static void VerifyTaskbarIcon(string referenceScreenshotName)
    {
        string referencePath = Path.Combine(_screenshotsDir, referenceScreenshotName);

        if (UpdateScreenshots)
        {
            Thread.Sleep(TestConstants.TwoSecondsTimeout);
            Directory.CreateDirectory(_screenshotsDir);
            using CaptureImage screenshot = CaptureTaskbarButton();
            screenshot.ToFile(referencePath);
            return;
        }

        Assert.That(File.Exists(referencePath), Is.True, $"Reference screenshot was not found: {referencePath}. Create it by running the test with TaskbarIconHelper.UpdateScreenshots set to true.");

        double similarity = 0;
        using Bitmap reference = new(referencePath);

        RetryResult<bool> retry = Retry.WhileFalse(() =>
        {
            using CaptureImage actual = CaptureTaskbarButton();
            similarity = InstallerHelper.CalculateImageSimilarity(reference, actual.Bitmap, PIXEL_TOLERANCE);
            return similarity >= REQUIRED_SIMILARITY;
        },
        TestConstants.TenSecondsTimeout,
        interval: TestConstants.OneSecondTimeout,
        ignoreException: true);

        if (!retry.Success)
        {
            SaveFailedScreenshot(referenceScreenshotName);
        }

        Assert.That(retry.Success, Is.True,
            $"Taskbar icon did not match '{referenceScreenshotName}'. Similarity: {similarity:P2}. Required: {REQUIRED_SIMILARITY:P2}.");
    }

    private static CaptureImage CaptureTaskbarButton()
    {
        Rectangle button = DesktopRobot.GetProtonVpnTaskbarButtonBounds()
            ?? throw new InvalidOperationException("Proton VPN taskbar button was not found. Make sure the app is pinned to the taskbar.");

        return Capture.Rectangle(button);
    }

    private static void SaveFailedScreenshot(string referenceScreenshotName)
    {
        try
        {
            string testName = TestContext.CurrentContext.Test.MethodName ?? throw new Exception("Test method name is null.");

            ArtifactsHelper.CreateTestFailureFolderIfNotExists();
            string directory = Path.Combine(ArtifactsHelper.ArtifactsDirectory, testName);
            Directory.CreateDirectory(directory);

            string referenceName = Path.GetFileNameWithoutExtension(referenceScreenshotName);
            string path = Path.Combine(directory, $"{referenceName}_ACTUAL_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            using CaptureImage screenshot = CaptureTaskbarButton();
            screenshot.ToFile(path);
            TestContext.AddTestAttachment(path, $"Failed screenshot for {referenceName}");
        }
        catch (Exception exception)
        {
            TestContext.WriteLine($"Could not save failed screenshot: {exception}");
        }
    }
}
