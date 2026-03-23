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

using System.Diagnostics;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class WindowsUtils
{
    public static void RunPowerShellScript(string psScript, bool enableLogging = false, string? stringToAssert = null)
    {
        ProcessStartInfo psi = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
            UseShellExecute = !enableLogging,
            RedirectStandardOutput = enableLogging,
            RedirectStandardError = enableLogging,
            CreateNoWindow = true
        };

        using (Process process = new Process())
        {
            process.StartInfo = psi;
            process.Start();

            if (enableLogging)
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

            process.WaitForExit();
        }
    }
}