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
using System.IO;
using System.Reflection;
using FlaUI.Core.Capturing;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper
{
    public class TestsRecorder
    {
        private static VideoRecorder Recorder;
        private static string ScreenshotDir { get; set; }

        public static void StartVideoCapture()
        {
            if (TestEnvironment.AreTestsRunningLocally() || !TestEnvironment.IsVideoRecorderPresent())
            {
                return;
            }

            CreateTestFailureFolder();
            string pathToVideo = Path.Combine(ScreenshotDir, "TestRun.mp4");
            Recorder = new VideoRecorder(new VideoRecorderSettings { VideoQuality = 26, ffmpegPath = TestConstants.PathToRecorder, TargetVideoPath = pathToVideo }, recorder =>
            {
                string testName = TestContext.CurrentContext.Test.MethodName;
                CaptureImage img = Capture.Screen(1);
                img.ApplyOverlays(new InfoOverlay(img) { 
                    RecordTimeSpan = recorder.RecordTimeSpan, 
                    OverlayStringFormat = @"{rt:hh\:mm\:ss\.fff} / {name} / CPU: {cpu} / RAM: {mem.p.used}/{mem.p.tot} ({mem.p.used.perc})" }, 
                    new MouseOverlay(img));
                return img;
            });
        }

        public static void StopRecording()
        {
            if (TestEnvironment.AreTestsRunningLocally() || !TestEnvironment.IsVideoRecorderPresent())
            {
                return;
            }
            Recorder.Stop();
        }

        public static void SaveScreenshotAndLogs(string testName)
        {
            CreateTestFailureFolder();
            string screenshotName = $"{testName} {DateTime.Now}.png".Replace("/", "-").Replace(":", "-");
            string pathToScreenshotFolder = Path.Combine(ScreenshotDir, testName);
            string pathToScreenshot = Path.Combine(pathToScreenshotFolder, screenshotName);
            Directory.CreateDirectory(pathToScreenshotFolder);
            Capture.Screen().ToFile(pathToScreenshot);
            File.Copy(TestConstants.AppLogsPath, pathToScreenshotFolder + @"\app-logs.txt", true);
            File.Copy(TestConstants.ServiceLogsPath, pathToScreenshotFolder + @"\service-logs.txt", true);
        }

        private static void CreateTestFailureFolder()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            ScreenshotDir = Path.Combine(Path.GetDirectoryName(asm.Location), "TestFailureData");
            if(!Directory.Exists(ScreenshotDir))
            {
                Directory.CreateDirectory(ScreenshotDir);
            }
        }
    }
}
