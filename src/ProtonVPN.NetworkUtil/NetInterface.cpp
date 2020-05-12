#include "NetInterface.h"
#include "Assertion.h"

#include <devguid.h>
#include <comdef.h>

#include <algorithm>
#include <iterator>

namespace Proton
{
	namespace NetworkUtil
	{
		const std::vector<NetInterface> GetNetworkInterfaces(const CComPtr<INetCfg>& netCfg)
		{
			std::vector<NetInterface> ifaces{};

			CComPtr<INetCfgClass> netCfgClass{};
			assertSuccess(netCfg->QueryNetCfgClass(
				&GUID_DEVCLASS_NET,
				IID_INetCfgClass,
				reinterpret_cast<void**>(&netCfgClass)
			));

			CComPtr<IEnumNetCfgComponent> componentEnum{};
			assertSuccess(netCfgClass->EnumComponents(&componentEnum));

			componentEnum->Reset();

			while (true)
			{
				ULONG count{};
				CComPtr<INetCfgComponent> component{};

				auto status = componentEnum->Next(1, &component, &count);
				if (status != S_OK)
				{
					break;
				}

				ifaces.push_back(NetComponent(component));
			}

			return ifaces;
		}

		const std::vector<NetInterface> GetNetworkInterfacesById(
			const CComPtr<INetCfg>& netCfg,
			const std::wstring& id)
		{
			auto ifaces = GetNetworkInterfaces(netCfg);

			std::vector<NetInterface> results{};

			std::copy_if(
				ifaces.begin(),
				ifaces.end(),
				std::back_inserter(results),
				[id](auto iface) { return iface.id() == id; }
			);

			return results;
		}

		const std::wstring NetInterface::id()
		{
			return this->netComponent.id();
		}

		const std::wstring NetInterface::displayName()
		{
			return this->netComponent.displayName();
		}

		const std::wstring NetInterface::bindName()
		{
			return this->netComponent.bindName();
		}

		bool NetInterface::isIPv6Enabled()
		{
			for (auto bindingPath : this->netComponent.bindingPaths())
			{
				if (bindingPath.getOwner().id() == L"ms_tcpip6")
				{
					if (bindingPath.isEnabled()) {
						return true;
					}
				}
			}

			return false;
		}

		void NetInterface::enableIPv6()
		{
			for (auto bindingPath : this->netComponent.bindingPaths())
			{
				if (bindingPath.getOwner().id() == L"ms_tcpip6")
				{
					bindingPath.enable();
				}
			}
		}

		void NetInterface::disableIPv6()
		{
			for (auto bindingPath : this->netComponent.bindingPaths())
			{
				if (bindingPath.getOwner().id() == L"ms_tcpip6")
				{
					bindingPath.disable();
				}
			}
		}

		const std::wstring NetComponent::id()
		{
			LPWSTR str{};
			assertSuccess(this->component->GetId(&str));

			std::wstring result = str;
			CoTaskMemFree(str);

			return result;
		}

		const std::wstring NetComponent::displayName()
		{
			LPWSTR str{};
			assertSuccess(this->component->GetDisplayName(&str));

			std::wstring result = str;
			CoTaskMemFree(str);

			return result;
		}

		const std::wstring NetComponent::bindName()
		{
			LPWSTR str{};
			assertSuccess(this->component->GetBindName(&str));

			std::wstring result = str;
			CoTaskMemFree(str);

			return result;
		}

		const std::vector<NetBindingPath> NetComponent::bindingPaths()
		{
			std::vector<NetBindingPath> results{};

			CComQIPtr<INetCfgComponentBindings> bindings{};
			assertSuccess(this->component->QueryInterface(
				IID_INetCfgComponentBindings,
				reinterpret_cast<void**>(&bindings)
			));

			CComPtr<IEnumNetCfgBindingPath> bindingPathList{};
			assertSuccess(bindings->EnumBindingPaths(EBP_ABOVE, &bindingPathList));

			while (true)
			{
				ULONG count = 0;
				CComPtr<INetCfgBindingPath> binding{};

				const auto result = bindingPathList->Next(1, &binding, &count);
				if (result != S_OK)
				{
					break;
				}

				results.push_back(NetBindingPath(binding));
			}

			return results;
		}

		NetComponent NetBindingPath::getOwner()
		{
			CComPtr<INetCfgComponent> component{};
			assertSuccess(this->bindingPath->GetOwner(&component));

			return component;
		}

		bool NetBindingPath::isEnabled()
		{
			auto status = this->bindingPath->IsEnabled();

			if (status == S_OK)
			{
				return true;
			}

			if (status == S_FALSE)
			{
				return false;
			}

			assertSuccess(status);

			return false;
		}

		void NetBindingPath::enable()
		{
			if (this->isEnabled())
			{
				return;
			}

			assertSuccess(this->bindingPath->Enable(true));
		}

		void NetBindingPath::disable()
		{
			if (!this->isEnabled())
			{
				return;
			}

			assertSuccess(this->bindingPath->Enable(false));
		}
	}
}