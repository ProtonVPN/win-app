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
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Common.OS.Net
{
    public class NetworkAddress
    {
        private readonly string _address;

        private readonly string _defaultMask = "255.255.255.255";

        public NetworkAddress(string address)
        {
            _address = address;
        }

        public string Ip
        {
            get
            {
                string ip = _address;

                if (IsCidr())
                {
                    string[] parts = GetCidrParts();
                    uint mask = StrMaskToUint(parts[4]);
                    uint ipUint = GetIpUint(IPAddress.Parse(string.Join(".", parts.Take(4))));
                    uint ipNetworkOrder = (uint)IPAddress.HostToNetworkOrder((int)(ipUint & mask));

                    return new IPAddress(ipNetworkOrder).ToString();
                }

                return IpValid(ip) ? ip : string.Empty;
            }
        }

        public string Mask
        {
            get
            {
                if (!IsCidr())
                {
                    return _defaultMask;
                }

                string[] parts = GetCidrParts();
                if (parts.Length < 5)
                {
                    return string.Empty;
                }

                string mask = GetMask(parts[4]);

                return IpValid(mask) ? mask : string.Empty;
            }
        }

        public string GetCidrString()
        {
            if (IsCidr())
            {
                string[] parts = GetCidrParts();
                return Ip + "/" + parts[4];
            }

            return Ip;
        }

        public bool Valid()
        {
            if (IsCidr())
            {
                return !string.IsNullOrEmpty(Ip) && !string.IsNullOrEmpty(Mask);
            }

            return !string.IsNullOrEmpty(Ip);
        }

        public bool IsCidr()
        {
            Match m = Regex.Match(_address, "^([0-9]{1,3}\\.){3}[0-9]{1,3}(\\/([0-9]|[1-2][0-9]|3[0-2]))$");
            return m.Success;
        }

        private bool IpValid(string ip)
        {
            return IPAddress.TryParse(ip, out IPAddress parsedIp) && ip.EqualsIgnoringCase(parsedIp.ToString());
        }

        private string GetMask(string bits)
        {
            uint mask = StrMaskToUint(bits);
            return new IPAddress((uint)IPAddress.HostToNetworkOrder((int)mask)).ToString();
        }

        private string[] GetCidrParts()
        {
            return _address.Split('.', '/');
        }

        private uint GetIpUint(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();
            uint ip = (uint)bytes[3] << 0;
            ip += (uint)bytes[2] << 8;
            ip += (uint)bytes[1] << 16;
            ip += (uint)bytes[0] << 24;

            return ip;
        }

        private uint StrMaskToUint(string strMask)
        {
            uint mask = 0xffffffff;
            mask <<= (32 - Convert.ToInt32(strMask));

            return mask;
        }
    }
}
