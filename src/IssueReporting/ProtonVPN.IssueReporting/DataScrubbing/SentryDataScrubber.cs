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
using System.Text.RegularExpressions;

namespace ProtonVPN.IssueReporting.DataScrubbing;

public class SentryDataScrubber
{
    private const string REDACTED_EMAIL = "[REDACTED_EMAIL]";
    private const string REDACTED_USERNAME = "[REDACTED_USER]";

    private static readonly Regex _emailAddressPattern = new(
        @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex _userProfilePathPattern = new(
        @"(?i)(?<drive>[A-Za-z]):\\Users\\[^\\]+(?<appdata>\\AppData\\Local)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex _envVarPathPattern = new(
        @"(?i)(?<envvar>%USERPROFILE%|~)\\(?<username>[^\\]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string? Scrub(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        try
        {
            string scrubbedText = input;

            scrubbedText = _emailAddressPattern.Replace(scrubbedText, REDACTED_EMAIL);
            
            scrubbedText = _userProfilePathPattern.Replace(scrubbedText, m =>
            {
                string drive = m.Groups["drive"].Value;
                string appData = m.Groups["appdata"].Success ? @"\AppData\Local" : "";
                return $@"{drive}:\Users\{REDACTED_USERNAME}{appData}";
            });
            
            scrubbedText = _envVarPathPattern.Replace(scrubbedText, m =>
            {
                string envVar = m.Groups["envvar"].Value;
                return $@"{envVar}\{REDACTED_USERNAME}";
            });

            return scrubbedText;
        }
        catch (Exception)
        {
            return input;
        }
    }
}