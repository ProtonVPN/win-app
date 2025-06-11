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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ProtonVPN.UI.Tests.ApiClient.Prod;

public class ProdTestApiClient
{
    public static string AcessToken;
    public static string UID;

    private readonly HttpClient _client;
    private readonly Random _random = new();
    
    public ProdTestApiClient()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri("https://api.protonvpn.ch")
        };
    }

    public async Task<string> GetRandomSpecificPaidServerAsync(string username, SecureString password)
    {
        JToken randomServer = null;
        JArray logicals = await new ProdTestApiClient().GetLogicalServersLoggedInAsync(username, password);
        List<JToken> filteredServers = logicals.Where(
            s => (int)s["Status"] == 1 &&
            (int)s["Tier"] == 2 &&
            !s["Name"].ToString().Contains("SE-") &&
            !s["Name"].ToString().Contains("IS-") &&
            !s["Name"].ToString().Contains("TOR") &&
            !s["Name"].ToString().Contains("CH-")).ToList();
        if (filteredServers.Count > 0)
        {
            randomServer = filteredServers.OrderBy(_ => _random.Next()).FirstOrDefault();
        }
        else
        {
            throw new Exception("Empty server list was returned.");
        }

        return randomServer["Name"].ToString();
    }

    public async Task<JArray> GetLogicalServersLoggedInAsync(string username, SecureString password)
    {
        TestUserAuthenticator _userAuthenticator = new();
        await _userAuthenticator.CreateSessionAsync(username, password);

        HttpRequestMessage request = GetAuthorizedRequestMessage(HttpMethod.Get, "/vpn/logicals?SignServer=Server.EntryIP,Server.Label", AcessToken, UID);
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        JObject logicals = JObject.Parse(await response.Content.ReadAsStringAsync());
        return (JArray)logicals["LogicalServers"];
    }

    public async Task<AuthInfoResponse> GetAuthInfoAsync(AuthInfoRequest username)
    {
        string content = JsonSerializer.Serialize(username);
        HttpResponseMessage response = await SendPostUnauthorizedAsync("/auth/v4/info", content);

        AuthInfoResponse authResponse = JsonSerializer.Deserialize<AuthInfoResponse>(await response.Content.ReadAsStringAsync());
        return authResponse;
    }

    public async Task<AuthResponse> GetAuthResponseAsync(AuthRequest body)
    {
        string content = JsonSerializer.Serialize(body);
        HttpResponseMessage response = await SendPostUnauthorizedAsync("/auth", content);

        AuthResponse authResponse = JsonSerializer.Deserialize<AuthResponse>(await response.Content.ReadAsStringAsync());
        return authResponse;
    }

    private async Task<JArray> GetLogicalServersUnauthorizedAsync()
    {
        HttpResponseMessage response = await _client.GetAsync("/vpn/logicals");
        response.EnsureSuccessStatusCode();
        string responseBody = response.Content.ReadAsStringAsync().Result;
        JObject json = JObject.Parse(responseBody);
        return (JArray)json["LogicalServers"];
    }

    private async Task<HttpResponseMessage> SendPostUnauthorizedAsync(string endpoint, string content)
    {
        HttpRequestMessage request = GetUnauthorizedRequestMessage(HttpMethod.Post, endpoint);

        var jsonContent = new StringContent(
            content,
            Encoding.UTF8,
            "application/json");
        request.Content = jsonContent;

        HttpResponseMessage response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Api call to {_client.BaseAddress}{endpoint} failed. " +
                $"It returned {response.StatusCode}: " +
                $"\n" + await response.Content.ReadAsStringAsync());
        }

        return response;
    }

    private HttpRequestMessage GetUnauthorizedRequestMessage(HttpMethod method, string requestUri)
    {
        HttpRequestMessage request = new(method, requestUri);
        request.Headers.Add("x-pm-apiversion", "3");
        request.Headers.Add("x-pm-appversion", "windows-vpn@2.4.3-dev");
        request.Headers.Add("x-pm-locale", "en");
        request.Headers.Add("User-Agent", "ProtonVPN/2.4.3 (Microsoft Windows NT 10.0.19045.0)");

        return request;
    }

    private HttpRequestMessage GetAuthorizedRequestMessage(HttpMethod method, string requestUri, string accessToken, string uniqueSessionId)
    {
        HttpRequestMessage request = GetUnauthorizedRequestMessage(method, requestUri);
        request.Headers.Add("x-pm-uid", uniqueSessionId);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        return request;
    }
}
