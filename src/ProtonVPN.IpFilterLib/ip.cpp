#include "pch.h"
#include <ws2tcpip.h>
#include <stdexcept>

#include "ip.h"

namespace ipfilter
{
    namespace ip
    {
        AddressV4::AddressV4(const AddressV4::BytesType& bytes):
            address({{bytes[0], bytes[1], bytes[2], bytes[3]}})
        {
        }

        AddressV4::AddressV4(): AddressV4({0, 0, 0, 0})
        {
        }

        AddressV4::BytesType AddressV4::toBytes() const
        {
            return AddressV4::BytesType({
                this->address.bytes[0],
                this->address.bytes[1],
                this->address.bytes[2],
                this->address.bytes[3]
            });
        }

        uint32_t AddressV4::uint32() const
        {
            return this->address.uint32;
        }

        AddressV4 AddressV4::loopback()
        {
            return AddressV4({127, 0, 0, 1});
        }

        AddressV4 AddressV4::broadcast()
        {
            return AddressV4({255, 255, 255, 255});
        }

        bool AddressV4::operator==(const AddressV4& other) const
        {
            if (&other == this)
            {
                return true;
            }

            if (other.address.uint32 == this->address.uint32)
            {
                return true;
            }
            return false;
        }

        AddressV4 makeAddressV4(const std::string& str)
        {
            IN_ADDR addr{};

            if (inet_pton(AF_INET, str.c_str(), &addr) != 1)
            {
                throw std::invalid_argument("Invalid format");
            }

            return AddressV4({
                addr.S_un.S_un_b.s_b1,
                addr.S_un.S_un_b.s_b2,
                addr.S_un.S_un_b.s_b3,
                addr.S_un.S_un_b.s_b4
            });
        }

        AddressV6::AddressV6() :
            AddressV6({ 0,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0 })
        {
        }

        AddressV6::AddressV6(const AddressV6::BytesType& bytes, uint8_t prefix) :
            address({ {
                bytes[0],  bytes[1],  bytes[2],  bytes[3],
                bytes[4],  bytes[5],  bytes[6],  bytes[7],
                bytes[8],  bytes[9],  bytes[10], bytes[11],
                bytes[12], bytes[13], bytes[14], bytes[15]
            } }),
            prefixLength(prefix)
        {
            if (prefix > 128)
            {
                throw std::out_of_range("IPv6 prefix must be 0-128");
            }
        }

        AddressV6::BytesType AddressV6::toBytes() const
        {
            return BytesType({
                address.bytes[0],  address.bytes[1],
                address.bytes[2],  address.bytes[3],
                address.bytes[4],  address.bytes[5],
                address.bytes[6],  address.bytes[7],
                address.bytes[8],  address.bytes[9],
                address.bytes[10], address.bytes[11],
                address.bytes[12], address.bytes[13],
                address.bytes[14], address.bytes[15]
            });
        }

        uint64_t AddressV6::high64() const { return address.uint64[0]; }
        uint64_t AddressV6::low64()  const { return address.uint64[1]; }

        bool AddressV6::operator==(const AddressV6& other) const
        {
            if (&other == this)
            {
                return true;
            }

            return address.uint64[0] == other.address.uint64[0] &&
                address.uint64[1] == other.address.uint64[1] &&
                prefixLength == other.prefixLength;
        }

        AddressV6 AddressV6::loopback() // ::1/128
        {
            return AddressV6({ 0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,
                               0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x01 });
        }

        AddressV6 AddressV6::broadcast() // ff02::1/128
        {
            return AddressV6({ 0xFF,0x02,0x00,0x00, 0x00,0x00,0x00,0x00,
                               0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x01 });
        }

        AddressV6 AddressV6::linkLocalRouterMulticast() // ff02::2/128
        {
            return AddressV6({ 0xFF,0x02,0x00,0x00, 0x00,0x00,0x00,0x00,
                               0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x02 });
        }

        AddressV6 AddressV6::linkLocal() // fe80::/10
        {
            return AddressV6({ 0xFE,0x80,0x00,0x00, 0x00,0x00,0x00,0x00,
                               0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00 }, 10);
        }

        AddressV6 AddressV6::linkLocalDhcpMulticast() // ff02::1:2/128
        {
            return AddressV6({ 0xFF,0x02,0x00,0x00, 0x00,0x00,0x00,0x00,
                               0x00,0x00,0x00,0x00, 0x00,0x01,0x00,0x02 });
        }

        AddressV6 AddressV6::siteLocalDhcpMulticast() // ff05::1:3/128
        {
            return AddressV6({ 0xFF,0x05,0x00,0x00, 0x00,0x00,0x00,0x00,
                               0x00,0x00,0x00,0x00, 0x00,0x01,0x00,0x03 });
        }

        AddressV6 makeAddressV6(const std::string& str, uint8_t prefix)
        {
            struct in6_addr addr6 {};
            int result = ::inet_pton(AF_INET6, str.c_str(), &addr6);

            if (result == 0)
            {
                throw std::invalid_argument("Invalid IPv6 string: " + str);
            }
            else if (result < 0)
            {
                throw std::out_of_range("inet_pton(AF_INET6) failed");
            }

            AddressV6::BytesType bytes{};
            std::copy(std::begin(addr6.s6_addr), std::end(addr6.s6_addr), bytes.begin());

            return AddressV6(bytes, prefix);
        }
    }
}
