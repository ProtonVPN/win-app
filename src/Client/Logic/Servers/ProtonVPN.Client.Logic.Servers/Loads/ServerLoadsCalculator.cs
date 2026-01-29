/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Loads.Native;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Servers.Loads;

public class ServerLoadsCalculator : IServerLoadsCalculator
{
    private readonly ILogger _logger;

    public ServerLoadsCalculator(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Computes and updates server loads using native binary status processing.
    /// </summary>
    /// <param name="servers">List of servers to compute loads for</param>
    /// <param name="statusFile">Binary status file data from API</param>
    /// <param name="deviceLocation">Optional user device location for geo-optimization</param>
    public bool UpdateServerLoads(IReadOnlyList<Server> servers, byte[] statusFile, DeviceLocation? deviceLocation)
    {
        if (servers is null || servers.Count == 0)
        {
            return false;
        }

        FfiLoad[]? loads = ComputeNative(servers, statusFile, deviceLocation);

        return loads is not null && ApplyServerMetrics(servers, loads);
    }

    private unsafe FfiLoad[]? ComputeNative(IReadOnlyList<Server> servers, byte[] statusFile, DeviceLocation? deviceLocation)
    {
        ReadOnlySpan<FfiLogical> logicals = MapServersToLogicals(servers).AsSpan();
        if (logicals.Length < servers.Count)
        {
            _logger.Error<AppLog>($"Failed to map all servers to logicals. Expected {servers.Count}, but only {logicals.Length} were mapped.");
            return null;
        }

        ReadOnlySpan<byte> statusFileSpan = statusFile.AsSpan();
        FfiLocation location = new()
        {
            Latitude = (float)(deviceLocation?.Latitude ?? 0),
            Longitude = (float)(deviceLocation?.Longitude ?? 0),
        };

        byte[] countryBytes = new byte[2];

        fixed (FfiLogical* logicalsPtr = logicals)
        fixed (byte* statusFilePtr = statusFileSpan)
        fixed (byte* countryBytesPtr = countryBytes)
        {
            SetCountryCode(countryBytesPtr, deviceLocation?.CountryCode);

            FfiLoad[] loads = new FfiLoad[logicals.Length];

            int result = NativeMethods.ComputeLoads(
                logicals,
                (nuint)logicals.Length,
                statusFileSpan,
                (nuint)statusFileSpan.Length,
                in location,
                !string.IsNullOrEmpty(deviceLocation?.CountryCode) ? countryBytesPtr : null,
                loads,
                out ErrorStringHandle error);

            using (error)
            {
                if (result == 0)
                {
                    return loads;
                }
                else
                {
                    _logger.Error<AppLog>($"Failed to calculate server loads: {error}");
                    return null;
                }
            }
        }
    }

    private unsafe static FfiLogical[] MapServersToLogicals(IReadOnlyList<Server> servers)
    {
        FfiLogical[] logicals = new FfiLogical[servers.Count];

        for (int i = 0; i < servers.Count; i++)
        {
            Server server = servers[i];

            // There is a theoretical possibiltiy that servers list is from the legacy logicals endpoint
            // which didn't provide StatusReference, EntryLocation and ExitLocation fields,
            // so we add this guard to not calculate loads in such cases. This guard should be removed
            // after removing IsBinaryServerStatusEnabled feature flag.
            if (server.StatusReference is null || server.EntryLocation is null || server.ExitLocation is null)
            {
                continue;
            }

            logicals[i] = new FfiLogical
            {
                StatusReference = new FfiStatusReference
                {
                    Index = server.StatusReference.Index,
                    Penalty = server.StatusReference.Penalty,
                    Cost = server.StatusReference.Cost
                },
                EntryLocation = new FfiLocation
                {
                    Latitude = (float)server.EntryLocation.Latitude,
                    Longitude = (float)server.EntryLocation.Longitude,
                },
                ExitLocation = new FfiLocation
                {
                    Latitude = (float)server.ExitLocation.Latitude,
                    Longitude = (float)server.ExitLocation.Longitude,
                },
                Features = (uint)server.Features
            };

            fixed (byte* countryPtr = logicals[i].ExitCountry)
            {
                SetCountryCode(countryPtr, server.ExitCountry);
            }
        }

        return logicals;
    }

    private static unsafe void SetCountryCode(byte* countryPtr, string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            countryPtr[0] = 0;
            countryPtr[1] = 0;
            return;
        }

        string normalizedCode = countryCode.ToUpperInvariant();
        countryPtr[0] = normalizedCode.Length > 0 ? (byte)normalizedCode[0] : (byte)0;
        countryPtr[1] = normalizedCode.Length > 1 ? (byte)normalizedCode[1] : (byte)0;
    }

    private bool ApplyServerMetrics(IReadOnlyList<Server> servers, FfiLoad[] loads)
    {
        if (servers.Count != loads.Length)
        {
            _logger.Error<AppLog>($"Server count ({servers.Count}) does not match load count ({loads.Length}). " +
                $"Skipping server load update to maintain data integrity.");
            return false;
        }

        for (int i = 0; i < servers.Count; i++)
        {
            Server server = servers[i];
            FfiLoad load = loads[i];

            server.Load = load.Load;
            server.Score = (float)load.Score;
            server.Status = (sbyte)(load.IsEnabled ? 1 : 0);
            server.IsVisible = load.IsVisible;
            server.IsAutoconnectable = load.IsAutoconnectable;

            // If the logical server only has one physical server, then the status of the logical and physical server are tied
            // If the status for the logical is down, it means that all physical servers for this logical are down
            // If the status for the logical is up, it means that at least one physical server is up, but we can't know which one(s)
            // -> in that case, we need to wait the update servers call to update the status properly
            if (server.Status == 0 || server.Servers.Count <= 1)
            {
                foreach (PhysicalServer physicalServer in server.Servers)
                {
                    physicalServer.Status = server.Status;
                }
            }
        }

        return true;
    }
}