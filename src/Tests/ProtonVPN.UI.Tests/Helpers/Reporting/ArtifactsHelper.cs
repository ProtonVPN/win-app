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

using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
using FlaUI.Core.Capturing;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class ArtifactsHelper
{
    public static VideoRecorder? Recorder;
    public static string ArtifactsDirectory { get; set; } = string.Empty;
    private static System.Timers.Timer? _videoTimer;
    private static readonly object _recorderLock = new();
    private const string EVENT_VIEWER_LOG_NAME = "Application";
    private static bool _isVideoRecordingUnavailable;

    public static async Task StartVideoCaptureAsync(string testName)
    {
        // The video is only a diagnostic aid, so a missing or broken recorder must never fail the test's SetUp.
        if (_isVideoRecordingUnavailable)
        {
            return;
        }

        string recorderFolder = @"C:\TestRecorder\";
        string recorderFullPath = Path.Combine(recorderFolder, "ffmpeg.exe");

        try
        {
            if (!File.Exists(recorderFullPath))
            {
                Task download = VideoRecorder.DownloadFFMpeg(recorderFolder);
                if (await Task.WhenAny(download, Task.Delay(TestConstants.TwoMinutesTimeout)) != download)
                {
                    throw new TimeoutException($"Downloading ffmpeg did not finish within {TestConstants.TwoMinutesTimeout.TotalSeconds}s.");
                }

                await download;
            }

            string pathToVideo = Path.Combine(ArtifactsDirectory, testName, $"{testName}-recording.mp4");
            Recorder = new VideoRecorder(new VideoRecorderSettings { VideoQuality = 18, FrameRate = 10u, ffmpegPath = recorderFullPath, TargetVideoPath = pathToVideo }, recorder =>
            {
                CaptureImage img = Capture.Screen(1);
                img.ApplyOverlays(new InfoOverlay(img)
                {
                    RecordTimeSpan = recorder.RecordTimeSpan,
                    OverlayStringFormat = @"{rt:hh\:mm\:ss\.fff} / {name} / CPU: {cpu} / RAM: {mem.p.used}/{mem.p.tot} ({mem.p.used.perc})"
                }, new MouseOverlay(img));
                return img;
            });
        }
        catch (Exception ex)
        {
            _isVideoRecordingUnavailable = true;
            TestContext.WriteLine($"Video recording is disabled for this run: {ex.Message}");
            return;
        }

        // Safety cap: force-finalize the video after 5 min even if the test hangs.
        _videoTimer?.Stop();
        _videoTimer?.Dispose();
        _videoTimer = new System.Timers.Timer(5 * 60 * 1000) { AutoReset = false };
        _videoTimer.Elapsed += (_, _) => StopRecorderSafely();
        _videoTimer.Start();
    }

    public static void StopRecorderSafely()
    {
        lock (_recorderLock)
        {
            try
            { Recorder?.Stop(); }
            catch { }
            try
            { Recorder?.Dispose(); }
            catch { }
            Recorder = null;
        }
        try
        { _videoTimer?.Stop(); }
        catch { }
    }

    public static void SaveScreenshotAndLogs(string testName, string serviceLogsPath)
    {
        string safeTestName = Path.GetFileName(testName);

        CreateTestFailureFolderIfNotExists();
        string screenshotName = $"{safeTestName} {DateTime.Now}.png".Replace("/", "-").Replace(":", "-");
        string pathToScreenshotFolder = Path.Combine(ArtifactsDirectory, safeTestName);
        string pathToScreenshot = Path.Combine(pathToScreenshotFolder, screenshotName);
        Directory.CreateDirectory(pathToScreenshotFolder);
        Capture.Screen().ToFile(pathToScreenshot);
        CopyIfExists(TestConstants.ClientLogsPath, Path.Combine(pathToScreenshotFolder, "client-logs.txt"));
        CopyIfExists(serviceLogsPath, Path.Combine(pathToScreenshotFolder, "service-logs.txt"));
        File.WriteAllText(Path.Combine(pathToScreenshotFolder, "system-diagnostics.txt"), ScriptHelper.CollectSystemDiagnostics());
    }

    public static void SaveEventViewerLogs(string testName)
    {
        string testFolder = Path.Combine(ArtifactsDirectory, Path.GetFileName(testName));

        // The video recorder is what normally creates this folder, so without a recording it is missing and the export throws.
        Directory.CreateDirectory(testFolder);
        string filePath = Path.Combine(testFolder, "EventViewerLogs.evtx");

        using (EventLogSession session = new())
        {
            session.ExportLog(EVENT_VIEWER_LOG_NAME, PathType.LogName, "*", filePath, true);
        }
    }

    private static void CopyIfExists(string sourcePath, string destinationPath)
    {
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destinationPath, true);
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
        using EventLog eventViewerLog = new(EVENT_VIEWER_LOG_NAME);
        eventViewerLog.Clear();
    }

    public static void CreateTestFailureFolderIfNotExists()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        ArtifactsDirectory = Path.Combine(Path.GetDirectoryName(asm.Location) ?? string.Empty, "TestFailureData");
        if (!Directory.Exists(ArtifactsDirectory))
        {
            Directory.CreateDirectory(ArtifactsDirectory);
        }
    }
}