#ifndef PROTON_NETWORK_UTIL_ROUTE_H
#define PROTON_NETWORK_UTIL_ROUTE_H

#include <Windows.h>
#include <Iphlpapi.h>
#include "IpAddress.h"

class Route
{
private:
    PMIB_IPFORWARDTABLE getForwardTable();
public:
    Route();
    DWORD Add(IpAddress destination, IpAddress mask);
    DWORD Delete(IpAddress destination, IpAddress mask);
    ~Route();
};

#endif
