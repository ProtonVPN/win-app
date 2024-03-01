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

using System.Net.Http;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.ApiClient;
public class LokiApiClient
{
    private readonly string _lokiPushEndpoint = Environment.GetEnvironmentVariable("LOKI_ENDPOINT");
    private HttpClient _httpClient = new HttpClient();

    private JObject GetMetadata(string runId)
    {
        return new JObject(
            new List<JProperty>
            {
                new JProperty("id", runId),
                new JProperty("platform", "Windows"),
                new JProperty("environment", "prod"),
                new JProperty("appVersion", TestEnvironment.GetAppVersion())
            });
    }

    public async Task PushCollectedMetricsAsync(List<JProperty> metrics, string runId, string labelValue)
    {
        JArray fullMetrics = BaseMetricsJsonBody(GetMetadata(runId), metrics);
        JObject requestBody = BaseLokiRequestJsonBody(fullMetrics, labelValue);
        await PostMetricsAsync(requestBody);
    }

    public async Task PushLogsAsync(string logsPath, string runId, string lokiLabel)
    {
        JObject requestBody = AddLogsToRequestJson(logsPath, lokiLabel, GetMetadata(runId));
        await PostMetricsAsync(requestBody);
    }

    private JObject AddLogsToRequestJson(string pathToLogs, string lokiLabel, JObject metadata)
    {
        List<JArray> logs = new List<JArray>();
        FileStream fileStream = new FileStream(pathToLogs, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using (StreamReader sr = new StreamReader(fileStream))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                List<string> logLines = new List<string>(line.Split(" | "));
                string timestamp = ConvertTimeToUnixNanosecond(logLines.First());
                logLines.RemoveAt(0);
                string formattedLogs = string.Join(" | ", logLines);
                JArray element = new JArray(timestamp, formattedLogs, metadata);
                logs.Add(element);
            }
        }
        return BaseLokiRequestJsonBody(logs, lokiLabel);
    }

    private async Task PostMetricsAsync(JObject requestBody)
    {
        string jsonContent = JsonConvert.SerializeObject(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(_lokiPushEndpoint, httpContent);

        response.EnsureSuccessStatusCode();
    }

    private string ConvertTimeToUnixNanosecond(string timestampString)
    {
        string timestamp = null;
        try
        {
            DateTime logTimeStamp = DateTime.Parse(timestampString, null, System.Globalization.DateTimeStyles.RoundtripKind);
            long unixTimestampNanoSeconds = (long)(logTimeStamp.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds * 1000000;
            timestamp = unixTimestampNanoSeconds.ToString();
        }
        catch (FormatException)
        {
            timestamp = GetCurrentUnixTimeInNanoseconds().ToString();
        }
        return timestamp;
    }

    private JObject BaseLokiRequestJsonBody(object data, string labelValue)
    {
        return new JObject(
            new JProperty("streams", new JArray(
                new JObject(
                    new JProperty("stream", new JObject(
                        new JProperty("sli", labelValue)
                    )),
                    new JProperty("values", new JArray(
                        data
                ))
            ))
        ));
    }

    private JArray BaseMetricsJsonBody(JObject metadata, object metric)
    {
        return new JArray(
            GetCurrentUnixTimeInNanoseconds().ToString(),
            new JObject(metric).ToString(),
            metadata);
    }

    private long GetCurrentUnixTimeInNanoseconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000;
    }
}