#include "StdAfx.h"
#include "NetworkConfiguration.h"
#include "Assertion.h"

namespace Proton
{
    namespace NetworkUtil
    {
        NetworkConfiguration::NetworkConfiguration(CComPtr<INetCfg> config):
            config(config)
        {
        }

        NetworkConfiguration NetworkConfiguration::instance()
        {
            CComPtr<INetCfg> config;

            assertSuccess(CoCreateInstance(
                CLSID_CNetCfg,
                nullptr,
                CLSCTX_INPROC_SERVER,
                IID_INetCfg,
                reinterpret_cast<void**>(&config)
            ));

            return NetworkConfiguration(config);
        }

        NetworkConfiguration::~NetworkConfiguration()
        {
            this->config->Uninitialize();
        }

        void NetworkConfiguration::initialize()
        {
            assertSuccess(this->config->Initialize(nullptr));
        }

        void NetworkConfiguration::applyChanges()
        {
            assertSuccess(this->config->Apply());
        }

        CComPtr<INetCfgLock> NetworkConfiguration::acquireWriteLock(
            DWORD timeoutInMs,
            const std::wstring& appName)
        {
            CComPtr<INetCfgLock> lock;

            assertSuccess(this->config->QueryInterface(
                IID_INetCfgLock,
                reinterpret_cast<void **>(&lock)
            ));

            assertSuccess(lock->AcquireWriteLock(
                timeoutInMs,
                appName.c_str(),
                nullptr
            ));

            return lock;
        }

        NetworkIPv6Settings NetworkConfiguration::ipv6Settings()
        {
            return NetworkIPv6Settings(this->config);
        }

        const std::vector<NetInterface> NetworkConfiguration::getNetworkInterfacesById(const std::wstring& id)
        {
            return GetNetworkInterfacesById(this->config, id);
        }
    }
}
