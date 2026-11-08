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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FlaUI.Core.Tools;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace ProtonVPN.UI.Tests.TestsHelper;

public class SliHelper
{
    public static string? SliName { get; set; }
    public static string? Workflow { get; set; }
    public static string? RunId { get; set; }
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
        if (Duration != 0)
        {
            AddMetric("duration", Duration.ToString());
        }
    }

    public static void StartCustomRun(string sliName)
    {
        RunId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        SliName = sliName;
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
        KillLingeringSpeedtestProcesses();
        DnsHelper.FlushDns();
        Dictionary<string, double>? networkSpeedConnected = null;
        RetryResult<bool> retry = Retry.WhileException(
            () =>
            {
                networkSpeedConnected = GetNetworkSpeed(TestConstants.TwoMinutesTimeout);
            },
            TestConstants.FourMinutesTimeout, TestConstants.TenSecondsTimeout);

        if (!retry.Success)
        {
            KillLingeringSpeedtestProcesses();
            throw new Exception($"Could not measure network speed: {retry.LastException?.Message}", retry.LastException);
        }

        AddMetric(downloadSpeedLabel, networkSpeedConnected?["downloadSpeed"].ToString() ?? string.Empty);
        AddMetric(uploadSpeedLabel, networkSpeedConnected?["uploadSpeed"].ToString() ?? string.Empty);
    }

    private static void KillLingeringSpeedtestProcesses()
    {
        foreach (Process process in Process.GetProcessesByName("speedtest"))
        {
            try
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(TestConstants.FiveSecondsTimeout);
            }
            catch { }
            finally
            {
                process.Dispose();
            }
        }
    }

    public static Dictionary<string, double> GetNetworkSpeed(TimeSpan? timeout = null)
    {
        TimeSpan effectiveTimeout = timeout ?? TestConstants.ThirtySecondsTimeout;

        Dictionary<string, double> networkStats = new();

        ProcessStartInfo startInfo = new()
        {
            FileName = "speedtest.exe",
            Arguments = "--accept-gdpr --accept-license --progress=no -f json",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new();
        process.StartInfo = startInfo;
        process.Start();

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();

        bool exited = process.WaitForExit(effectiveTimeout);

        if (!exited)
        {
            TestContext.WriteLine($"speedtest.exe did not exit within {effectiveTimeout.TotalSeconds}s. Killing the process tree.");
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch { }
            process.WaitForExit(TestConstants.FiveSecondsTimeout);
        }

        string output = outputTask.Wait(TestConstants.FiveSecondsTimeout) ? outputTask.Result : string.Empty;
        string error = errorTask.Wait(TestConstants.FiveSecondsTimeout) ? errorTask.Result : string.Empty;

        JObject result = JObject.Parse(output);
        JToken? downloadToken = result["download"]?["bandwidth"];
        JToken? uploadToken = result["upload"]?["bandwidth"];

        if (downloadToken is null || uploadToken is null)
        {
            throw new InvalidOperationException(
                $"Speedtest output missing required bandwidth fields (exit code {process.ExitCode}).\n" +
                $"OUTPUT: {output}\nERROR: {error}");
        }

        double downloadSpeedInMbps = ConvertBytesToMbps((double)downloadToken);
        double uploadSpeedInMbps = ConvertBytesToMbps((double)uploadToken);

        networkStats.Add("downloadSpeed", downloadSpeedInMbps);
        networkStats.Add("uploadSpeed", uploadSpeedInMbps);

        return networkStats;
    }

    private static double ConvertBytesToMbps(double valueInBytes)
    {
        return (valueInBytes * 8) / 1000000;
    }
}