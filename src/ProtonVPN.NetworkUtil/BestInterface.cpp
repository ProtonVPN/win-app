#include "StdAfx.h"
#include "BestInterface.h"
#include <Iphlpapi.h>
#include <ws2tcpip.h>

bool IsIfaceExcluded(const std::vector<std::wstring>& guids, const std::wstring& ifaceGuid)
{
    auto it = std::find(guids.begin(), guids.end(), ifaceGuid);

    return it != guids.end();
}

IN_ADDR BestInterface::IpAddress(const std::vector<std::wstring>& excludedIfaceGuids)
{
    PIP_ADAPTER_INFO pAdapterInfo;
    PIP_ADAPTER_INFO pAdapter;
    PIP_ADDR_STRING pIPAddrString, pIPGwString;
    ULONG ulOutBufLen;
    IN_ADDR ipBuff = {0};

    pAdapterInfo = static_cast<IP_ADAPTER_INFO*>(malloc(sizeof(IP_ADAPTER_INFO)));
    if (!pAdapterInfo)
    {
        return ipBuff;
    }

    ulOutBufLen = sizeof(IP_ADAPTER_INFO);

    if (GetAdaptersInfo(pAdapterInfo, &ulOutBufLen) == ERROR_BUFFER_OVERFLOW)
    {
        free(pAdapterInfo);
        pAdapterInfo = static_cast<IP_ADAPTER_INFO*>(malloc(ulOutBufLen));
        if (!pAdapterInfo)
        {
            return ipBuff;
        }
    }

    if (GetAdaptersInfo(pAdapterInfo, &ulOutBufLen) == NO_ERROR)
    {
        pAdapter = pAdapterInfo;
        while (pAdapter)
        {
            std::string guid(pAdapter->AdapterName);
            if (IsIfaceExcluded(excludedIfaceGuids, std::wstring(guid.begin(), guid.end())))
            {
                pAdapter = pAdapter->Next;
                continue;
            }

            pIPAddrString = &pAdapter->IpAddressList;

            pIPGwString = &pAdapter->GatewayList;
            while (pIPAddrString)
            {
                IN_ADDR maskBuff = {0};
                inet_pton(AF_INET, pIPAddrString->IpMask.String, &maskBuff);

                if (maskBuff.S_un.S_addr == 0)
                {
                    pIPAddrString = pIPAddrString->Next;
                    continue;
                }

                IN_ADDR gwBuff = {0};
                inet_pton(AF_INET, pIPGwString->IpAddress.String, &gwBuff);

                // First adapter with a default gateway
                if (gwBuff.S_un.S_addr != 0)
                {
                    inet_pton(AF_INET, pIPAddrString->IpAddress.String, &ipBuff);

                    free(pAdapterInfo);

                    return ipBuff;
                }

                pIPAddrString = pIPAddrString->Next;
            }

            pAdapter = pAdapter->Next;
        }
    }

    free(pAdapterInfo);

    return ipBuff;
}
