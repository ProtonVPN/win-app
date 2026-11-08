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
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class InstallerHelper
{
    public static bool UpdateScreenshots { get; set; } = false;
    private const double GLOBAL_SIMILARITY = 0.90;
    private const int GLOBAL_PIXEL_TOLERANCE = 15;

    private static string? _installerPath;
    private static readonly string _screenshotsDir = Path.Combine(TestConstants.SolutionRoot, @"src\Tests\ProtonVPN.UI.Tests\Screenshots\Installer");
    private static readonly string _installerDir = Path.Combine(TestConstants.SolutionRoot, @"Setup\Installers");
    private static string InstallerPath => _installerPath ??= Directory.GetFiles(_installerDir, "ProtonVPN_*.exe").FirstOrDefault() ?? throw new FileNotFoundException($"No installer found in: {_installerDir}");

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr windowHandle, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;

    static InstallerHelper()
    {
        SetProcessDPIAware();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public static Process StartInstaller(string? customInstallerPath = null, string? arguments = null)
    {
        string path = customInstallerPath ?? InstallerPath;

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Installer was not found: {path}");
        }

        ProcessStartInfo processStartInfo = new()
        {
            FileName = path,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true
        };

        return Process.Start(processStartInfo) ?? throw new InvalidOperationException($"Could not start installer: {path}");
    }

    public static void WaitForWindowToDisappear(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TestConstants.TwoMinutesTimeout;
        DateTime timeoutDate = DateTime.UtcNow + effectiveTimeout;

        while (DateTime.UtcNow < timeoutDate)
        {
            if (!Process.GetProcesses().Any(process =>
            {
                try
                {
                    return process.MainModule?.FileVersionInfo.FileDescription is "Proton VPN Setup" or "Setup/Uninstall";
                }
                catch
                {
                    return false;
                }
            }))
            {
                return;
            }

            Thread.Sleep(TestConstants.RetryInterval);
        }

        throw new TimeoutException($"Installer window did not disappear within {effectiveTimeout.TotalSeconds} seconds");
    }

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

    public static IntPtr WaitForAndFocusWindow(string title, TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TestConstants.ThirtySecondsTimeout;
        DateTime timeoutDate = DateTime.UtcNow + effectiveTimeout;

        while (DateTime.UtcNow < timeoutDate)
        {
            IntPtr windowHandle = FindWindow(null, title);

            if (windowHandle != IntPtr.Zero)
            {
                if (!GetWindowRect(windowHandle, out RECT rect))
                {
                    throw new InvalidOperationException($"Could not retrieve the window position: {title}");
                }

                SetForegroundWindow(windowHandle);
                Thread.Sleep(500);

                int x = rect.Left + ((rect.Right - rect.Left) / 2);
                int y = rect.Top + 12;

                Cursor.Position = new Point(x, y);

                Thread.Sleep(200);

                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                Thread.Sleep(TestConstants.OneSecondTimeout);

                return windowHandle;
            }

            Thread.Sleep(TestConstants.RetryInterval);
        }

        throw new TimeoutException($"Window was not found within {effectiveTimeout.TotalSeconds} seconds: {title}");
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
                IntPtr windowHandle = WaitForAndFocusWindow(windowTitle);
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

    private static double CalculateImageSimilarity(Bitmap reference, Bitmap actual, int pixelTolerance)
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
            IntPtr windowHandle = WaitForAndFocusWindow(windowTitle);
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
            string directory = Path.Combine(ArtifactsHelper.ArtifactsDirectory, testName);
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
        if (!GetWindowRect(windowHandle, out RECT rect))
        {
            throw new InvalidOperationException("Could not retrieve the installer window position.");
        }

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        int croppedWidth = width - (crop * 2);
        int croppedHeight = height - (crop * 2);

        if (croppedWidth <= 0 || croppedHeight <= 0)
        {
            throw new InvalidOperationException($"The window is too small to capture. " +
                $"Size: {width}x{height}, crop: {crop}px per side (needs more than {crop * 2}x{crop * 2}). " +
                $"The window is likely minimized or still being created.");
        }

        Bitmap bitmap = new(croppedWidth, croppedHeight, PixelFormat.Format32bppArgb);

        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(rect.Left + crop, rect.Top + crop, 0, 0, new Size(croppedWidth, croppedHeight), CopyPixelOperation.SourceCopy);

        return bitmap;
    }

    private static void CreateReferenceScreenshot(string windowTitle, string screenshotName)
    {
        Thread.Sleep(TestConstants.OneSecondTimeout);

        IntPtr windowHandle = WaitForAndFocusWindow(windowTitle);

        using Bitmap screenshot = CaptureWindow(windowHandle);

        Directory.CreateDirectory(_screenshotsDir);

        string screenshotPath = Path.Combine(_screenshotsDir, screenshotName);

        screenshot.Save(screenshotPath, ImageFormat.Png);

        TestContext.WriteLine($"Reference screenshot created: {screenshotPath}");
    }
}