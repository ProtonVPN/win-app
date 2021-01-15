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
            virtual operator FWP_CONDITION_VALUE0() = 0;
        };

        class IpAddressV4 : public Value
        {
        public:
            IpAddressV4(const ip::AddressV4& addr);

            virtual operator FWP_CONDITION_VALUE0();

        private:
            ip::AddressV4 addr;
        };

        class IpNetworkAddressV4 : public Value
        {
        public:
            IpNetworkAddressV4(const ip::AddressV4& addr, const ip::AddressV4& mask);

            virtual operator FWP_CONDITION_VALUE0();

        private:
            FWP_V4_ADDR_AND_MASK addr;
        };

        class Port : public Value
        {
        public:
            Port(unsigned short number);

            virtual operator FWP_CONDITION_VALUE0();

        private:
            unsigned short number;
        };

        class TcpProtocol : public Value
        {
        public:
            virtual operator FWP_CONDITION_VALUE0();

            static TcpProtocol udp();

            static TcpProtocol tcp();

        private:
            TcpProtocol(uint8_t protocol);

            uint8_t protocol;
        };

        class Flag : public Value
        {
        public:
            virtual operator FWP_CONDITION_VALUE0();

            static Flag loopback();

        private:
            Flag(uint32_t flag);

            uint32_t flag;
        };

        class ApplicationId : public Value
        {
        public:
            virtual operator FWP_CONDITION_VALUE0();

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

            virtual operator FWP_CONDITION_VALUE0();

        private:
            uint64_t localId;
        };

        class NetInterfaceIndex : public Value
        {
        public:
            NetInterfaceIndex(ULONG index);

            virtual operator FWP_CONDITION_VALUE0();

        private:
            ULONG index;
        };
    }
}
