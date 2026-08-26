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
using System.Diagnostics;
using Microsoft.Win32;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class WindowsUtils
{
    private const string REGISTRY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string REGISTRY_NAME = "Proton VPN";
    private const string REGISTRY_DATA = @"C:\Program Files\Proton\VPN\ProtonVPN.Launcher.exe ""----ms-protocol:ms-encodedlaunch:App?ContractId=Windows.StartupTask&TaskId=Proton%20VPN""";

    private const string ORIGINAL_CHROME_FOLDER = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
    private const string RENAMED_CHROME_FOLDER = @"C:\Program Files\Google\Chrome\Application\chrome_disabled.exe";

    public static void AssertLogFile(string filePath, string lineToLookFor, string? wordToLookFor = null)
    {
        string? lastLine = GetLastLogLine(filePath, lineToLookFor, wordToLookFor);
        Assert.That(lastLine, Is.Not.Null, $"No line containing '{lineToLookFor}' found in {filePath}");
        Assert.That(lastLine, Does.Contain(wordToLookFor ?? lineToLookFor));
    }

    public static string? GetLastLogLine(string filePath, string lineToLookFor, string? wordToLookFor = null)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found at path: {filePath}");
        }

        string tempFile = Path.GetTempFileName();
        File.Copy(filePath, tempFile, true);

        try
        {
            string[] allLines = File.ReadAllLines(tempFile);
            string? lastLine = allLines.Reverse().FirstOrDefault(l => l.Contains(lineToLookFor));
            return lastLine;
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    public static void RunPowerShellScript(string psScript, bool shouldEnableLogging = false, string? stringToAssert = null)
    {
        ProcessStartInfo psi = new()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using Process process = new();
        process.StartInfo = psi;
        process.Start();

        string psOutput = process.StandardOutput.ReadToEnd();
        string psError = process.StandardError.ReadToEnd();

        if (shouldEnableLogging)
        {
            TestContext.WriteLine($"PS OUTPUT: {psOutput}");
            TestContext.WriteLine($"PS ERROR: {psError}");
        }

        if (!string.IsNullOrEmpty(stringToAssert))
        {
            Assert.That(psOutput, Does.Contain(stringToAssert));
        }

        bool exited = process.WaitForExit(TestConstants.ThirtySecondsTimeout);
        if (!exited)
        {
            TestContext.WriteLine($"PowerShell script '{psScript}' did not exit within the timeout. Exiting by force");
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch { }
            process.WaitForExit(TestConstants.ThirtySecondsTimeout);

            throw new TimeoutException($"PowerShell script did not complete within {TestConstants.ThirtySecondsTimeout}s and was force-killed.\n" +
                $"Script: {psScript}\nPS OUTPUT: {psOutput}\nPS ERROR: {psError}");
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"PowerShell script exited with code {process.ExitCode}.\n" +
                $"Script: {psScript}\nPS OUTPUT: {psOutput}\nPS ERROR: {psError}");
        }
    }

    public static void AssertVpnRunsOnStartup(bool shouldRunOnStartup)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH);
        Assert.That(key, Is.Not.Null);
        string? value = key!.GetValue(REGISTRY_NAME) as string;
        Assert.That(value, shouldRunOnStartup ? Is.EqualTo(REGISTRY_DATA) : Is.Null);
    }

    public static void RenameChrome()
    {
        if (File.Exists(ORIGINAL_CHROME_FOLDER))
        {
            File.Move(ORIGINAL_CHROME_FOLDER, RENAMED_CHROME_FOLDER);
        }
    }

    public static void RestoreChrome()
    {
        if (File.Exists(RENAMED_CHROME_FOLDER))
        {
            File.Move(RENAMED_CHROME_FOLDER, ORIGINAL_CHROME_FOLDER);
        }
    }
}