/*
 * Copyright (c) 2024 Proton AG
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.Core;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;

namespace ProtonVPN.Client.Localization.Tests.Extensions;

[TestClass]
public class LocalizationExtensionsTest
{
    private ILocalizationProvider _localizer;

    [TestInitialize]
    public void Initialize()
    {
        _localizer = Substitute.For<ILocalizationProvider>();
        _localizer.GetFormat(Arg.Any<string>(), Arg.Any<object>()).Returns(LocalizerGetFormatSingleNumber);
        _localizer.GetFormat(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<object>()).Returns(LocalizerGetFormatDoubleNumber);
        _localizer.GetPlural(Arg.Any<string>(), Arg.Any<long>()).Returns(LocalizerGetPlural);
        _localizer.GetPluralFormat(Arg.Any<string>(), Arg.Any<long>()).Returns(LocalizerGetPluralFormat);
    }

    private string LocalizerGetFormatSingleNumber(CallInfo args)
    {
        return $"GetFormat {args[0]} {args[1]}";
    }

    private string LocalizerGetFormatDoubleNumber(CallInfo args)
    {
        return $"GetFormat {args[0]} {args[1]} {args[2]}";
    }

    private string LocalizerGetPlural(CallInfo args)
    {
        return $"GetPlural {args[0]} {args[1]} {{0}} {{1}}";
    }

    private string LocalizerGetPluralFormat(CallInfo args)
    {
        return $"GetPluralFormat {args[0]} {args[1]}";
    }

    [TestCleanup]
    public void Cleanup()
    {
        _localizer = null;
    }

    [TestMethod]
    public void TestGetFormattedTime_WhenNegative()
    {
        TimeSpan time = TimeSpan.Zero - TimeSpan.FromSeconds(1);
        string result = LocalizationExtensions.GetFormattedTime(_localizer, time);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void TestGetFormattedTime_WhenZero()
    {
        TimeSpan time = TimeSpan.Zero;
        string result = LocalizationExtensions.GetFormattedTime(_localizer, time);

        Assert.AreEqual("GetFormat Format_Time_Seconds 0", result);
    }

    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(59)]
    [TestMethod]
    public void TestGetFormattedTime_WhenIsUnderAMinute(int seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        string result = LocalizationExtensions.GetFormattedTime(_localizer, time);

        Assert.AreEqual($"GetFormat Format_Time_Seconds {seconds}", result);
    }

    [DataRow(1)]
    [DataRow(2)]
    [DataRow(59)]
    [TestMethod]
    public void TestGetFormattedTime_WhenIsUnderAnHourAndOnlyMinutes(int minutes)
    {
        TimeSpan time = TimeSpan.FromMinutes(minutes);
        string result = LocalizationExtensions.GetFormattedTime(_localizer, time);

        Assert.AreEqual($"GetFormat Format_Time_Minutes {minutes}", result);
    }

    [DataRow(1, 1)]
    [DataRow(1, 2)]
    [DataRow(1, 59)]
    [DataRow(2, 1)]
    [DataRow(2, 2)]
    [DataRow(2, 59)]
    [DataRow(59, 1)]
    [DataRow(59, 2)]
    [DataRow(59, 59)]
    [TestMethod]
    public void TestGetFormattedTime_WhenIsUnderAnHourWithSeconds(int minutes, int seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds((minutes * 60) + seconds);
        string result = LocalizationExtensions.GetFormattedTime(_localizer, time);

        Assert.AreEqual($"GetFormat Format_Time_MinutesSeconds {minutes} {seconds}", result);
    }

    [DataRow(1)]
    [DataRow(2)]
    [DataRow(23)]
    [TestMethod]
    public void TestGetFormattedTime_WhenIsUnderADayAndOnlyHours(int hours)
    {
        TimeSpan time = TimeSpan.FromHours(hours);
        string result = LocalizationExtensions.GetFormattedTime(_localizer, time);

        Assert.AreEqual($"GetFormat Format_Time_Hours {hours}", result);
    }

    [DataRow(1, 1)]
    [DataRow(1, 2)]
    [DataRow(1, 59)]
    [DataRow(2, 1)]
    [DataRow(2, 2)]
    [DataRow(2, 59)]
    [DataRow(23, 1)]
    [DataRow(23, 2)]
    [DataRow(23, 59)]
    [TestMethod]
    public void TestGetFormattedTime_WhenIsUnderADayWithMinutes(int hours, int minutes)
    {
        TimeSpan time = TimeSpan.FromMinutes((hours * 60) + minutes);
        string result = LocalizationExtensions.GetFormattedTime(_localizer, time);

        Assert.AreEqual($"GetFormat Format_Time_HoursMinutes {hours} {minutes}", result);
    }

    [DataRow(1)]
    [DataRow(2)]
    [DataRow(99)]
    [TestMethod]
    public void TestGetFormattedTime_WhenIsDays(int days)
    {
        TimeSpan time = TimeSpan.FromDays(days);
        string result = LocalizationExtensions.GetFormattedTime(_localizer, time);

        Assert.AreEqual($"GetPluralFormat Format_Time_Day {days}", result);
    }

    [DataRow(1, 1)]
    [DataRow(1, 2)]
    [DataRow(1, 23)]
    [DataRow(2, 1)]
    [DataRow(2, 2)]
    [DataRow(2, 23)]
    [DataRow(99, 1)]
    [DataRow(99, 2)]
    [DataRow(99, 23)]
    [TestMethod]
    public void TestGetFormattedTime_WhenIsDaysAndHours(int days, int hours)
    {
        TimeSpan time = TimeSpan.FromHours((days * 24) + hours);
        string result = LocalizationExtensions.GetFormattedTime(_localizer, time);

        Assert.AreEqual($"GetPlural Format_Time_DayHour {days} {days} {hours}", result);
    }
}
