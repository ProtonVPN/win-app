using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ProtonVPN.Common.Extensions;

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
                var ip = _address;

                if (IsCidr())
                {
                    var parts = GetCidrParts();
                    var mask = StrMaskToUint(parts[4]);
                    var ipUint = GetIpUint(IPAddress.Parse(string.Join(".", parts.Take(4))));
                    var ipNetworkOrder = (uint)IPAddress.HostToNetworkOrder((int)(ipUint & mask));

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

                var parts = GetCidrParts();
                if (parts.Length < 5)
                {
                    return string.Empty;
                }

                var mask = GetMask(parts[4]);

                return IpValid(mask) ? mask : string.Empty;
            }
        }

        public string GetCidrString()
        {
            if (IsCidr())
            {
                var parts = GetCidrParts();
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
            var m = Regex.Match(_address, "^([0-9]{1,3}\\.){3}[0-9]{1,3}(\\/([0-9]|[1-2][0-9]|3[0-2]))$");
            return m.Success;
        }

        private bool IpValid(string ip)
        {
            return IPAddress.TryParse(ip, out var parsedIp) && ip.EqualsIgnoringCase(parsedIp.ToString());
        }

        private string GetMask(string bits)
        {
            var mask = StrMaskToUint(bits);
            return new IPAddress((uint)IPAddress.HostToNetworkOrder((int)mask)).ToString();
        }

        private string[] GetCidrParts()
        {
            return _address.Split('.', '/');
        }

        private uint GetIpUint(IPAddress address)
        {
            var bytes = address.GetAddressBytes();
            var ip = (uint)bytes[3] << 0;
            ip += (uint)bytes[2] << 8;
            ip += (uint)bytes[1] << 16;
            ip += (uint)bytes[0] << 24;

            return ip;
        }

        private uint StrMaskToUint(string strMask)
        {
            var mask = 0xffffffff;
            mask <<= (32 - Convert.ToInt32(strMask));

            return mask;
        }
    }
}
