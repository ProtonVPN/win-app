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
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Prometheus.Client;
using Prometheus.Client.MetricPusher;

namespace ProtonVPN.UI.Tests.TestsHelper
{
    public class TestMonitorHelper
    {
        public static readonly Stopwatch Timer = new Stopwatch();
        private readonly static string _endpoint = Environment.GetEnvironmentVariable("PROMETHEUS_HOST");
        private static bool IsMonitoringState { get; set; }

        public static async Task IncrementMetricAsync(string name, string description, double value)
        {
            IGauge gauge = Metrics.DefaultFactory.CreateGauge(name, description);
            MetricPusher pusher = new MetricPusher(new MetricPusherOptions { Endpoint = _endpoint, Job = "vpn-client-performance-metrics" });
            gauge.Inc(value);
            await pusher.PushAsync();
        }

        public static async Task ReportDurationAsync(string name, string description)
        {
            await IncrementMetricAsync(name, description, Timer.Elapsed.TotalSeconds);
        }

        public static async Task ReportTestStatusAsync(string name, string description)
        {
            TestStatus status = TestContext.CurrentContext.Result.Outcome.Status;
            if (status == TestStatus.Passed)
            {
                await IncrementMetricAsync(name, description, 0);
            }
            else if (IsMonitoringState && status == TestStatus.Failed)
            {
                await IncrementMetricAsync(name, description, 1);
            }
        }

        public static void Start()
        {
            Timer.Start();
            IsMonitoringState = true;
        }

        public static void Stop()
        {
            Timer.Stop();
            IsMonitoringState = false;
        }

        public static void Reset()
        {
            Timer.Reset();
            IsMonitoringState = false;
        }
    }
}
