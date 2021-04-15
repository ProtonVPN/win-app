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
    }
}
