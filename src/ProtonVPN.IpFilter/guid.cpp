#include "pch.h"
#include <cguid.h>
#include "guid.h"

#include <stdexcept>

namespace ipfilter
{
    namespace guid
    {
        GUID makeGuid(const GUID* value)
        {
            if (*value != GUID_NULL)
            {
                return *value;
            }

            return makeGuid();
        }

        GUID makeGuid()
        {
            GUID guid;

            auto result = UuidCreate(&guid);
            if (result != RPC_S_OK)
            {
                throw std::runtime_error("Failed to create GUID");
            }

            return guid;
        }
    }
}
