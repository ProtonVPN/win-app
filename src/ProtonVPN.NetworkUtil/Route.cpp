#include "Route.h"

#include <winsock2.h>
#include <iphlpapi.h>
#include <ws2tcpip.h>
#include <vector>
#include <string>

namespace Proton
{
    namespace NetworkUtil
    {
        namespace Route
        {
            struct IfaceInfo
            {
                GUID Guid;
                ULONG Ipv4Metric;
                IF_INDEX Index;
            };

            bool GetIfaceInfo(const GUID& guid, IfaceInfo& info)
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

                    GUID adapterGuid{};
                    CLSIDFromString(std::wstring(adapterName.begin(), adapterName.end()).data(), &adapterGuid);

                    if (adapterGuid == guid)
                    {
                        info.Guid = guid;
                        info.Ipv4Metric = adapter->Ipv4Metric;
                        info.Index = adapter->IfIndex;

                        return true;
                    }

                    adapter = adapter->Next;
                }

                return false;
            }

            bool AddDefaultGatewayForIface(const GUID* guid, const wchar_t* gatewayAddr)
            {
                IfaceInfo info{};
                if (!GetIfaceInfo(*guid, info))
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

            bool DeleteDefaultGatewayForIface(const GUID* guid, const wchar_t* gatewayAddr)
            {
                IfaceInfo info{};
                if (!GetIfaceInfo(*guid, info))
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