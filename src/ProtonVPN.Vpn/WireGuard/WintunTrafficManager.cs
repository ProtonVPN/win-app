/*
 * Copyright (c) 2024 Proton Technologies AG
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
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Vpn.WireGuard
{
    public class WintunTrafficManager
    {
        private const string RECEIVED_BYTES_LINE = "rx_bytes=";
        private const string TRANSMITTED_BYTES_LINE = "tx_bytes=";

        private StreamReader _reader;
        private NamedPipeClientStream _stream;

        private readonly SingleAction _updateBytesTransferredAction;
        private readonly string _pipeName;
        private readonly byte[] _getCommandBytes = Encoding.UTF8.GetBytes("get=1\n\n");

        public event EventHandler<InOutBytes> TrafficSent;

        public WintunTrafficManager(string pipeName)
        {
            _pipeName = pipeName;
            _updateBytesTransferredAction = new SingleAction(UpdateBytesTransferredAsync);
        }

        public void Start()
        {
            _updateBytesTransferredAction.Run();
        }

        public void Stop()
        {
            _stream?.Dispose();
            _updateBytesTransferredAction.Cancel();
        }

        private async Task UpdateBytesTransferredAsync(CancellationToken ct)
        {
            await ConnectToPipeAsync(ct);

            try
            {
                while (!ct.IsCancellationRequested && _stream != null && _stream.IsConnected)
                {
                    await _stream.WriteAsync(_getCommandBytes, 0, _getCommandBytes.Length, ct);
                    ulong rx = 0, tx = 0;
                    while (true)
                    {
                        string line = await _reader.ReadLineAsync();
                        ct.ThrowIfCancellationRequested();
                        if (line == null)
                        {
                            break;
                        }

                        line = line.Trim();
                        if (line.Length == 0)
                        {
                            break;
                        }

                        if (line.StartsWith(RECEIVED_BYTES_LINE))
                        {
                            rx += ulong.Parse(line.Substring(9));
                        }
                        else if (line.StartsWith(TRANSMITTED_BYTES_LINE))
                        {
                            tx += ulong.Parse(line.Substring(9));
                        }

                        TrafficSent?.Invoke(this, new InOutBytes(rx, tx));
                    }

                    ct.ThrowIfCancellationRequested();

                    await Task.Delay(1000, ct);
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                if (_stream is not null)
                {
                    await _stream.DisposeAsync();
                }
            }
        }

        private async Task ConnectToPipeAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    _stream = new NamedPipeClientStream(_pipeName);
                    await _stream.ConnectAsync(ct);
                    _reader = new StreamReader(_stream);
                    break;
                }
                catch
                {
                    // ignored
                }

                ct.ThrowIfCancellationRequested();

                await Task.Delay(1000, ct);
            }
        }
    }
}