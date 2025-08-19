#pragma once
#include <fwptypes.h>
#include <fwpmu.h>

#include <memory>

#include "value.h"
#include "matcher.h"
#include "net_interface.h"

namespace ipfilter
{
    namespace condition
    {
        class Condition
        {
        public:
            Condition(matcher::Matcher matcher, const GUID& identifier,
                      const std::shared_ptr<value::Value>& value);

            virtual operator FWPM_FILTER_CONDITION();

        private:
            matcher::Matcher matcher;
            GUID identifier;
            std::shared_ptr<value::Value> value;
        };

        Condition localIpV4Address(matcher::Matcher matcher,
                                   const value::IpAddressV4& addr);

        Condition localIpV6AddressWithPrefix(matcher::Matcher matcher,
                                             const value::IpAddressV6WithPrefix& addr);

        Condition remoteIpV4Address(matcher::Matcher matcher,
                                    const value::IpAddressV4& addr);

        Condition remoteIpNetworkAddressV4(matcher::Matcher matcher,
                                           const value::IpNetworkAddressV4& addr);

        Condition remoteIpV6Address(matcher::Matcher matcher,
                                    const value::IpAddressV6& addr);

        Condition remoteIpV6AddressWithPrefix(matcher::Matcher matcher,
                                              const value::IpAddressV6WithPrefix& addr);

        Condition remotePort(matcher::Matcher matcher, const value::Port& port);

        Condition localPort(matcher::Matcher matcher, const value::Port& port);

        Condition tcpProtocol(matcher::Matcher matcher, const value::TcpProtocol& protocol);

        Condition loopback();

        Condition nonLoopback();

        Condition applicationId(matcher::Matcher matcher,
                                const value::ApplicationId& appId);

        Condition netInterface(matcher::Matcher matcher, const NetInterface& iface);

        Condition netInterfaceIndex(matcher::Matcher matcher, const NetInterface& iface);

        Condition icmpv6Protocol(matcher::Matcher matcher);

        Condition icmpType(matcher::Matcher matcher, UINT16 type);

        Condition icmpCode(matcher::Matcher matcher, UINT16 code);
    }
}
