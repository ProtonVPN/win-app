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
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace ProtonVPN.WireGuardDriver
{
    public class Adapter : IDisposable
    {
        private readonly IntPtr _handle;
        private uint _lastGetGuess;

        public Adapter(string name)
        {
            _lastGetGuess = 1024;
            _handle = Win32.OpenAdapter(name);

            if (_handle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

        public void Dispose()
        {
            Win32.CloseAdapter(_handle);
        }

        public unsafe Interface GetConfiguration()
        {
            Interface iface = new();
            byte[] bytes;

            for (;;)
            {
                bytes = new byte[_lastGetGuess];
                if (Win32.GetConfiguration(_handle, bytes, ref _lastGetGuess))
                {
                    break;
                }

                if (Marshal.GetLastWin32Error() != 234 /* ERROR_MORE_DATA */)
                {
                    throw new Win32Exception();
                }
            }

            fixed (void* start = bytes)
            {
                IoctlInterface* ioctlIface = (IoctlInterface*)start;
                if ((ioctlIface->Flags & IoctlInterfaceFlags.HasPublicKey) != 0)
                {
                    iface.PublicKey = new Key(ioctlIface->PublicKey);
                }

                if ((ioctlIface->Flags & IoctlInterfaceFlags.HasPrivateKey) != 0)
                {
                    iface.PrivateKey = new Key(ioctlIface->PrivateKey);
                }

                if ((ioctlIface->Flags & IoctlInterfaceFlags.HasListenPort) != 0)
                {
                    iface.ListenPort = ioctlIface->ListenPort;
                }

                Peer[] peers = new Peer[ioctlIface->PeersCount];
                IoctlPeer* ioctlPeer = (IoctlPeer*)((byte*)ioctlIface + sizeof(IoctlInterface));

                for (uint i = 0; i < peers.Length; ++i)
                {
                    Peer peer = new();
                    if ((ioctlPeer->Flags & IoctlPeerFlags.HasPublicKey) != 0)
                    {
                        peer.PublicKey = new Key(ioctlPeer->PublicKey);
                    }

                    if ((ioctlPeer->Flags & IoctlPeerFlags.HasPresharedKey) != 0)
                    {
                        peer.PresharedKey = new Key(ioctlPeer->PresharedKey);
                    }

                    if ((ioctlPeer->Flags & IoctlPeerFlags.HasPersistentKeepalive) != 0)
                    {
                        peer.PersistentKeepalive = ioctlPeer->PersistentKeepalive;
                    }

                    if ((ioctlPeer->Flags & IoctlPeerFlags.HasEndpoint) != 0)
                    {
                        if (ioctlPeer->Endpoint.SiFamily == AddressFamily.AfInet)
                        {
                            byte[] ip = new byte[4];
                            Marshal.Copy((IntPtr)ioctlPeer->Endpoint.Ipv4.SinAddr.Bytes, ip, 0, 4);
                            peer.Endpoint = new IPEndPoint(new IPAddress(ip),
                                (ushort)IPAddress.NetworkToHostOrder((short)ioctlPeer->Endpoint.Ipv4.SinPort));
                        }
                        else if (ioctlPeer->Endpoint.SiFamily == AddressFamily.AfInet6)
                        {
                            byte[] ip = new byte[16];
                            Marshal.Copy((IntPtr)ioctlPeer->Endpoint.Ipv6.Sin6Addr.Bytes, ip, 0, 16);
                            peer.Endpoint = new IPEndPoint(new IPAddress(ip),
                                (ushort)IPAddress.NetworkToHostOrder((short)ioctlPeer->Endpoint.Ipv6.Sin6Port));
                        }
                    }

                    peer.TxBytes = ioctlPeer->TxBytes;
                    peer.RxBytes = ioctlPeer->RxBytes;
                    if (ioctlPeer->LastHandshake != 0)
                    {
                        peer.LastHandshake = DateTime.FromFileTimeUtc((long)ioctlPeer->LastHandshake);
                    }

                    AllowedIP[] allowedIPs = new AllowedIP[ioctlPeer->AllowedIPsCount];
                    IoctlAllowedIP* ioctlAllowedIP = (IoctlAllowedIP*)((byte*)ioctlPeer + sizeof(IoctlPeer));

                    for (uint j = 0; j < allowedIPs.Length; ++j)
                    {
                        var allowedIP = new AllowedIP();
                        if (ioctlAllowedIP->AddressFamily == AddressFamily.AfInet)
                        {
                            byte[] ip = new byte[4];
                            Marshal.Copy((IntPtr)ioctlAllowedIP->V4.Bytes, ip, 0, 4);
                            allowedIP.Address = new IPAddress(ip);
                        }
                        else if (ioctlAllowedIP->AddressFamily == AddressFamily.AfInet6)
                        {
                            byte[] ip = new byte[16];
                            Marshal.Copy((IntPtr)ioctlAllowedIP->V6.Bytes, ip, 0, 16);
                            allowedIP.Address = new IPAddress(ip);
                        }

                        allowedIP.Cidr = ioctlAllowedIP->Cidr;
                        allowedIPs[j] = allowedIP;
                        ioctlAllowedIP = (IoctlAllowedIP*)((byte*)ioctlAllowedIP + sizeof(IoctlAllowedIP));
                    }

                    peer.AllowedIPs = allowedIPs;
                    peers[i] = peer;
                    ioctlPeer = (IoctlPeer*)ioctlAllowedIP;
                }

                iface.Peers = peers;
            }

            return iface;
        }
    }
}