#include "pch.h"
#include <fwpmu.h>
#include <stdexcept>
#include <winsock.h>

#include "value.h"

namespace ipfilter
{
    namespace value
    {
        IpAddressV4::IpAddressV4(const ip::AddressV4& addr): addr(addr)
        {
        }

        IpAddressV4::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_UINT32;
            value.uint32 = htonl(this->addr.uint32());

            return value;
        }

        IpNetworkAddressV4::IpNetworkAddressV4(
            const ip::AddressV4& addr,
            const ip::AddressV4& mask)
        {
            this->addr.addr = htonl(addr.uint32());
            this->addr.mask = htonl(mask.uint32());
        }

        IpNetworkAddressV4::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_V4_ADDR_MASK;
            value.v4AddrMask = &this->addr;

            return value;
        }

        Port::Port(unsigned short number): number(number)
        {
        }

        Port::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_UINT16;
            value.uint16 = this->number;

            return value;
        }

        TcpProtocol::TcpProtocol(uint8_t protocol):
            protocol(protocol)
        {
        }

        TcpProtocol::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_UINT8;
            value.uint16 = this->protocol;

            return value;
        }

        TcpProtocol TcpProtocol::udp()
        {
            return TcpProtocol(IPPROTO_UDP);
        }

        TcpProtocol TcpProtocol::tcp()
        {
            return TcpProtocol(IPPROTO_TCP);
        }

        Flag::Flag(uint32_t flag): flag(flag)
        {
        }

        Flag::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_UINT32;
            value.uint32 = this->flag;

            return value;
        }

        Flag Flag::loopback()
        {
            return Flag(FWP_CONDITION_FLAG_IS_LOOPBACK);
        }

        ApplicationId::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_BYTE_BLOB_TYPE;
            value.byteBlob = &this->blob;

            return value;
        }

        ApplicationId ApplicationId::fromFilePath(const std::wstring& path)
        {
            FWP_BYTE_BLOB* byteBlob = nullptr;

            auto result = FwpmGetAppIdFromFileName(path.c_str(), &byteBlob);
            if (result != ERROR_SUCCESS)
            {
                throw std::runtime_error("Application id resolution failed");
            }

            ApplicationId id(*byteBlob);

            FwpmFreeMemory(reinterpret_cast<void **>(&byteBlob));

            return id;
        }

        ApplicationId::ApplicationId(const FWP_BYTE_BLOB& blob):
            value(Buffer(blob.data, blob.size)),
            blob({static_cast<UINT32>(this->value.size()), this->value.data()})
        {
        }

        ApplicationId::ApplicationId(const ApplicationId& other):
            value(other.value), blob({
                static_cast<UINT32>(this->value.size()),
                this->value.data()
            })
        {
        }

        ApplicationId::ApplicationId(ApplicationId&& other):
            value(std::move(other.value)), blob({
                static_cast<UINT32>(this->value.size()),
                this->value.data()
            })
        {
            other.blob.data = nullptr;
            other.blob.size = 0;
        }

        NetInterfaceId::NetInterfaceId(uint64_t localId): localId(localId)
        {
        }

        NetInterfaceId::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_UINT64;
            value.uint64 = &this->localId;

            return value;
        }

        NetInterfaceIndex::NetInterfaceIndex(ULONG index): index(index)
        {
        }

        NetInterfaceIndex::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_UINT32;
            value.uint32 = this->index;

            return value;
        }
    }
}
