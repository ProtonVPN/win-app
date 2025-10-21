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
using Microsoft.Win32;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("client-resource-metrics")]
public class ClientPropertiesSLIs : BaseTest
{
    private LokiPusher _lokiPusher = new();
    private const string CLIENT_NAME = "Proton VPN";
    private const string REGISTRY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    [Test]
    [Sli("client_size")]
    public void ClientSizeSliMeasurement()
    {
        SliHelper.AddMetric("client_size_megabytes", GetClientSizeMegabytes().ToString());
    }

    [Test]
    [Sli("client_fresh_launch_time")]
    [Duration]
    public void ClientFreshLaunchTimeSli()
    {
        LaunchApp();

        SliHelper.MeasureTime(() =>
        {
            LoginRobot.Verify.IsLoginWindowDisplayed();
        });

        Cleanup();
    }

    [TearDown]
    public void TestCleanup()
    {
        _lokiPusher.PushMetrics();
        SliHelper.Reset();
    }

    private double GetClientSizeMegabytes()
    {
        using RegistryKey key = Registry.LocalMachine.OpenSubKey(REGISTRY_PATH);

        foreach (string subKeyName in key.GetSubKeyNames())
        {
            using RegistryKey subKey = key.OpenSubKey(subKeyName);
            if (subKey?.GetValue("DisplayName")?.ToString().Contains(CLIENT_NAME) == true)
            {
                object sizeInKilobytes = subKey.GetValue("EstimatedSize");
                if (sizeInKilobytes != null)
                {
                    return Convert.ToInt64(sizeInKilobytes) / 1024.0;
                }
            }
        }
        return 0;
    }
}
