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
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.Dns.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.DnsLogs;

namespace ProtonVPN.Dns.Resolvers;

public abstract class DnsResolverBase
{
    protected ILogger Logger { get; }
    protected TimeSpan DnsResolveTimeout { get; }

    private readonly TimeSpan _defaultDnsTimeToLive;

    protected DnsResolverBase(IConfiguration config, ILogger logger)
    {
        Logger = logger;
        DnsResolveTimeout = config.DnsResolveTimeout;
        _defaultDnsTimeToLive = config.DefaultDnsTimeToLive;
    }

    public async Task<DnsResponse> ResolveAsync(string host, CancellationToken cancellationToken)
    {
        DnsResponse dnsResponse = null;
        if (cancellationToken.IsCancellationRequested)
        {
            Logger.Error<DnsErrorLog>($"DNS resolver called with cancelled token for host '{host}'.");
        }
        else if (string.IsNullOrEmpty(host))
        {
            Logger.Error<DnsErrorLog>($"DNS resolver called for empty host '{host}'.");
        }
        else
        {
            Logger.Info<DnsResolveLog>($"Attempting to resolve host '{host}'.");
            dnsResponse = await StartTasksAndWaitAnySuccessAsync(host, cancellationToken);
        }
        LogResult(host, dnsResponse);
        return dnsResponse;
    }

    protected abstract Task<DnsResponse> StartTasksAndWaitAnySuccessAsync(string host, CancellationToken cancellationToken);

    private void LogResult(string host, DnsResponse dnsResponse)
    {
        if (IsNullOrEmpty(dnsResponse))
        {
            Logger.Error<DnsErrorLog>($"Failed to resolve host '{host}'.");
        }
        else
        {
            Logger.Info<DnsResponseLog>($"Successfully resolved host '{host}'.");
        }
    }

    protected abstract bool IsNullOrEmpty(DnsResponse dnsResponse);

    // This method receives the DNS request tasks as funcs to be able to use with them a cancellation token it can cancel.
    // When there is a task that successfully responds, this method can cancel immediately all other pending tasks.
    // When there is a timeout or an unexpected error, this method can cancel immediately all pending tasks.
    protected async Task<DnsResponse> WaitAnySuccessfulResolveAsync(
        IList<Func<CancellationToken, Task<DnsResponse>>> resolveFuncs, TimeSpan timeout, CancellationToken cancellationToken)
    {
        CancellationTokenSource timeoutCancellationTokenSource = new(timeout);
        CancellationTokenSource childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCancellationTokenSource.Token);
        IList<Task<DnsResponse>> resolveTasks = resolveFuncs
            .Select(resolveTask => resolveTask(childCancellationTokenSource.Token)).ToList();

        return await TryWaitAnySuccessfulResolveAsync(resolveTasks, childCancellationTokenSource);
    }

    private async Task<DnsResponse> TryWaitAnySuccessfulResolveAsync(IList<Task<DnsResponse>> resolveTasks,
        CancellationTokenSource cancellationTokenSource)
    {
        DnsResponse dnsResponse = null;
        while (resolveTasks.Any())
        {
            try
            {
                Task<DnsResponse> completedTask = await Task.WhenAny(resolveTasks);
                resolveTasks.Remove(completedTask);
                dnsResponse = await completedTask;

                if (!IsNullOrEmpty(dnsResponse))
                {
                    break;
                }
                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    LogOperationCancelled("Task cancelled after a DNS resolve task completed without response.");
                    break;
                }
            }
            catch (Exception e)
            {
                if (e.IsOrAnyInnerIsOfExceptionType<OperationCanceledException>())
                {
                    LogOperationCancelled("Task cancelled when waiting for DNS resolve tasks.");
                }
                else
                {
                    Logger.Error<DnsErrorLog>("Unexpected error in DNS resolve task wait.", e);
                }
                break;
            }
        }

        resolveTasks.ForEach(t => t.FireAndForget());
        cancellationTokenSource.Cancel();
        return dnsResponse;
    }

    protected void LogOperationCancelled(string message,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string sourceMemberName = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        Logger.Info<DnsLog>(message,
            sourceFilePath: sourceFilePath,
            sourceMemberName: sourceMemberName,
            sourceLineNumber: sourceLineNumber);
    }

    protected DnsResponse CreateDnsResponseWithIpAddresses(string host, int? timeToLiveInSeconds,
        IList<IPAddress> systemTypeIpAddresses)
    {
        if (systemTypeIpAddresses.IsNullOrEmpty())
        {
            Logger.Error<DnsErrorLog>("Cannot create DNS response entity because no IP addresses were provided.");
            return null;
        }

        List<IpAddress> ipAddresses = systemTypeIpAddresses.Select(ia => new IpAddress(ia)).ToList();

        foreach (IpAddress ipAddress in ipAddresses.ToList())
        {
            if (!ipAddress.IsPublicIp())
            {
                Logger.Warn<DnsResponseLog>($"Resolved {ipAddress.ToString()} is not a public IP address, removing from the list.");

                ipAddresses.Remove(ipAddress);
            }
        }

        if (ipAddresses.Count == 0)
        {
            Logger.Error<DnsErrorLog>("Cannot create DNS response entity because all resolved IP addresses were not public IP addresses.");
            return null;
        }

        DnsResponse dnsResponse = new(host, GetTimeToLiveOrDefault(timeToLiveInSeconds), ipAddresses);
        Logger.Info<DnsResponseLog>($"Created DNS response entity for host '{dnsResponse.Host}' with " +
            $"{dnsResponse.IpAddresses.Count} IP addresses and an expiration date in UTC of " +
            $"{dnsResponse.ExpirationDateTimeUtc} based on a TTL of {dnsResponse.TimeToLive}.");
        Logger.Debug<DnsResponseLog>($"IP addresses: [{string.Join(",", dnsResponse.IpAddresses.Select(ia => ia.ToString()))}].");

        return dnsResponse;
    }

    protected TimeSpan GetTimeToLiveOrDefault(int? timeToLiveInSeconds)
    {
        return timeToLiveInSeconds is null or 0
            ? _defaultDnsTimeToLive.AddJitter(0.25)
            : TimeSpan.FromSeconds(timeToLiveInSeconds.Value);
    }

    protected DnsResponse CreateDnsResponseWithAlternativeHosts(string host, int? timeToLiveInSeconds,
        IList<string> alternativeHosts)
    {
        if (alternativeHosts.IsNullOrEmpty())
        {
            Logger.Error<DnsErrorLog>("Cannot create DNS response entity because no alternative hosts were provided.");
            return null;
        }
        DnsResponse dnsResponse = new(host, GetTimeToLiveOrDefault(timeToLiveInSeconds), alternativeHosts);
        Logger.Info<DnsResponseLog>($"Created DNS response entity for host '{dnsResponse.Host}' with " +
            $"{dnsResponse.AlternativeHosts.Count} alternative hosts and an expiration date in UTC of " +
            $"{dnsResponse.ExpirationDateTimeUtc} based on a TTL of {dnsResponse.TimeToLive}.");
        Logger.Debug<DnsResponseLog>(
            $"Alternative hosts: [{string.Join(",", dnsResponse.AlternativeHosts.Select(ia => ia.ToString()))}].");

        return dnsResponse;
    }
}