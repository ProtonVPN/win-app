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
                    if (parts.Length < 4)
                    {
                        return string.Empty;
                    }

                    ip = string.Join(".", parts.Take(4));
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
            var mask = 0xffffffff;
            mask <<= (32 - Convert.ToInt32(bits));
            return new IPAddress((uint)IPAddress.HostToNetworkOrder((int)mask)).ToString();
        }

        private string[] GetCidrParts()
        {
            return _address.Split('.', '/');
        }
    }
}
