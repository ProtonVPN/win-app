#ifndef PROTON_NETWORK_UTIL_NETWORK_IPV6_SETTINGS_H
#define PROTON_NETWORK_UTIL_NETWORK_IPV6_SETTINGS_H

#include <netcfgx.h>
#include <atlbase.h>

#include <string>
#include <set>

namespace Proton
{
    namespace NetworkUtil
    {
        class NetworkIPv6Settings
        {
        public:
            NetworkIPv6Settings(CComPtr<INetCfg> networkConfiguration);

            void enableIPv6OnAllAdapters(bool enable, const std::set<std::wstring>& excludeIds);

            void enableIPv6OnInterfacesWithId(const std::wstring& id);
        private:
            CComPtr<INetCfg> networkConfiguration;
        };
    }
}

#endif // PROTON_NETWORK_UTIL_NETWORK_IPV6_SETTINGS_H
