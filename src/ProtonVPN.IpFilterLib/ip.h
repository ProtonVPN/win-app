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
    }
}
