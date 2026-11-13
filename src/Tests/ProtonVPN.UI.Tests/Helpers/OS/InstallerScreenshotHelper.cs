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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class InstallerScreenshotHelper
{
    public static bool UpdateScreenshots { get; set; } = false;
    private const double GLOBAL_SIMILARITY = 0.90;
    private const int GLOBAL_PIXEL_TOLERANCE = 15;

    private static readonly string _screenshotsDir = Path.Combine(TestConstants.SolutionRoot, @"src\Tests\ProtonVPN.UI.Tests\Screenshots\Installer");

    public static void AssertWindowMatchesScreenshot(string windowTitle, string referenceScreenshotName, double requiredSimilarity)
    {
        bool matches = IsWindowMatchingScreenshot(windowTitle, referenceScreenshotName, out double similarity, out string? lastError, requiredSimilarity);

        if (!matches)
        {
            SaveFailedScreenshot(windowTitle, referenceScreenshotName);
        }

        Assert.That(matches, Is.True,
            $"Installer screenshot did not match '{referenceScreenshotName}'. " +
            $"Similarity: {similarity:P2}. " +
            $"Required: {requiredSimilarity:P2}." +
            (lastError is null ? string.Empty : $" Last error: {lastError}"));
    }

    public static void VerifyScreenshot(string windowTitle, string screenshotName, double requiredSimilarity = GLOBAL_SIMILARITY)
    {
        if (UpdateScreenshots)
        {
            CreateReferenceScreenshot(windowTitle, screenshotName);
        }
        else
        {
            AssertWindowMatchesScreenshot(windowTitle, screenshotName, requiredSimilarity);
        }
    }

    public static void VerifyScreenshotNonFatal(string windowTitle, string screenshotName, double requiredSimilarity = GLOBAL_SIMILARITY)
    {
        if (UpdateScreenshots)
        {
            CreateReferenceScreenshot(windowTitle, screenshotName);
            return;
        }

        bool matches = IsWindowMatchingScreenshot(windowTitle, screenshotName, out double similarity, out string? lastError, requiredSimilarity);

        if (!matches)
        {
            TestContext.WriteLine(
                $"[non-fatal] Installer screenshot did not match '{screenshotName}'. " +
                $"Similarity: {similarity:P2}. Required: {requiredSimilarity:P2}." +
                (lastError is null ? string.Empty : $" Last error: {lastError}"));
        }
    }

    public static bool IsWindowMatchingScreenshot(
        string windowTitle,
        string referenceScreenshotName,
        out double similarity,
        out string? lastError,
        double requiredSimilarity = GLOBAL_SIMILARITY,
        int pixelTolerance = GLOBAL_PIXEL_TOLERANCE)
    {
        similarity = 0;
        double currentSimilarity = 0;
        Exception? lastException = null;

        RetryResult<bool> retry = Retry.WhileFalse(() =>
        {
            try
            {
                IntPtr windowHandle = WindowHelper.WaitForAndFocusWindow(windowTitle);
                using Bitmap actualScreenshot = CaptureWindow(windowHandle);

                string referencePath = Path.Combine(_screenshotsDir, referenceScreenshotName);
                if (!File.Exists(referencePath))
                {
                    throw new FileNotFoundException($"Reference screenshot was not found: {referencePath}");
                }

                using Bitmap referenceScreenshot = new(referencePath);

                currentSimilarity = CalculateImageSimilarity(referenceScreenshot, actualScreenshot, pixelTolerance);
                lastException = null;
                return currentSimilarity >= requiredSimilarity;
            }
            catch (Exception ex)
            {
                lastException = ex;
                throw;
            }
        },
        TestConstants.TenSecondsTimeout,
        interval: TestConstants.OneSecondTimeout,
        ignoreException: true);

        similarity = currentSimilarity;
        lastError = lastException is null ? null : $"{lastException.GetType().Name}: {lastException.Message}";

        return retry.Success;
    }

    internal static double CalculateImageSimilarity(Bitmap reference, Bitmap actual, int pixelTolerance)
    {
        using Bitmap resizedActual = actual.Width == reference.Width && actual.Height == reference.Height
            ? new Bitmap(actual)
            : new Bitmap(actual, reference.Width, reference.Height);

        Rectangle rectangle = new(0, 0, reference.Width, reference.Height);

        BitmapData referenceData = reference.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData actualData = resizedActual.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try
        {
            int bytes = Math.Abs(referenceData.Stride) * reference.Height;
            byte[] referencePixels = new byte[bytes];
            byte[] actualPixels = new byte[bytes];

            Marshal.Copy(referenceData.Scan0, referencePixels, 0, bytes);
            Marshal.Copy(actualData.Scan0, actualPixels, 0, bytes);

            long matchingPixels = 0;
            long totalPixels = (long)reference.Width * reference.Height;

            for (int y = 0; y < reference.Height; y++)
            {
                int referenceRow = y * referenceData.Stride;
                int actualRow = y * actualData.Stride;

                for (int x = 0; x < reference.Width; x++)
                {
                    int referenceIndex = referenceRow + (x * 4);
                    int actualIndex = actualRow + (x * 4);

                    int blueDifference = Math.Abs(referencePixels[referenceIndex] - actualPixels[actualIndex]);
                    int greenDifference = Math.Abs(referencePixels[referenceIndex + 1] - actualPixels[actualIndex + 1]);
                    int redDifference = Math.Abs(referencePixels[referenceIndex + 2] - actualPixels[actualIndex + 2]);

                    if (redDifference <= pixelTolerance && greenDifference <= pixelTolerance && blueDifference <= pixelTolerance)
                    {
                        matchingPixels++;
                    }
                }
            }

            return (double)matchingPixels / totalPixels;
        }
        finally
        {
            reference.UnlockBits(referenceData);
            resizedActual.UnlockBits(actualData);
        }
    }

    private static void SaveFailedScreenshot(string windowTitle, string referenceScreenshotName, int pixelTolerance = GLOBAL_PIXEL_TOLERANCE)
    {
        try
        {
            string testName = TestContext.CurrentContext.Test.MethodName ?? throw new Exception("Test method name is null.");
            IntPtr windowHandle = WindowHelper.WaitForAndFocusWindow(windowTitle);
            using Bitmap screenshot = CaptureWindow(windowHandle);

            string referencePath = Path.Combine(_screenshotsDir, referenceScreenshotName);
            using Bitmap reference = new(referencePath);

            if (reference.Width == screenshot.Width && reference.Height == screenshot.Height)
            {
                for (int y = 0; y < screenshot.Height; y++)
                {
                    for (int x = 0; x < screenshot.Width; x++)
                    {
                        Color expected = reference.GetPixel(x, y);
                        Color actual = screenshot.GetPixel(x, y);

                        if (Math.Abs(expected.R - actual.R) > pixelTolerance ||
                            Math.Abs(expected.G - actual.G) > pixelTolerance ||
                            Math.Abs(expected.B - actual.B) > pixelTolerance)
                        {
                            screenshot.SetPixel(x, y, Color.Red);
                        }
                    }
                }
            }

            ArtifactsHelper.CreateTestFailureFolderIfNotExists();
            string directory = Path.Combine(ArtifactsHelper.ArtifactsDirectory, Path.GetFileName(testName));
            Directory.CreateDirectory(directory);
            string referenceName = Path.GetFileNameWithoutExtension(referenceScreenshotName);
            string path = Path.Combine(directory, $"{referenceName}_ACTUAL_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            screenshot.Save(path, ImageFormat.Png);
            TestContext.AddTestAttachment(path, $"Failed screenshot for {referenceName}");
            TestContext.WriteLine($"Actual screenshot with differences highlighted saved: {path}");
        }
        catch (Exception exception)
        {
            TestContext.WriteLine($"Could not save failed screenshot: {exception}");
        }
    }

    private static Bitmap CaptureWindow(IntPtr windowHandle, int crop = 8)
    {
        if (!WindowHelper.TryGetWindowBounds(windowHandle, out Rectangle bounds))
        {
            throw new InvalidOperationException("Could not retrieve the installer window position.");
        }

        int croppedWidth = bounds.Width - (crop * 2);
        int croppedHeight = bounds.Height - (crop * 2);

        if (croppedWidth <= 0 || croppedHeight <= 0)
        {
            throw new InvalidOperationException($"The window is too small to capture. " +
                $"Size: {bounds.Width}x{bounds.Height}, crop: {crop}px per side (needs more than {crop * 2}x{crop * 2}). " +
                $"The window is likely minimized or still being created.");
        }

        Bitmap bitmap = new(croppedWidth, croppedHeight, PixelFormat.Format32bppArgb);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(bounds.Left + crop, bounds.Top + crop, 0, 0, new Size(croppedWidth, croppedHeight), CopyPixelOperation.SourceCopy);

        return bitmap;
    }

    private static void CreateReferenceScreenshot(string windowTitle, string screenshotName)
    {
        Thread.Sleep(TestConstants.OneSecondTimeout);

        IntPtr windowHandle = WindowHelper.WaitForAndFocusWindow(windowTitle);

        using Bitmap screenshot = CaptureWindow(windowHandle);

        Directory.CreateDirectory(_screenshotsDir);

        string screenshotPath = Path.Combine(_screenshotsDir, screenshotName);

        screenshot.Save(screenshotPath, ImageFormat.Png);

        TestContext.WriteLine($"Reference screenshot created: {screenshotPath}");
    }
}
