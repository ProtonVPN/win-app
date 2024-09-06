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
using FlaUI.Core.Tools;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.ApiClient.TestEnv;

public class BtiController
{
    private static HttpClient _client;

    static BtiController()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("BTI_CONTROLLER_URL"))
        };
    }

    public static void SetScenario(string scenarioEndpoint)
    {
        RetryResult<bool> retry = Retry.WhileException(
            () => {
                MakeScenarioRequest(scenarioEndpoint);
            },
            TestConstants.TenSecondsTimeout, TestConstants.ApiRetryInterval);

        if (!retry.Success)
        {
            throw new Exception($"Failed to set scenario:\n${retry.LastException}");
        }
    }

    private static void MakeScenarioRequest(string scenarioEndpoint)
    {
        HttpResponseMessage response = _client.GetAsync(scenarioEndpoint).Result;
        response.EnsureSuccessStatusCode();
    }
}
