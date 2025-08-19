#include "pch.h"
#include <fwptypes.h>
#include <fwpmu.h>
#include <vector>
#include <cguid.h>

#include "ip_filter.h"
#include "guid.h"
#include "filter_specification.h"
#include "net_interface.h"

void IPFilterGetLayerKey(
    GUID& spec,
    unsigned int layer)
{
    switch (layer)
    {
    case (unsigned int)IPFilterLayer::AppAuthConnectV4:
        spec = FWPM_LAYER_ALE_AUTH_CONNECT_V4;
        break;
    case (unsigned int)IPFilterLayer::AppAuthConnectV6:
        spec = FWPM_LAYER_ALE_AUTH_CONNECT_V6;
        break;
    case (unsigned int)IPFilterLayer::AppFlowEstablishedV4:
        spec = FWPM_LAYER_ALE_FLOW_ESTABLISHED_V4;
        break;
    case (unsigned int)IPFilterLayer::AppFlowEstablishedV6:
        spec = FWPM_LAYER_ALE_FLOW_ESTABLISHED_V6;
        break;
    case (unsigned int)IPFilterLayer::BindRedirectV4:
        spec = FWPM_LAYER_ALE_BIND_REDIRECT_V4;
        break;
    case (unsigned int)IPFilterLayer::BindRedirectV6:
        spec = FWPM_LAYER_ALE_BIND_REDIRECT_V6;
        break;
    case (unsigned int)IPFilterLayer::AppConnectRedirectV4:
        spec = FWPM_LAYER_ALE_CONNECT_REDIRECT_V4;
        break;
    case (unsigned int)IPFilterLayer::OutboundIPPacketV4:
        spec = FWPM_LAYER_OUTBOUND_IPPACKET_V4;
        break;
    case (unsigned int)IPFilterLayer::AppAuthRecvAcceptV6:
        spec = FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V6;
        break;
    default:
        throw std::invalid_argument("Invalid layer");
    }
}

void IPFilterSetFilterSpecificationAction(
    ipfilter::FilterSpecification& specification,
    unsigned int action,
    GUID* calloutKey)
{
    switch (action)
    {
    case (unsigned int)IPFilterAction::SoftBlock:
        specification.soft();
        specification.block();
        break;
    case (unsigned int)IPFilterAction::HardBlock:
        specification.hard();
        specification.block();
        break;
    case (unsigned int)IPFilterAction::SoftPermit:
        specification.soft();
        specification.permit();
        break;
    case (unsigned int)IPFilterAction::HardPermit:
        specification.hard();
        specification.permit();
        break;
    case (unsigned int)IPFilterAction::Callout:
        specification.callout(calloutKey);
        break;
    default:
        throw std::invalid_argument("Invalid action");
    }
}

void IPFilterCreateFilterSpecification(
    ipfilter::FilterSpecification& spec,
    unsigned int action,
    GUID* calloutKey,
    unsigned int weight,
    BOOL persistent,
    const std::vector<ipfilter::condition::Condition>& conditions
)
{
    IPFilterSetFilterSpecificationAction(spec, action, calloutKey);

    spec.setWeight(weight);

    if (persistent)
    {
        spec.persistent();
    }

    for (auto condition : conditions)
    {
        spec.addCondition(condition);
    }
}

