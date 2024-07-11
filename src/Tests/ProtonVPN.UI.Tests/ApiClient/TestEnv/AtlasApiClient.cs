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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProtonVPN.UI.Tests.ApiClient.TestEnv;

public class AtlasApiClient
{
    private readonly HttpClient _client;
    //Yes it's not beautiful... Infra supports only this JSON inside JSON... So it's better to have it as raw string.
    private const string FORCE_CAPTCHA_ENV_VAR = @"{""env"":""FINGERPRINT_RESPONSE=\""{\\\""code\\\"": 2000, \\\""result\\\"": \\\""captcha\\\""}\""""}";

    public AtlasApiClient()
    {
        HttpClientHandler handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("BTI_ATLAS_CONTROLLER_URL"))
        };
    }

    public async Task ForceCaptchaOnLoginAsync()
    {
        await PostAtlasEnvVarAsync(FORCE_CAPTCHA_ENV_VAR);
    }

    public async Task CleanupEnvVarsAsync()
    {
        await PostAtlasEnvVarAsync("");
    }

    public async Task MockApiAsync(string apiMock)
    { 
        string urlEncodedString = WebUtility.UrlEncode(apiMock)
                                        .Replace("+", "%20")
                                        .Replace("%0A", "%0D%0A");

        StringContent content = new($"_method=put&mockConfig={urlEncodedString}");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        HttpResponseMessage response = await _client.PostAsync("/internal/mock", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to mock API: \n {await response.Content.ReadAsStringAsync()}");
        }
    }

    private async Task PostAtlasEnvVarAsync(string query)
    {
        HttpResponseMessage response = await _client.PostAsync("/internal/system", new StringContent(query));

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set Atlas Env Variable: \n {await response.Content.ReadAsStringAsync()}");
        }
    }
}
