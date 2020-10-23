#pragma once
#include <winsock2.h>
#include <windows.h>
#include <ws2ipdef.h>
#include <iphlpapi.h>

namespace Proton
{
    namespace NetworkUtil
    {
        class InterfaceMetric
        {
        public:
            static InterfaceMetric* instance();

            bool SetLowestMetric(IF_LUID luid);

            bool RestoreDefaultMetric(IF_LUID luid);
        private:
            static InterfaceMetric* inst;
        };
    }
}