unsigned int IPFilterCreateFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    const std::vector<ipfilter::condition::Condition>& conditions,
    bool persistent,
    GUID* filterKey)
{
    ipfilter::FilterSpecification spec{};

    IPFilterCreateFilterSpecification(
        spec,
        action,
        calloutKey,
        weight,
        persistent,
        conditions);

    GUID layerKey{};
    IPFilterGetLayerKey(layerKey, layer);

    std::vector<FWPM_FILTER_CONDITION0> fwpmConditions{};
    for (auto condition : spec.getConditions())
    {
        fwpmConditions.push_back(condition);
    }

    FWPM_FILTER0 filter{};
    filter.filterKey = ipfilter::guid::makeGuid(filterKey);
    filter.action = spec.getAction();
    filter.layerKey = layerKey;
    filter.subLayerKey = *sublayerKey;
    filter.flags = spec.getFlags();
    filter.providerKey = providerKey;
    if (providerContextKey != nullptr && *providerContextKey != GUID_NULL)
    {
        filter.providerContextKey = *providerContextKey;
        filter.flags |= FWPM_FILTER_FLAG_HAS_PROVIDER_CONTEXT;
    }
    filter.weight = spec.getWeight();
    filter.filterCondition = fwpmConditions.data();
    filter.numFilterConditions = static_cast<UINT32>(fwpmConditions.size());
    filter.displayData.name = const_cast<wchar_t *>(displayData->name);
    filter.displayData.description = const_cast<wchar_t *>(displayData->description);

    auto result = FwpmFilterAdd(
        sessionHandle,
        &filter,
        nullptr,
        &filter.filterId);

    if (result == ERROR_SUCCESS)
    {
        *filterKey = filter.filterKey;
    }

    return result;
}

unsigned int IPFilterCreateLayerFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    BOOL persistent,
    GUID* filterKey)
{
    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        {},
        persistent,
        filterKey);
}

unsigned int IPFilterCreateRemoteIPv4Filter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    const char* addr,
    BOOL persistent,
    GUID* filterKey)
{
    std::vector<ipfilter::condition::Condition> conditions{};

    conditions.push_back(ipfilter::condition::remoteIpV4Address(
        ipfilter::matcher::equal(),
        ipfilter::ip::makeAddressV4(addr)));

    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        conditions,
        persistent,
        filterKey);
}

unsigned int IPFilterCreateAppFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    const wchar_t* path,
    BOOL persistent,
    GUID* filterKey)
{
    std::vector<ipfilter::condition::Condition> conditions{};

    auto appIdCondition = ipfilter::condition::applicationId(
        ipfilter::matcher::equal(),
        ipfilter::value::ApplicationId::fromFilePath(path));
    conditions.push_back(appIdCondition);

    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        conditions,
        persistent,
        filterKey);
}

unsigned int BlockOutsideOpenVpn(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int weight,
    const wchar_t* openVpnPath,
    const char* serverIpAddress,
    BOOL persistent,
    GUID* filterKey)
{
    auto appIdCondition = ipfilter::condition::applicationId(
        ipfilter::matcher::notEqual(),
        ipfilter::value::ApplicationId::fromFilePath(openVpnPath));

    auto serverIpCondition = ipfilter::condition::remoteIpV4Address(
        ipfilter::matcher::equal(),
        ipfilter::ip::makeAddressV4(serverIpAddress));

    std::vector<ipfilter::condition::Condition> conditions{};
    conditions.push_back(serverIpCondition);
    conditions.push_back(appIdCondition);

    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        static_cast<unsigned int>(IPFilterAction::SoftBlock),
        weight,
        nullptr,
        nullptr,
        conditions,
        persistent,
        filterKey);
}


unsigned int IPFilterCreateRemoteTCPPortFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    unsigned int port,
    BOOL persistent,
    GUID* filterKey)
{
    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        nullptr,
        nullptr,
        {
            ipfilter::condition::tcpProtocol(ipfilter::matcher::equal(),
                                             ipfilter::value::TcpProtocol::tcp()),
            ipfilter::condition::remotePort(ipfilter::matcher::equal(), port)
        },
        persistent,
        filterKey);
}

unsigned int IPFilterCreateRemoteUDPPortFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    unsigned int port,
    BOOL persistent,
    GUID* filterKey)
{
    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        nullptr,
        nullptr,
        {
            ipfilter::condition::tcpProtocol(ipfilter::matcher::equal(),
                                             ipfilter::value::TcpProtocol::udp()),
            ipfilter::condition::remotePort(ipfilter::matcher::equal(), port)
        },
        persistent,
        filterKey);
}

