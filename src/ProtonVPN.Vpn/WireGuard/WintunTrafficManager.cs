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
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Legacy.Threading;

namespace ProtonVPN.Vpn.WireGuard;

public class WintunTrafficManager
{
    private readonly string _pipeName;
    private StreamReader _reader;
    private NamedPipeClientStream _stream;
    private readonly SingleAction _updateBytesTransferredAction;

    public event EventHandler<NetworkTraffic> TrafficSent;

    public WintunTrafficManager(string pipeName)
    {
        _pipeName = pipeName;
        _updateBytesTransferredAction = new SingleAction(UpdateBytesTransferred);
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

    private void UpdateBytesTransferred()
    {
        ConnectToPipe();

        try
        {
            while (_stream != null && _stream.IsConnected)
            {
                byte[] bytes = Encoding.UTF8.GetBytes("get=1\n\n");
                _stream.Write(bytes, 0, bytes.Length);
                ulong rx = 0, tx = 0;
                while (true)
                {
                    string line = _reader.ReadLine();
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

                    TrafficSent?.Invoke(this, new NetworkTraffic(rx, tx));
                }

                Thread.Sleep(1000);
            }
        }
        catch
        {
            // ignored
        }
        finally
        {
            _stream?.Dispose();
        }
    }

    private void ConnectToPipe()
    {
        while (true)
        {
            try
            {
                _stream = new NamedPipeClientStream(_pipeName);
                _stream.Connect();
                _reader = new StreamReader(_stream);
                break;
            }
            catch
            {
                // ignored
            }

            Thread.Sleep(1000);
        }
    }
}