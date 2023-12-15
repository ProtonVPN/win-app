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

        public static void StartVideoCapture()
        {
            if (TestEnvironment.AreTestsRunningLocally() || !TestEnvironment.IsVideoRecorderPresent())
            {
                return;
            }

            string screenshotDir = CreateTestArtifactFolder();
            string pathToVideo = Path.Combine(screenshotDir, "TestRun.mp4");
            Recorder = new VideoRecorder(new VideoRecorderSettings { VideoQuality = 26, ffmpegPath = TestData.PathToRecorder, TargetVideoPath = pathToVideo }, recorder =>
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

        public static void SaveScreenshot(string testName)
        {
            string artifactsDir = CreateTestArtifactFolder();
            string screenshotName = $"{testName} {DateTime.Now}.png".Replace("/", "-").Replace(":", "-");
            string pathToTestArtifact = Path.Combine(artifactsDir, testName);
            string pathToScreenshot = Path.Combine(pathToTestArtifact, screenshotName);
            Capture.Screen().ToFile(pathToScreenshot);
        }

        public static void SaveLogs(string testName)
        {
            string artifactsDir = CreateTestArtifactFolder();
            string pathToTestArtifact = Path.Combine(artifactsDir, testName);
            Directory.CreateDirectory(pathToTestArtifact);
            if (File.Exists(TestData.AppLogsPath))
            {
                File.Copy(TestData.AppLogsPath, pathToTestArtifact + @"\app-logs.txt", true);
            }
            if (File.Exists(TestData.ServiceLogsPath))
            {
                File.Copy(TestData.ServiceLogsPath, pathToTestArtifact + @"\service-logs.txt", true);
            }
        }

        private static string CreateTestArtifactFolder()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string testArtifactFolderPath = Path.Combine(Path.GetDirectoryName(asm.Location), "TestArtifactData");
            if(!Directory.Exists(testArtifactFolderPath))
            {
                Directory.CreateDirectory(testArtifactFolderPath);
            }
            return testArtifactFolderPath;
        }
    }
}
