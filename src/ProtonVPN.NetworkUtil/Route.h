#pragma once

#include <Guiddef.h>
#include <iphlpapi.h>

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
                IF_LUID Luid;
            };

            bool AddDefaultGatewayForIface(const GUID* guid, const wchar_t* gatewayAddr);

            bool DeleteDefaultGatewayForIface(const GUID* guid, const wchar_t* gatewayAddr);

            bool GetIfaceInfo(const GUID& guid, IfaceInfo& info);
        }
    }
}
