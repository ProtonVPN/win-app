#include "StdAfx.h"
#include "NetworkIPv6Settings.h"
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

        void NetworkIPv6Settings::enableIPv6OnAllAdapters(bool enable)
        {
            CComPtr<INetCfgComponent> ipv6Component = this->getIPv6Component();

            CComPtr<INetCfgComponentBindings> componentBindings;
            assertSuccess(ipv6Component->QueryInterface(
                IID_INetCfgComponentBindings,
                reinterpret_cast<void**>(&componentBindings)
            ));

            CComPtr<IEnumNetCfgBindingPath> bindingPathList;
            assertSuccess(componentBindings->EnumBindingPaths(
                EBP_BELOW,
                &bindingPathList
            ));

            while (true)
            {
                CComPtr<INetCfgBindingPath> binding;
                ULONG count = 0;

                const auto result = bindingPathList->Next(1, &binding, &count);
                if (result != S_OK)
                {
                    break;
                }

                LPWSTR token = nullptr;
                binding->GetPathToken(&token);
                std::wstring tokenStr = token;
                CoTaskMemFree(token);
                const auto enabled = binding->IsEnabled();

                if (!enable)
                {
                    if (enabled == S_OK)
                    {
                        interfacesAffected.insert(tokenStr);
                    }

                    assertSuccess(binding->Enable(false));
                }
                else
                {
                    auto it = interfacesAffected.find(tokenStr);
                    if (it != interfacesAffected.end())
                    {
                        assertSuccess(binding->Enable(true));
                    }
                }
            }

            if (enable)
            {
                interfacesAffected.clear();
            }
        }

        CComPtr<INetCfgComponent> NetworkIPv6Settings::getIPv6Component()
        {
            CComPtr<INetCfgClass> networkProtocolComponents;

            assertSuccess(this->networkConfiguration->QueryNetCfgClass(
                &GUID_DEVCLASS_NETTRANS,
                IID_INetCfgClass,
                reinterpret_cast<void**>(&networkProtocolComponents)
            ));

            CComPtr<INetCfgComponent> ipv6Component;

            assertSuccess(networkProtocolComponents->FindComponent(
                L"ms_tcpip6",
                &ipv6Component
            ));

            return ipv6Component;
        }
    }
}
