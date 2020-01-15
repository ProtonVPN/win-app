#ifndef PROTON_NETWORK_UTIL_NETWORK_CONFIGURATION_H
#define PROTON_NETWORK_UTIL_NETWORK_CONFIGURATION_H

#include <Windows.h>
#include <Netcfgx.h>
#include <atlbase.h>

#include "NetworkIPv6Settings.h"

#include <string>

namespace Proton
{
    namespace NetworkUtil
    {
        class NetworkConfiguration
        {
        public:
            virtual ~NetworkConfiguration();

            static NetworkConfiguration instance();

            void initialize();

            void applyChanges();

            CComPtr<INetCfgLock> acquireWriteLock(
                DWORD timeoutInMs,
                const std::wstring& appName);

            NetworkIPv6Settings ipv6Settings();

        protected:
            NetworkConfiguration(CComPtr<INetCfg> config);

        private:
            CComPtr<INetCfg> config;
        };
    }
}

#endif // PROTON_NETWORK_UTIL_NETWORK_CONFIGURATION_H
