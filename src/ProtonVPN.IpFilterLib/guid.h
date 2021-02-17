#pragma once
#include <rpc.h>
#include <string>

namespace ipfilter
{
    namespace guid
    {
        GUID makeGuid(const GUID* value);

        GUID makeGuid();
    }
}
