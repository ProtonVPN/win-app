#include "pch.h"
#include <fwpmu.h>
#include <stdexcept>
#include <winsock.h>

#include "value.h"

#ifndef IPPROTO_ICMPV6
#define IPPROTO_ICMPV6 58
#endif

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

        IpAddressV6::IpAddressV6(const ip::AddressV6& addr) : addr(addr)
        {
        }

        IpAddressV6::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};
            value.type = FWP_BYTE_ARRAY16_TYPE;

            auto bytes = addr.toBytes();
            auto* holder = new FWP_BYTE_ARRAY16{};

            std::memcpy(holder->byteArray16, bytes.data(), 16);

            value.byteArray16 = holder;

            return value;
        }

        IpAddressV6WithPrefix::IpAddressV6WithPrefix(const ip::AddressV6& addr) : addr(addr)
        {
        }

        IpAddressV6WithPrefix::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_V6_ADDR_MASK;
            
            auto* mask = new FWP_V6_ADDR_AND_MASK{};
            auto bytes = addr.toBytes();
            std::memcpy(mask->addr, bytes.data(), 16);
            mask->prefixLength = addr.prefix();

            value.v6AddrMask = mask;

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

        IcmpCode::IcmpCode(unsigned short code) : code(code)
        {
        }

        IcmpCode::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_UINT16;
            value.uint16 = this->code;

            return value;
        }

        IcmpType::IcmpType(unsigned short type) : type(type)
        {
        }

        IcmpType::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_UINT16;
            value.uint16 = this->type;

            return value;
        }

        IcmpProtocol::IcmpProtocol()
        {
        }

        IcmpProtocol::operator FWP_CONDITION_VALUE()
        {
            FWP_CONDITION_VALUE value{};

            value.type = FWP_UINT8;
            value.uint8 = IPPROTO_ICMPV6;

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
