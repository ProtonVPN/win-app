#ifndef PROTON_NETWORK_UTIL_ASSERTION_H
#define PROTON_NETWORK_UTIL_ASSERTION_H

#include <Windows.h>

namespace Proton
{
    namespace NetworkUtil
    {
        void assertSuccess(HRESULT result);
    }
}

#endif // PROTON_NETWORKUTIL_ASSERTION_H
