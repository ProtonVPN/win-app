#pragma once

#include <comdef.h>
#include <netcfgx.h>
#include <atlbase.h>

#include <string>
#include <vector>

namespace Proton
{
	namespace NetworkUtil
	{
		class NetComponent;

		class NetBindingPath
		{
		public:
			NetBindingPath(CComPtr<INetCfgBindingPath>& bindingPath) : bindingPath(bindingPath)
			{
			}

			NetComponent getOwner();

			bool isEnabled();

			void enable();

			void disable();
		private:
			CComPtr<INetCfgBindingPath> bindingPath;
		};

		class NetComponent
		{
		public:
			NetComponent(CComPtr<INetCfgComponent>& component) : component(component)
			{
			}

			const std::wstring id();

			const std::wstring displayName();

			const std::wstring bindName();

			const std::vector<NetBindingPath> bindingPaths();
		private:
			CComPtr<INetCfgComponent> component;
		};

		class NetInterface
		{
		public:
			NetInterface(const NetComponent& netComponent) : netComponent(netComponent)
			{
			}

			const std::wstring id();

			const std::wstring displayName();

			const std::wstring bindName();

			bool isIPv6Enabled();

			void enableIPv6();

			void disableIPv6();
		private:
			NetComponent netComponent;
		};

		const std::vector<NetInterface> GetNetworkInterfaces(const CComPtr<INetCfg>& netCfg);

		const std::vector<NetInterface> GetNetworkInterfacesById(
			const CComPtr<INetCfg>& netCfg,
			const std::wstring& id
		);
	}
}