#include "StdAfx.h"
#include "IpAddress.h"
#include <ip2string.h>

IpAddress::IpAddress(PCWSTR address) : address(address)
{
    UINT16 port = 0;
    IN_ADDR v4Addr = {0};
    pIPv4Address = 0;
    auto status = NO_ERROR;

    status = RtlIpv4StringToAddressEx(address, FALSE, &v4Addr, &port);

    CopyMemory(&pIPv4Address, &v4Addr, 4);
}

UINT32 IpAddress::IPv4()
{
    return ntohl(pIPv4Address);
}

UINT32 IpAddress::ReversedIPv4()
{
    return htonl(this->IPv4());
}

IpAddress::~IpAddress()
= default;
