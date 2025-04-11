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

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Annotations;
using ProtonVPN.UI.Tests.ApiClient.Prod;
using ProtonVPN.UI.Tests.ApiClient.TestEnv;
using ProtonVPN.UI.Tests.TestsHelper;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;

namespace ProtonVPN.UI.Tests.Tests.SliTests;

[TestFixture]
[Category("SLI")]
[Workflow("api_measurements")]
public class ApiSli
{
    private ProdTestApiClient _prodTestApiClient = new();
    private LokiPusher _lokiPusher = new();

    [Test]
    [Sli("paid_servers_stats")]
    public async Task PushLogicalMetricsPlusUser()
    {
        SecureString password = new NetworkCredential("", TestUserData.PlusUser.Password).SecurePassword;
        await PushLogicalStats(TestUserData.PlusUser.Username, password, 2);
    }

    [Test]
    [Sli("free_servers_stats")]
    public async Task PushLogicalsStatsFreeUser()
    {
        SecureString password = new NetworkCredential("", TestUserData.FreeUser.Password).SecurePassword;
        await PushLogicalStats(TestUserData.FreeUser.Username, password, 0);
    }

    [TearDown]
    public void TestCleanup()
    {
        _lokiPusher.PushMetrics();
        SliHelper.Reset();
    }

    private async Task PushLogicalStats(string username, SecureString password, int serverTier)
    {
        int totalIndividualServers = 0;
        int onlineIndividualServers = 0;

        JArray logicalServers = await _prodTestApiClient.GetLogicalServersLoggedInAsync(username, password);
        JArray totalServersByTier = new JArray(logicalServers.Where(server => (int)server["Tier"] == serverTier));

        foreach (JObject server in totalServersByTier)
        {
            JArray serversArray = (JArray)server["Servers"];
            totalIndividualServers += serversArray.Count;
            onlineIndividualServers += serversArray.Count(s => (int)s["Status"] == 1);
        }
        SliHelper.AddMetric("total_servers", totalIndividualServers.ToString());
        SliHelper.AddMetric("online_servers", onlineIndividualServers.ToString());
    }
}
