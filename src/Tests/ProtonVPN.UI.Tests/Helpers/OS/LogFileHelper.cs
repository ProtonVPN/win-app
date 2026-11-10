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
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class LogFileHelper
{
    public static void AssertLogFile(string filePath, string lineToLookFor, string? wordToLookFor = null)
    {
        string? lastLine = GetLastLogLine(filePath, lineToLookFor);
        Assert.That(lastLine, Is.Not.Null, $"No line containing '{lineToLookFor}' found in {filePath}");
        Assert.That(lastLine, Does.Contain(wordToLookFor ?? lineToLookFor));
    }

    public static int CountLogLines(string filePath, string lineToLookFor)
    {
        return ReadLinesFromCopy(filePath).Count(l => l.Contains(lineToLookFor));
    }

    public static string? GetLastLogLine(string filePath, string lineToLookFor)
    {
        return ReadLinesFromCopy(filePath).LastOrDefault(l => l.Contains(lineToLookFor));
    }

    private static string[] ReadLinesFromCopy(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found at path: {filePath}");
        }

        string tempFile = Path.GetTempFileName();
        File.Copy(filePath, tempFile, true);

        try
        {
            return File.ReadAllLines(tempFile);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
