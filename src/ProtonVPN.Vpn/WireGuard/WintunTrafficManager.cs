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
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Vpn.WireGuard;

public class WintunTrafficManager : IWintunTrafficManager
{
    private readonly string _pipeName;
    private StreamReader? _reader;
    private NamedPipeClientStream? _stream;

    public WintunTrafficManager(IStaticConfiguration config)
    {
        _pipeName = config.WireGuard.PipeName;
    }

    public async IAsyncEnumerable<NetworkTraffic> WatchTrafficAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using (IAsyncEnumerator<NetworkTraffic> enumerator = WatchOnceAsync(cancellationToken).GetAsyncEnumerator(cancellationToken))
            {
                while (true)
                {
                    bool hasNext;
                    try
                    {
                        hasNext = await enumerator.MoveNextAsync();
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch
                    {
                        // ignored; retry outer loop
                        break;
                    }

                    if (!hasNext)
                    {
                        break;
                    }

                    yield return enumerator.Current;
                }
            }

            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
        }
    }

    private async IAsyncEnumerable<NetworkTraffic> WatchOnceAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await ConnectToPipeAsync(cancellationToken);

        try
        {
            while (_stream != null && _stream.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                byte[] bytes = Encoding.UTF8.GetBytes("get=1\n\n");
                await _stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                ulong rx = 0, tx = 0;
                while (true)
                {
                    if (_reader == null)
                    {
                        break;
                    }

                    string? line = await _reader.ReadLineAsync(cancellationToken);
                    if (line == null)
                    {
                        break;
                    }

                    line = line.Trim();
                    if (line.Length == 0)
                    {
                        break;
                    }

                    if (line.StartsWith("rx_bytes="))
                    {
                        rx += ulong.Parse(line.Substring(9));
                    }
                    else if (line.StartsWith("tx_bytes="))
                    {
                        tx += ulong.Parse(line.Substring(9));
                    }

                    yield return new NetworkTraffic(rx, tx);
                }

                await Task.Delay(1000, cancellationToken);
            }
        }
        finally
        {
            _reader?.Dispose();
            _reader = null;
            _stream?.Dispose();
            _stream = null;
        }
    }

    private async Task ConnectToPipeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                _stream = new NamedPipeClientStream(_pipeName);
                await _stream.ConnectAsync(cancellationToken);
                _reader = new StreamReader(_stream);
                break;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // ignored
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}