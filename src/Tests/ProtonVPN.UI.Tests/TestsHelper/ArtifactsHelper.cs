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
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FlaUI.Core.Capturing;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class ArtifactsHelper
{
    public static VideoRecorder Recorder;
    private static string ArtifactsDirectory { get; set; }
    private static EventLog _eventViewerLogs = EventLog.GetEventLogs().Where(logs => logs.Log == "Application").FirstOrDefault();

    public static async Task StartVideoCaptureAsync(string testName)
    {
        string recorderFolder = @"C:\TestRecorder\";
        string recorderFullPath = Path.Combine(recorderFolder, "ffmpeg.exe");

        if (!File.Exists(recorderFullPath))
        {
            await VideoRecorder.DownloadFFMpeg(recorderFolder);
        }

        string pathToVideo = Path.Combine(ArtifactsDirectory, testName, $"{testName}-recording.mp4");
        Recorder = new VideoRecorder(new VideoRecorderSettings { VideoQuality = 18, FrameRate = 10u,ffmpegPath = recorderFullPath, TargetVideoPath = pathToVideo }, recorder =>
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

    public static void SaveScreenshotAndLogs(string testName, string serviceLogsPath)
    {
        CreateTestFailureFolderIfNotExists();
        string screenshotName = $"{testName} {DateTime.Now}.png".Replace("/", "-").Replace(":", "-");
        string pathToScreenshotFolder = Path.Combine(ArtifactsDirectory, testName);
        string pathToScreenshot = Path.Combine(pathToScreenshotFolder, screenshotName);
        Directory.CreateDirectory(pathToScreenshotFolder);
        Capture.Screen().ToFile(pathToScreenshot);
        if (File.Exists(TestConstants.ClientLogsPath))
        {
            File.Copy(TestConstants.ClientLogsPath, pathToScreenshotFolder + @"\client-logs.txt", true);
            File.Copy(serviceLogsPath, pathToScreenshotFolder + @"\service-logs.txt", true);
        }
    }

    public static void SaveEventViewerLogs(string testName)
    {
        string filePath = Path.Combine(ArtifactsDirectory, testName, "EventViewerLogs.evtx");
        
        using (EventLogSession session = new EventLogSession())
        {
            session.ExportLog(_eventViewerLogs.Log, PathType.LogName, "*", filePath, true);
        }
    }

    public static void DeleteArtifactFolder(string testName)
    {
        string pathToVideo = Path.Combine(ArtifactsDirectory, testName);
        if (Directory.Exists(pathToVideo))
        {
            Directory.Delete(pathToVideo, true);
        }
    }

    public static void ClearEventViewerLogs()
    {
        _eventViewerLogs.Clear();
    }

    public static void CreateTestFailureFolderIfNotExists()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        ArtifactsDirectory = Path.Combine(Path.GetDirectoryName(asm.Location), "TestFailureData");
        if(!Directory.Exists(ArtifactsDirectory))
        {
            Directory.CreateDirectory(ArtifactsDirectory);
        }
    }
}