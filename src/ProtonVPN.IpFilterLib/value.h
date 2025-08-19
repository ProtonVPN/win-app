#pragma once
#include <fwptypes.h>

#include "buffer.h"
#include "ip.h"

namespace ipfilter
{
    namespace value
    {
        class Value
        {
        public:
            virtual operator FWP_CONDITION_VALUE() = 0;
        };

        class IpAddressV4 : public Value
        {
        public:
            IpAddressV4(const ip::AddressV4& addr);

            virtual operator FWP_CONDITION_VALUE();

        private:
            ip::AddressV4 addr;
        };

        class IpNetworkAddressV4 : public Value
        {
        public:
            IpNetworkAddressV4(const ip::AddressV4& addr, const ip::AddressV4& mask);

            virtual operator FWP_CONDITION_VALUE();

        private:
            FWP_V4_ADDR_AND_MASK addr;
        };

        class IpAddressV6 : public Value
        {
        public:
            IpAddressV6(const ip::AddressV6& addr);

            virtual operator FWP_CONDITION_VALUE();

        private:
            ip::AddressV6 addr;
        };

        class IpAddressV6WithPrefix : public Value
        {
        public:
            IpAddressV6WithPrefix(const ip::AddressV6& addr);

            virtual operator FWP_CONDITION_VALUE();

        private:
            ip::AddressV6 addr;
        };

        class Port : public Value
        {
        public:
            Port(unsigned short number);

            virtual operator FWP_CONDITION_VALUE();

        private:
            unsigned short number;
        };

        class IcmpCode : public Value
        {
        public:
            IcmpCode(unsigned short code);

            virtual operator FWP_CONDITION_VALUE();

        private:
            unsigned short code;
        };

        class IcmpType : public Value
        {
        public:
            IcmpType(unsigned short type);

            virtual operator FWP_CONDITION_VALUE();

        private:
            unsigned short type;
        };

        class IcmpProtocol : public Value
        {
        public:
            IcmpProtocol();

            virtual operator FWP_CONDITION_VALUE();
        };

        class TcpProtocol : public Value
        {
        public:
            virtual operator FWP_CONDITION_VALUE();

            static TcpProtocol udp();

            static TcpProtocol tcp();

        private:
            TcpProtocol(uint8_t protocol);

            uint8_t protocol;
        };

        class Flag : public Value
        {
        public:
            virtual operator FWP_CONDITION_VALUE();

            static Flag loopback();

        private:
            Flag(uint32_t flag);

            uint32_t flag;
        };

        class ApplicationId : public Value
        {
        public:
            virtual operator FWP_CONDITION_VALUE();

            static ApplicationId fromFilePath(const std::wstring& path);

            ApplicationId(const ApplicationId& other);

            ApplicationId(ApplicationId&& other);

        private:
            ApplicationId(const FWP_BYTE_BLOB& blob);

            Buffer value;
            FWP_BYTE_BLOB blob;
        };

        class NetInterfaceId : public Value
        {
        public:
            NetInterfaceId(uint64_t localId);

            virtual operator FWP_CONDITION_VALUE();

        private:
            uint64_t localId;
        };

        class NetInterfaceIndex : public Value
        {
        public:
            NetInterfaceIndex(ULONG index);

            virtual operator FWP_CONDITION_VALUE();

        private:
            ULONG index;
        };
    }
}
