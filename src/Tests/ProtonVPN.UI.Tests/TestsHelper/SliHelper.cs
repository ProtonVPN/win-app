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

using FlaUI.Core.Tools;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProtonVPN.UI.Tests.TestsHelper;
public class SliHelper
{
    public static string SliName { get; set; }
    public static string Workflow { get; set; }
    public static string RunId { get; set; }
    public static readonly Stopwatch Timer = new();
    public static List<JProperty> MetricsList = [];
    private static bool IsMonitoring { get; set; }
    private static double Duration => Timer.Elapsed.TotalSeconds;

    public static void MeasureTime(Action method)
    {
        Timer.Start();
        IsMonitoring = true;
        method();
        Timer.Stop();
        IsMonitoring = false;
    }

    public static void MeasureTestStatus(Action method)
    {
        IsMonitoring = true;
        method();
        IsMonitoring = false;
    }

    public static void Reset()
    {
        Timer.Reset();
        MetricsList.Clear();
    }

    public static void AddMetric(string metricName, string value)
    {
        MetricsList.Add(new JProperty(metricName, value));
    }

    public static void AddDuration()
    {
        if(Duration != 0)
        {
            AddMetric("duration", Duration.ToString());
        }
    }

    public static void AddTestStatusMetric()
    {
        TestStatus status = TestContext.CurrentContext.Result.Outcome.Status;
        if (status == TestStatus.Passed)
        {
            AddMetric("status", "passed");
        }
        else if (IsMonitoring && status == TestStatus.Failed)
        {
            AddMetric("status", "failed");
        }
    }

    public static void AddNetworkSpeedToMetrics(string downloadSpeedLabel, string uploadSpeedLabel)
    {
        NetworkUtils.FlushDns();
        Dictionary<string, double> networkSpeedConnected = null;
        RetryResult<bool> retry = Retry.WhileException(
            () =>
            {
                networkSpeedConnected = GetNetworkSpeed();
            },
            TestConstants.ThirtySecondsTimeout, TestConstants.ApiRetryInterval);

        if (!retry.Success)
        {
            throw new Exception(retry.LastException.Message);
        }
        
        AddMetric(downloadSpeedLabel, networkSpeedConnected["downloadSpeed"].ToString());
        AddMetric(uploadSpeedLabel, networkSpeedConnected["uploadSpeed"].ToString());
    }

    public static Dictionary<string, double> GetNetworkSpeed()
    {
        Dictionary<string, double> networkStats = new();
        string speedTestPath = "speedtest.exe";

        // Arguments for speedtest.exe to output in JSON format
        string arguments = "--accept-gdpr --accept-license -f json ";

        // Start the process
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = speedTestPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                JObject result = JObject.Parse(output);

                double downloadSpeedInBytes = (double)result["download"]["bandwidth"];
                double downloadSpeedInMbps = ConvertBytesToMbps(downloadSpeedInBytes);
                double uploadSpeedInBytes = (double)result["upload"]["bandwidth"];
                double uploadSpeedInMbps = ConvertBytesToMbps(uploadSpeedInBytes);

                networkStats.Add("downloadSpeed", downloadSpeedInMbps);
                networkStats.Add("uploadSpeed", uploadSpeedInMbps);
            }
            else
            {
                Console.WriteLine("Failed to start speedtest.exe process.");
            }
        }
        return networkStats;
    }

    private static double ConvertBytesToMbps(double valueInBytes)
    {
        return (valueInBytes * 8) / 1000000;
    }
}