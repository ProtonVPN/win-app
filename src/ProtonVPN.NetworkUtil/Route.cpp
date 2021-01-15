#include <winsock2.h>
#include <ws2tcpip.h>
#include <vector>
#include <string>
#include "Route.h"

namespace Proton
{
    namespace NetworkUtil
    {
        namespace Route
        {
            bool GetIfaceInfo(UINT index, IfaceInfo& info)
            {
                ULONG adapterListSize = 0;

                auto status = GetAdaptersAddresses(AF_INET, 0, nullptr, nullptr, &adapterListSize);
                if (status != ERROR_BUFFER_OVERFLOW)
                {
                    return false;
                }

                std::vector<char> adapterListData(adapterListSize);

                status = GetAdaptersAddresses(AF_INET, 0, nullptr, reinterpret_cast<PIP_ADAPTER_ADDRESSES>(adapterListData.data()), &adapterListSize);
                if (status != NO_ERROR)
                {
                    return false;
                }

                IP_ADAPTER_ADDRESSES* adapter = reinterpret_cast<IP_ADAPTER_ADDRESSES*>(adapterListData.data());
                while (adapter != nullptr)
                {
                    std::string adapterName = adapter->AdapterName;

                    if (adapter->IfIndex == index)
                    {
                        info.Ipv4Metric = adapter->Ipv4Metric;
                        info.Index = adapter->IfIndex;
                        info.Luid = adapter->Luid;

                        return true;
                    }

                    adapter = adapter->Next;
                }

                return false;
            }

            bool AddDefaultGatewayForIface(UINT index, const wchar_t* gatewayAddr)
            {
                IfaceInfo info{};
                if (!GetIfaceInfo(index, info))
                {
                    return false;
                }

                MIB_IPFORWARDROW route{};
                route.dwForwardIfIndex = info.Index;
                route.dwForwardProto = MIB_IPPROTO_NETMGMT;
                route.dwForwardMetric1 = info.Ipv4Metric;
                route.dwForwardMetric2 = -1;
                route.dwForwardMetric3 = -1;
                route.dwForwardMetric4 = -1;

                InetPton(AF_INET, gatewayAddr, &route.dwForwardNextHop);

                auto status = CreateIpForwardEntry(&route);
                if (status != NO_ERROR && status != ERROR_OBJECT_ALREADY_EXISTS)
                {
                    return false;
                }

                return true;
            }

            bool DeleteDefaultGatewayForIface(UINT index, const wchar_t* gatewayAddr)
            {
                IfaceInfo info{};
                if (!GetIfaceInfo(index, info))
                {
                    return false;
                }

                MIB_IPFORWARDROW route{};
                route.dwForwardIfIndex = info.Index;
                route.dwForwardProto = MIB_IPPROTO_NETMGMT;
                route.dwForwardMetric1 = info.Ipv4Metric;
                route.dwForwardMetric2 = -1;
                route.dwForwardMetric3 = -1;
                route.dwForwardMetric4 = -1;

                InetPton(AF_INET, gatewayAddr, &route.dwForwardNextHop);

                auto status = DeleteIpForwardEntry(&route);
                if (status != NO_ERROR && status != ERROR_NOT_FOUND)
                {
                    return false;
                }

                return true;
            }
        }
    }
}