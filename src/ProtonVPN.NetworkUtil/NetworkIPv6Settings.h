#ifndef PROTON_NETWORK_UTIL_NETWORK_IPV6_SETTINGS_H
#define PROTON_NETWORK_UTIL_NETWORK_IPV6_SETTINGS_H

#include <Netcfgx.h>
#include <atlbase.h>

namespace Proton
{
    namespace NetworkUtil
    {
        class NetworkIPv6Settings
        {
        public:
            NetworkIPv6Settings(CComPtr<INetCfg> networkConfiguration);

            void enableIPv6OnAllAdapters(bool enable);

        private:
            CComPtr<INetCfg> networkConfiguration;

            CComPtr<INetCfgComponent> getIPv6Component();
        };
    }
}

#endif // PROTON_NETWORK_UTIL_NETWORK_IPV6_SETTINGS_H
