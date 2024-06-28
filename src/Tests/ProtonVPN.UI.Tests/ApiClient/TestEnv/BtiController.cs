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
using System.Net.Http;
using System.Threading.Tasks;
using FlaUI.Core.Tools;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.ApiClient.TestEnv;

public class BtiController
{
    private static readonly HttpClient _client = new HttpClient
    {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("BTI_CONTROLLER_URL"))
    };

    public static void SetScenarioAsync(string scenarioEndpoint)
    {
        RetryResult<bool> retry = Retry.WhileFalse(
            () => {
                return SendScenario(scenarioEndpoint).Result;
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.RetryInterval, ignoreException: true);
    }

    private static async Task<bool> SendScenario(string scenarioEndpoint)
    {
        try
        {
            HttpResponseMessage response = await _client.GetAsync(scenarioEndpoint);
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
