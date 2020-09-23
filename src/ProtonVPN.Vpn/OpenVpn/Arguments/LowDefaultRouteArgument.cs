using System.Collections;
using System.Collections.Generic;

namespace ProtonVPN.Vpn.OpenVpn.Arguments
{
    internal class LowDefaultRouteArgument : IEnumerable<string>
    {
        public IEnumerator<string> GetEnumerator()
        {
            yield return "--pull-filter ignore \"redirect-gateway\"";
            yield return "--route 0.0.0.0 0.0.0.0 vpn_gateway 32000";
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
