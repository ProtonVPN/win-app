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
        }
    }
}