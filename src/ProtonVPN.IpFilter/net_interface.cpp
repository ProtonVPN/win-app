#include "pch.h"
#include <winsock2.h>
#include <iphlpapi.h>
#include <algorithm>
#include <stdexcept>

#include "net_interface.h"
#include "buffer.h"

namespace ipfilter
{
    NetInterface::NetInterface(const std::string& name, uint64_t localId, ULONG index):
        name(name), localId(localId), index(index)
    {
    }

    std::string NetInterface::getName() const
    {
        return this->name;
    }

    uint64_t NetInterface::getLocalId() const
    {
        return this->localId;
    }

    ULONG NetInterface::getIndex() const
    {
        return this->index;
    }

    std::vector<NetInterface> getNetworkInterfaces()
    {
        ULONG family = AF_UNSPEC;

        ULONG size = 0;
        GetAdaptersAddresses(family, 0, nullptr, nullptr, &size);

        Buffer buffer(size);

        auto res = GetAdaptersAddresses(
            family,
            0,
            nullptr,
            buffer.as<IP_ADAPTER_ADDRESSES_LH>(),
            &size);
        if (res != ERROR_SUCCESS)
        {
            throw std::runtime_error("Failed to get network interfaces");
        }

        std::vector<NetInterface> ifaces;

        auto address = buffer.as<IP_ADAPTER_ADDRESSES_LH>();
        while (address)
        {
            ifaces.push_back(NetInterface(address->AdapterName, address->Luid.Value, address->IfIndex));
            address = address->Next;
        }

        return ifaces;
    }

    std::vector<NetInterface>::iterator findNetworkInterfaceByIndex(
        std::vector<NetInterface>& interfaces,
        ULONG index)
    {
        return std::find_if(
            interfaces.begin(),
            interfaces.end(),
            [&index](const auto& netInterface)
            {
                return netInterface.getIndex() == index;
            });
    }
}
