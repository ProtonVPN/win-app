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

    public static void AssertLogFile(string filePath, string lineToLookFor, string? wordToLookFor = null)
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
            Assert.That(lastLine, Is.Not.Null, $"No line containing '{lineToLookFor}' found in {filePath}");
            Assert.That(lastLine, Does.Contain(wordToLookFor ?? lineToLookFor));
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
            UseShellExecute = !shouldEnableLogging,
            RedirectStandardOutput = shouldEnableLogging,
            RedirectStandardError = shouldEnableLogging,
            CreateNoWindow = true
        };

        using (Process process = new())
        {
            process.StartInfo = psi;
            process.Start();

            if (shouldEnableLogging)
            {
                string psOutput = process.StandardOutput.ReadToEnd();
                string psError = process.StandardError.ReadToEnd();
                TestContext.WriteLine($"PS OUTPUT: {psOutput}");
                TestContext.WriteLine($"PS ERROR: {psError}");

                if (!string.IsNullOrEmpty(stringToAssert))
                {
                    Assert.That(psOutput, Does.Contain(stringToAssert));
                }
            }

            process.WaitForExit(TestConstants.TenSecondsTimeout);
        }
    }

    public static void AssertVpnRunsOnStartup(bool shouldRunOnStartup)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH);
        Assert.That(key, Is.Not.Null);
        string? value = key!.GetValue(REGISTRY_NAME) as string;
        Assert.That(value, shouldRunOnStartup  ? Is.EqualTo(REGISTRY_DATA) : Is.Null);
    }
}