unsigned int IPFilterCreateRemoteNetworkIPFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    const IPFilterNetworkAddress* addr,
    BOOL persistent,
    GUID* filterKey)
{
    std::vector<ipfilter::condition::Condition> conditions{};

    if (addr->isIpv6)
    {
        auto address = ipfilter::ip::makeAddressV6(addr->address, addr->prefix);
        auto networkAddrCondition = ipfilter::condition::remoteIpV6AddressWithPrefix(
            ipfilter::matcher::equal(),
            ipfilter::value::IpAddressV6WithPrefix(address));

        conditions.push_back(networkAddrCondition);
    }
    else
    {
        auto address = ipfilter::ip::makeAddressV4(addr->address);
        auto mask = ipfilter::ip::makeAddressV4(addr->mask);
        auto networkAddrCondition = ipfilter::condition::remoteIpNetworkAddressV4(
            ipfilter::matcher::equal(),
            ipfilter::value::IpNetworkAddressV4(address, mask));

        conditions.push_back(networkAddrCondition);
    }

    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        conditions,
        persistent,
        filterKey);
}

unsigned int IPFilterCreateNetInterfaceFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    ULONG index,
    BOOL persistent,
    GUID* filterKey)
{
    std::vector<ipfilter::condition::Condition> conditions{};

    auto netInterfaces = ipfilter::getNetworkInterfaces();

    auto netInterfaceIt = ipfilter::findNetworkInterfaceByIndex(
        netInterfaces,
        index);
    if (netInterfaceIt == std::end(netInterfaces))
    {
        return E_ADAPTER_NOT_FOUND;
    }

    conditions.push_back(ipfilter::condition::netInterface(
        ipfilter::matcher::equal(),
        *netInterfaceIt));

    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        nullptr,
        nullptr,
        conditions,
        persistent,
        filterKey);
}

unsigned int IPFilterCreateLoopbackFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    BOOL persistent,
    GUID* filterKey)
{
    std::vector<ipfilter::condition::Condition> conditions{};

    conditions.push_back(ipfilter::condition::loopback());

    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        nullptr,
        nullptr,
        conditions,
        persistent,
        filterKey);
}

unsigned int BlockOutsideDns(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    ULONG index,
    BOOL persistent,
    GUID* filterKey)
{
    std::vector<ipfilter::condition::Condition> conditions{};

    auto netInterfaces = ipfilter::getNetworkInterfaces();
    auto netInterfaceIt = findNetworkInterfaceByIndex(netInterfaces, index);
    if (netInterfaceIt == std::end(netInterfaces))
    {
        return E_ADAPTER_NOT_FOUND;
    }

    conditions.push_back(ipfilter::condition::netInterfaceIndex(
        ipfilter::matcher::notEqual(),
        *netInterfaceIt));

    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        nullptr,
        conditions,
        persistent,
        filterKey);
}

unsigned int PermitRouterSolicitationMessage(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    BOOL persistent,
    GUID* filterKey)
{
    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        {
            ipfilter::condition::icmpv6Protocol(ipfilter::matcher::equal()),
            ipfilter::condition::icmpType(ipfilter::matcher::equal(), 133),
            ipfilter::condition::icmpCode(ipfilter::matcher::equal(), 0),
            ipfilter::condition::remoteIpV6Address(
                ipfilter::matcher::equal(),
                ipfilter::ip::AddressV6::linkLocalRouterMulticast())
        },
        persistent,
        filterKey);
}

unsigned int PermitRouterAdvertisementMessage(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    BOOL persistent,
    GUID* filterKey)
{
    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        {
            ipfilter::condition::icmpv6Protocol(ipfilter::matcher::equal()),
            ipfilter::condition::icmpType(ipfilter::matcher::equal(), 134),
            ipfilter::condition::icmpCode(ipfilter::matcher::equal(), 0),
            ipfilter::condition::remoteIpV6AddressWithPrefix(
                ipfilter::matcher::equal(),
                ipfilter::value::IpAddressV6WithPrefix(ipfilter::ip::AddressV6::linkLocal())),
        },
        persistent,
        filterKey);
}

