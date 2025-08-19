#include "pch.h"
#include "condition.h"
#include <winsock2.h>

namespace ipfilter
{
    namespace condition
    {
        Condition::Condition(matcher::Matcher matcher, const GUID& identifier,
                             const std::shared_ptr<value::Value>& value):
            matcher(matcher), identifier(identifier), value(value)
        {
        }

        Condition::operator FWPM_FILTER_CONDITION()
        {
            FWPM_FILTER_CONDITION condition{};

            condition.fieldKey = this->identifier;
            condition.matchType = this->matcher;
            condition.conditionValue = *this->value.get();

            return condition;
        }

        Condition localIpV4Address(matcher::Matcher matcher,
                                   const value::IpAddressV4& addr)
        {
            return Condition(matcher, FWPM_CONDITION_IP_LOCAL_ADDRESS,
                             std::make_shared<value::IpAddressV4>(addr));
        }

        Condition localIpV6AddressWithPrefix(matcher::Matcher matcher,
                                             const value::IpAddressV6WithPrefix& addr)
        {
            return Condition(matcher, FWPM_CONDITION_IP_LOCAL_ADDRESS,
                             std::make_shared<value::IpAddressV6WithPrefix>(addr));
        }

        Condition remoteIpV4Address(matcher::Matcher matcher,
                                    const value::IpAddressV4& addr)
        {
            return Condition(matcher, FWPM_CONDITION_IP_REMOTE_ADDRESS,
                             std::make_shared<value::IpAddressV4>(addr));
        }

        Condition remoteIpNetworkAddressV4(matcher::Matcher matcher,
                                           const value::IpNetworkAddressV4& addr)
        {
            return Condition(matcher, FWPM_CONDITION_IP_REMOTE_ADDRESS,
                             std::make_shared<value::IpNetworkAddressV4>(addr));
        }

        Condition remoteIpV6Address(matcher::Matcher matcher,
                                    const value::IpAddressV6& addr)
        {
            return Condition(matcher, FWPM_CONDITION_IP_REMOTE_ADDRESS,
                std::make_shared<value::IpAddressV6>(addr));
        }

        Condition remoteIpV6AddressWithPrefix(matcher::Matcher matcher,
                                              const value::IpAddressV6WithPrefix& addr)
        {
            return Condition(matcher, FWPM_CONDITION_IP_REMOTE_ADDRESS,
                std::make_shared<value::IpAddressV6WithPrefix>(addr));
        }

        Condition remotePort(matcher::Matcher matcher, const value::Port& port)
        {
            return Condition(matcher, FWPM_CONDITION_IP_REMOTE_PORT,
                             std::make_shared<value::Port>(port));
        }

        Condition localPort(matcher::Matcher matcher, const value::Port& port)
        {
            return Condition(matcher, FWPM_CONDITION_IP_LOCAL_PORT,
                             std::make_shared<value::Port>(port));
        }

        Condition tcpProtocol(matcher::Matcher matcher, const value::TcpProtocol& protocol)
        {
            return Condition(matcher, FWPM_CONDITION_IP_PROTOCOL,
                             std::make_shared<value::TcpProtocol>(protocol));
        }

        Condition loopback()
        {
            return Condition(matcher::Matcher(FWP_MATCH_FLAGS_ALL_SET),
                             FWPM_CONDITION_FLAGS, std::make_shared<value::Flag>(
                                 value::Flag::loopback()));
        }

        Condition nonLoopback()
        {
            return Condition(matcher::Matcher(FWP_MATCH_FLAGS_NONE_SET),
                             FWPM_CONDITION_FLAGS, std::make_shared<value::Flag>(
                                 value::Flag::loopback()));
        }

        Condition applicationId(
            matcher::Matcher matcher,
            const value::ApplicationId& appId)
        {
            return Condition(matcher, FWPM_CONDITION_ALE_APP_ID,
                             std::make_shared<value::ApplicationId>(appId));
        }

        Condition netInterface(matcher::Matcher matcher, const NetInterface& iface)
        {
            return Condition(
                matcher,
                FWPM_CONDITION_IP_LOCAL_INTERFACE,
                std::make_shared<value::NetInterfaceId>(iface.getLocalId()));
        }

        Condition netInterfaceIndex(matcher::Matcher matcher, const NetInterface& iface)
        {
            return Condition(
                matcher,
                FWPM_CONDITION_INTERFACE_INDEX,
                std::make_shared<value::NetInterfaceIndex>(iface.getIndex()));
        }

        Condition icmpv6Protocol(matcher::Matcher matcher)
        {
            return Condition(matcher, FWPM_CONDITION_IP_PROTOCOL, std::make_shared<value::IcmpProtocol>());
        }

        Condition icmpType(matcher::Matcher matcher, UINT16 type)
        {
            return Condition(matcher, FWPM_CONDITION_ICMP_TYPE, std::make_shared<value::IcmpType>(type));
        }

        Condition icmpCode(matcher::Matcher matcher, UINT16 code)
        {
            return Condition(matcher, FWPM_CONDITION_ICMP_CODE, std::make_shared<value::IcmpCode>(code));
        }
    }
}
