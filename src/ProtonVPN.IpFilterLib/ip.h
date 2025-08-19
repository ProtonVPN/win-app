#pragma once
#include <array>
#include <string>
#include <cstdint>

namespace ipfilter
{
    namespace ip
    {
        class AddressV4
        {
        public:
            typedef std::array<unsigned char, 4> BytesType;

            AddressV4();

            AddressV4(const BytesType& bytes);

            BytesType toBytes() const;

            uint32_t uint32() const;

            bool operator==(const AddressV4& other) const;

            static AddressV4 loopback();

            static AddressV4 broadcast();

        private:
            union
            {
                unsigned char bytes[4];
                uint32_t uint32;
            } address;
        };

        AddressV4 makeAddressV4(const std::string& str);

        class AddressV6
        {
        public:
            typedef std::array<unsigned char, 16> BytesType;

            AddressV6();

            AddressV6(const BytesType& bytes, uint8_t prefix = 128);

            BytesType toBytes() const;

            uint64_t high64() const;

            uint64_t low64() const;

            uint8_t prefix() const { return prefixLength; }

            bool operator==(const AddressV6& other) const;

            static AddressV6 loopback();

            static AddressV6 broadcast();

            static AddressV6 linkLocalRouterMulticast();

            static AddressV6 linkLocal();

            static AddressV6 linkLocalDhcpMulticast();

            static AddressV6 siteLocalDhcpMulticast();

        private:
            union
            {
                unsigned char bytes[16];
                uint64_t      uint64[2];
            } address{};

            uint8_t prefixLength{128};
        };

        AddressV6 makeAddressV6(const std::string& str, uint8_t prefix);
    }
}
