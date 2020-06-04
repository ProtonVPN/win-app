#pragma once

#include <Guiddef.h>

namespace Proton
{
    namespace NetworkUtil
    {
        namespace Route
        {
            bool AddDefaultGatewayForIface(const GUID* guid, const wchar_t* gatewayAddr);

            bool DeleteDefaultGatewayForIface(const GUID* guid, const wchar_t* gatewayAddr);
        }
    }
}