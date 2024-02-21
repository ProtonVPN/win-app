﻿/*
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
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Threading.Tasks;
using FlaUI.Core.Tools;
using NUnit.Framework;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class CommonAssertions
{
    public static void AssertDnsIsResolved(string url)
    {
        RetryResult<bool> retry = Retry.WhileFalse(() =>
        {
            return TryToResolveDns(url);
        },
        TestConstants.ShortTimeout, TestConstants.RetryInterval);

        Assert.IsTrue(retry.Result, $"Dns was not resolved for {url}.");
    }

    public static void AssertDnsIsNotResolved(string url)
    {
        RetryResult<bool> retry = Retry.WhileTrue(() =>
        {
            return TryToResolveDns(url);
        },
        TestConstants.ShortTimeout, TestConstants.RetryInterval);

        Assert.IsTrue(retry.Result, $"DNS was resolved for {url}");
    }

    public static void AssertIpAddressChanged(string previousIpAddress)
    {
        RetryResult<bool> retry = Retry.WhileTrue(() =>
        {
            string currentIpAddress = NetworkUtils.GetIpAddress();
            return currentIpAddress == previousIpAddress;
        },
        TestConstants.ShortTimeout, TestConstants.RetryInterval);

        Assert.IsTrue(retry.Result, $"IP Address has not changed from {previousIpAddress}");
    }

    public static void AssertIpAddressUnchanged(string previousIpAddress)
    {
        RetryResult<bool> retry = Retry.WhileFalse(() =>
        {
            string currentIpAddress = NetworkUtils.GetIpAddress();
            return currentIpAddress == previousIpAddress;
        },
        TestConstants.ShortTimeout, TestConstants.RetryInterval);

        Assert.IsTrue(retry.Result, $"IP Address has changed from {previousIpAddress}");
    }

    private static bool TryToResolveDns(string url)
    {
        try
        {
            Dns.GetHostEntry(url);

            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}