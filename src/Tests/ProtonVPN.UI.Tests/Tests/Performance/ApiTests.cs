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
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ProtonVPN.UI.Tests.ApiClient;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.Performance;

[TestFixture]
[Category("Performance")]
public class ApiTests
{
    private TestUserAuthenticator _userAuthenticator = new();
    private ProdTestApiClient _prodTestApiClient = new();
    private const string WORKFLOW = "api_measurements";
    private LokiApiClient _lokiApiClient = new();
    private string _measurementGroup;
    private string _runId;

    [OneTimeSetUp]
    public void TestInitialize()
    {
        _runId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    [Test]
    public async Task LogicalsStatsPlus()
    {
        _measurementGroup = "paid_servers_stats";

        SecureString password = new NetworkCredential("", TestUserData.PlusUser.Password).SecurePassword;
        await PushServerMaintenanceStatsAsync(TestUserData.PlusUser.Username, password, 2);
    }

    [Test]
    public async Task LogicalsStatsFree()
    {
        _measurementGroup = "free_servers_stats";

        SecureString password = new NetworkCredential("", TestUserData.FreeUser.Password).SecurePassword;
        await PushServerMaintenanceStatsAsync(TestUserData.FreeUser.Username, password, 0);
    }

    [TearDown]
    public async Task TestCleanup()
    {
        await _lokiApiClient.PushCollectedMetricsAsync(PerformanceTestHelper.MetricsList, _runId, _measurementGroup, WORKFLOW);
        PerformanceTestHelper.Reset();
    }

    private async Task PushServerMaintenanceStatsAsync(string username, SecureString password, int serverTier)
    {
        int totalIndividualServers = 0;
        int onlineIndividualServers = 0;

        await _userAuthenticator.CreateSessionAsync(username, password);

        JArray logicalServers = await _prodTestApiClient.GetLogicalServersLoggedInAsync();
        JArray totalServersByTier = new JArray(logicalServers.Where(server => (int)server["Tier"] == serverTier));

        foreach (JObject server in totalServersByTier)
        {
            JArray serversArray = (JArray)server["Servers"];
            totalIndividualServers += serversArray.Count;
            onlineIndividualServers += serversArray.Count(s => (int)s["Status"] == 1);
        }
        Console.WriteLine(totalIndividualServers.ToString());
        Console.WriteLine(onlineIndividualServers.ToString());
        PerformanceTestHelper.AddMetric("total_servers", totalIndividualServers.ToString());
        PerformanceTestHelper.AddMetric("online_servers", onlineIndividualServers.ToString());
    }
}