unsigned int PermitNeighborSolicitationMessage(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    BOOL persistent,
    GUID* filterKey)
{
    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        {
            ipfilter::condition::icmpv6Protocol(ipfilter::matcher::equal()),
            ipfilter::condition::icmpType(ipfilter::matcher::equal(), 135),
            ipfilter::condition::icmpCode(ipfilter::matcher::equal(), 0),
        },
        persistent,
        filterKey);
}

unsigned int PermitNeighborAdvertisementMessage(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    BOOL persistent,
    GUID* filterKey)
{
    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        {
            ipfilter::condition::icmpv6Protocol(ipfilter::matcher::equal()),
            ipfilter::condition::icmpType(ipfilter::matcher::equal(), 136),
            ipfilter::condition::icmpCode(ipfilter::matcher::equal(), 0),
        },
        persistent,
        filterKey);
}

unsigned int PermitIcmpRedirectMessage(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    BOOL persistent,
    GUID* filterKey)
{
    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        {
            ipfilter::condition::icmpv6Protocol(ipfilter::matcher::equal()),
            ipfilter::condition::icmpType(ipfilter::matcher::equal(), 137),
            ipfilter::condition::icmpCode(ipfilter::matcher::equal(), 0),
            ipfilter::condition::remoteIpV6AddressWithPrefix(
                ipfilter::matcher::equal(),
                ipfilter::value::IpAddressV6WithPrefix(ipfilter::ip::AddressV6::linkLocal())),
        },
        persistent,
        filterKey);
}

unsigned int PermitOutboundIpv6Dhcp(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    BOOL persistent,
    GUID* filterKey)
{
    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        {
            ipfilter::condition::tcpProtocol(ipfilter::matcher::equal(), ipfilter::value::TcpProtocol::udp()),
            ipfilter::condition::remoteIpV6Address(
                ipfilter::matcher::equal(),
                ipfilter::value::IpAddressV6(ipfilter::ip::AddressV6::linkLocalDhcpMulticast())),
            ipfilter::condition::remoteIpV6Address(
                ipfilter::matcher::equal(),
                ipfilter::value::IpAddressV6(ipfilter::ip::AddressV6::siteLocalDhcpMulticast())),
            ipfilter::condition::remotePort(ipfilter::matcher::equal(), ipfilter::value::Port(547)),
            ipfilter::condition::localIpV6AddressWithPrefix(
                ipfilter::matcher::equal(),
                ipfilter::value::IpAddressV6WithPrefix(ipfilter::ip::AddressV6::linkLocal())),
            ipfilter::condition::localPort(ipfilter::matcher::equal(), ipfilter::value::Port(546)),
        },
        persistent,
        filterKey);
}

unsigned int PermitInboundIpv6Dhcp(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    BOOL persistent,
    GUID* filterKey)
{
    auto linkLocal = ipfilter::value::IpAddressV6WithPrefix(ipfilter::ip::AddressV6::linkLocal());

    return IPFilterCreateFilter(
        sessionHandle,
        providerKey,
        sublayerKey,
        displayData,
        layer,
        action,
        weight,
        calloutKey,
        providerContextKey,
        {
            ipfilter::condition::tcpProtocol(ipfilter::matcher::equal(), ipfilter::value::TcpProtocol::udp()),
            ipfilter::condition::remoteIpV6AddressWithPrefix(ipfilter::matcher::equal(), linkLocal),
            ipfilter::condition::remotePort(ipfilter::matcher::equal(), ipfilter::value::Port(547)),
            ipfilter::condition::localIpV6AddressWithPrefix(ipfilter::matcher::equal(), linkLocal),
            ipfilter::condition::localPort(ipfilter::matcher::equal(), ipfilter::value::Port(546)),
        },
        persistent,
        filterKey);
}