#include "StdAfx.h"
#include "NetworkIPv6Settings.h"
#include "NetInterface.h"
#include <string>
#include <set>
#include <devguid.h>

#include "Assertion.h"

namespace Proton
{
    namespace NetworkUtil
    {
        std::set<std::wstring> interfacesAffected;

        NetworkIPv6Settings::NetworkIPv6Settings(CComPtr<INetCfg> networkConfiguration) :
            networkConfiguration(networkConfiguration)
        {
        }

        void NetworkIPv6Settings::enableIPv6OnInterfacesWithId(const std::wstring& id)
        {
            auto ifaces = GetNetworkInterfacesById(this->networkConfiguration, id);
            for (auto iface : ifaces)
            {
                iface.enableIPv6();
            }
        }

        void NetworkIPv6Settings::enableIPv6OnAllAdapters(bool enable, const std::set<std::wstring>& excludeIds)
        {
            auto ifaces = GetNetworkInterfaces(this->networkConfiguration);

            auto disable = !enable;

            if (disable)
            {
                interfacesAffected.clear();
            }

            for (auto iface : ifaces)
            {
                if (excludeIds.find(iface.id()) != excludeIds.end())
                {
                    continue;
                }

                if (disable)
                {
                    interfacesAffected.insert(iface.bindName());
                    if (iface.isIPv6Enabled())
                    {
                        iface.disableIPv6();
                    }
                }
                else if (enable)
                {
                    if (interfacesAffected.find(iface.bindName()) == interfacesAffected.end())
                    {
                        continue;
                    }

                    iface.enableIPv6();
                }
            }
        }
    }
}
