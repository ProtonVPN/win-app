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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using ProtonVPN.UI.Tests.TestsHelper;
using System.Linq;
using FlaUI.Core.Tools;

namespace ProtonVPN.UI.Tests.ApiClient.TestEnv;

public class LokiPusher
{
    private readonly string _lokiPushEndpoint = Environment.GetEnvironmentVariable("LOKI_ENDPOINT") + "loki/api/v1/push";
    private HttpClient _httpClient;

    public LokiPusher()
    {
        _httpClient = new LokiApiClient().GetHttpClient();
    }

    public void PushMetrics()
    {
        if (SliHelper.MetricsList.Count == 0)
        {
            throw new Exception("Pushing empty metric list is not allowed.");
        }

        try
        {
            JArray fullMetrics = BaseMetricsJsonBody(GetMetadata(SliHelper.RunId), SliHelper.MetricsList);
            JObject requestBody = BaseLokiRequestJsonBody(fullMetrics, GetMetricsLabels(SliHelper.SliName, SliHelper.Workflow));
            PushToLokiWithRetry(requestBody);
        }
        catch (Exception ex)
        {
            SliHelper.Reset();
            throw;
        }
    }

    public void PushLogs(string logsPath, string logType)
    {
        JObject requestBody = AddLogsToRequestJson(logsPath, logType, SliHelper.Workflow, GetMetadata(SliHelper.RunId));
        PushToLokiWithRetry(requestBody);
    }

    public void PushAllLogs()
    {
        PushLogs(TestConstants.ClientLogsPath, "windows_client_logs");
        PushLogs(TestEnvironment.GetServiceLogsPath(), "windows_service_logs");
    }

    private JObject AddLogsToRequestJson(string pathToLogs, string logType, string workflow, JObject metadata)
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
        return BaseLokiRequestJsonBody(logs, GetLogsLabels(workflow, logType));
    }

    private void PushToLokiWithRetry(JObject requestBody)
    {
        RetryResult<string> retry = Retry.WhileNull(
            () =>
            {
                return PushToLokiAsync(requestBody).Result;
            },
            TestConstants.TenSecondsTimeout, TestConstants.ApiRetryInterval, ignoreException: true);

        if (!retry.Success)
        {
            throw new Exception($"Failed to push to loki:\n{retry.LastException}");
        }
    }

    private async Task<string> PushToLokiAsync(JObject requestBody)
    {
        string jsonContent = JsonConvert.SerializeObject(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync(_lokiPushEndpoint, httpContent);
        string responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseBody);
        }
        response.EnsureSuccessStatusCode();
        return responseBody;
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

    private JArray BaseMetricsJsonBody(JObject metadata, object metric)
    {
        return new JArray(
            GetCurrentUnixTimeInNanoseconds().ToString(),
            new JObject(metric).ToString(),
            metadata);
    }


    private JObject BaseLokiRequestJsonBody(object data, JObject labels)
    {
        return new JObject(
            new JProperty("streams", new JArray(
                new JObject(
                    new JProperty("stream", labels),
                    new JProperty("values", new JArray(data))
            ))
        ));
    }
    private JObject GetMetadata(string runId)
    {
        return new JObject(
            new List<JProperty>
            {
                new JProperty("id", runId),
                new JProperty("app_version", TestEnvironment.GetAppVersion()),
                new JProperty("build_commit_sha1", TestEnvironment.GetCommitHash())
            });
    }

    private JObject GetMetricsLabels(string measurementGroup, string workflow)
    {
        JObject labels = GetCommonLabels(workflow);
        labels.Add(new JProperty("sli", measurementGroup));
        return labels;
    }

    private JObject GetLogsLabels(string workflow, string logType)
    {
        JObject labels = GetCommonLabels(workflow);
        labels.Add(new JProperty("logType", logType));
        return labels;
    }

    private JObject GetCommonLabels(string workflow)
    {
        return new JObject
            (
                new JProperty("os_version", TestEnvironment.GetOperatingSystem()),
                new JProperty("workflow", workflow),
                new JProperty("environment", "prod"),
                new JProperty("platform", "windows"),
                new JProperty("product", "VPN")
            );
    }

    private long GetCurrentUnixTimeInNanoseconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000;
    }
}
