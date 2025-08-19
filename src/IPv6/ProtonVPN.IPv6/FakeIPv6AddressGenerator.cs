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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.IPv6.Contracts;
using ProtonVPN.IPv6.Interop;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.IPv6Logs;

namespace ProtonVPN.IPv6;

public class FakeIPv6AddressGenerator : IFakeIPv6AddressGenerator
{
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public FakeIPv6AddressGenerator(ILogger logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<List<NetworkAddress>> GenerateAddressesAsync(
        List<string> prefixes,
        List<string> realIpv6Addresses,
        ushort addressesToGenerate)
    {
        byte[]? prefixTree = await GetPrefixTreeAsync();
        if (prefixTree == null)
        {
            return [];
        }

        string? persistedData = await GetPersistedDataAsync();

        try
        {
            NativeResult result = (NativeResult)NativeMethods.CreateInstance(
                [.. prefixes],
                (nuint)prefixes.Count,
                prefixTree,
                (nuint)prefixTree.Length,
                out Ipv6ChaosAlgoHandle handle);

            using (handle)
            {
                if (result != NativeResult.Success)
                {
                    _logger.Error<IPv6Log>("Failed to initialize IPv6 chaos instance.");

                    return [];
                }

                List<NetworkAddress> addresses = GenerateAddressesInner(handle, persistedData, realIpv6Addresses, addressesToGenerate);

                await PersistUpdatedDataAsync(handle, _config.IPv6PersistedDataFilePath);

                _logger.Info<IPv6Log>($"Generated {addresses.Count} fake IPv6 addresses.");

                return addresses;
            }
        }
        catch (Exception e)
        {
            _logger.Error<IPv6Log>("Failed to generate fake IPv6 addresses list.", e);
            return [];
        }
    }

    private async Task<byte[]?> GetPrefixTreeAsync()
    {
        if (!File.Exists(_config.IPv6PrefixTreeFilePath))
        {
            _logger.Error<IPv6Log>($"File {_config.IPv6PrefixTreeFilePath} does not exist.");
            return null;
        }

        try
        {
            return await File.ReadAllBytesAsync(_config.IPv6PrefixTreeFilePath);
        }
        catch (Exception e)
        {
            _logger.Error<IPv6Log>("Failed to read IPv6 prefix tree file.", e);
            return null;
        }
    }

    private List<NetworkAddress> GenerateAddressesInner(
        Ipv6ChaosAlgoHandle handle,
        string? persistedData,
        List<string> realIpv6Addresses,
        ushort addressesToGenerate)
    {
        if (persistedData != null)
        {
            NativeResult persistResult = (NativeResult)NativeMethods.PersistLoad(handle, persistedData);
            if (persistResult != NativeResult.Success)
            {
                _logger.Error<IPv6Log>($"Failed to load persisted data. Error code: {persistResult}. " +
                    "Since this operation is not required the code will proceed.");
            }
        }

        NativeResult result = (NativeResult)NativeMethods.Generate(
            handle,
            [.. realIpv6Addresses],
            (nuint)realIpv6Addresses.Count,
            addressesToGenerate,
            out FFISliceHandle sliceHandle);

        using (sliceHandle)
        {
            if (result != NativeResult.Success || sliceHandle.IsInvalid)
            {
                _logger.Error<IPv6Log>($"Failed to generate fake IPv6 addresses. Error code: {result}");
                return [];
            }

            FFISlice slice = Marshal.PtrToStructure<FFISlice>(sliceHandle.DangerousGetHandle());

            if (addressesToGenerate > 0 && slice.Len == 0)
            {
                _logger.Error<IPv6Log>($"Failed to generate fake IPv6 addresses. " +
                    $"Expected to get {addressesToGenerate}, but instead got {slice.Len}.");
                return [];
            }

            return GetFakeIpv6Addresses(slice);
        }
    }

    private async Task<string?> GetPersistedDataAsync()
    {
        if (!File.Exists(_config.IPv6PersistedDataFilePath))
        {
            _logger.Error<IPv6Log>($"File {_config.IPv6PersistedDataFilePath} does not exist.");
            return null;
        }

        try
        {
            return await File.ReadAllTextAsync(_config.IPv6PersistedDataFilePath);
        }
        catch (Exception e)
        {
            _logger.Error<IPv6Log>("Failed to read persisted data file.", e);
            return null;
        }
    }

    private unsafe List<NetworkAddress> GetFakeIpv6Addresses(FFISlice result)
    {
        int n = checked((int)result.Len);
        List<NetworkAddress> addresses = [];

        ReadOnlySpan<Ipv6AddrRaw> rawSpan = new(result.Ptr.ToPointer(), n);

        for (int i = 0; i < n; i++)
        {
            byte[] buffer = new byte[16];
            fixed (byte* src = rawSpan[i].Bytes)
            {
                new ReadOnlySpan<byte>(src, 16).CopyTo(buffer);
            }

            string address = new IpAddress { Bytes = buffer }.ToString() ?? string.Empty;

            if (NetworkAddress.TryParse(address, out NetworkAddress networkAddress))
            {
                addresses.Add(networkAddress);
            }
        }

        return addresses;
    }

    private async Task PersistUpdatedDataAsync(Ipv6ChaosAlgoHandle handle, string path)
    {
        NativeResult result = (NativeResult)NativeMethods.PersistDump(handle, out Utf8StringHandle stringHandle);

        using (stringHandle)
        {
            if (result != NativeResult.Success)
            {
                return;
            }

            string persistedData = Marshal.PtrToStringUTF8(stringHandle.DangerousGetHandle()) ?? string.Empty;

            if (!string.IsNullOrEmpty(persistedData))
            {
                await File.WriteAllTextAsync(path, persistedData);
            }
        }
    }
}