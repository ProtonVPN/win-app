/*
 * Copyright (c) 2021 Proton Technologies AG
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
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Threading;
using ProtonVPN.Common.Vpn;

namespace ProtonVPN.Vpn.WireGuard
{
    internal class TrafficManager
    {
        private readonly string _pipeName;
        private StreamReader _reader;
        private NamedPipeClientStream _stream;
        private readonly SingleAction _updateBytesTransferredAction;
        private readonly ILogger _logger;

        public event EventHandler<InOutBytes> TrafficSent;

        public TrafficManager(string pipeName, ILogger logger)
        {
            _pipeName = pipeName;
            _logger = logger;
            _updateBytesTransferredAction = new SingleAction(UpdateBytesTransferred);
        }

        public void Start()
        {
            _updateBytesTransferredAction.Run();
        }

        public void Stop()
        {
            _updateBytesTransferredAction.Cancel();
        }

        private async Task UpdateBytesTransferred(CancellationToken cancellationToken)
        {
            await ConnectToPipe(cancellationToken);

            try
            {
                while (_stream != null && _stream.IsConnected)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    byte[] bytes = Encoding.UTF8.GetBytes("get=1\n\n");
                    _stream.Write(bytes, 0, bytes.Length);
                    ulong rx = 0, tx = 0;
                    while (true)
                    {
                        string line = await _reader.ReadLineAsync();
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

                        TrafficSent?.Invoke(this, new InOutBytes(rx, tx));
                    }

                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                _logger.Error("[TrafficManager] Error receiving traffic data.", e);
                throw;
            }
            finally
            {
                CloseStream();
            }
        }

        private async Task ConnectToPipe(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _stream = new NamedPipeClientStream(_pipeName);
                    await _stream.ConnectAsync(cancellationToken);
                    _reader = new StreamReader(_stream);
                    break;
                }
                catch (Exception e)
                {
                    _logger.Error("[TrafficManager] Failed to connect to a named pipe.", e);
                }

                await Task.Delay(1000, cancellationToken);
            }
        }

        private void CloseStream()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}