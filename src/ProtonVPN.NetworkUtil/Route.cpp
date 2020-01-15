#include "StdAfx.h"
#include "Route.h"

Route::Route()
= default;

PMIB_IPFORWARDTABLE Route::getForwardTable()
{
    PMIB_IPFORWARDTABLE pIpForwardTable = nullptr;
    DWORD size = 0;
    DWORD status = GetIpForwardTable(pIpForwardTable, &size, FALSE);
    if (status == ERROR_INSUFFICIENT_BUFFER)
    {
        if (!((pIpForwardTable = static_cast<PMIB_IPFORWARDTABLE>(malloc(size)))))
            return pIpForwardTable;

        status = GetIpForwardTable(pIpForwardTable, &size, FALSE);
    }

    if (status != ERROR_SUCCESS && pIpForwardTable)
        free(pIpForwardTable);

    return pIpForwardTable;
}

DWORD Route::Add(IpAddress destination, IpAddress mask)
{
    PMIB_IPFORWARDROW pRow = nullptr;
    auto pIpForwardTable = getForwardTable();

    for (unsigned int i = 0; i < pIpForwardTable->dwNumEntries; i++)
    {
        if (pIpForwardTable->table[i].dwForwardDest == 0 && pIpForwardTable->table[i].dwForwardMask == 0)
        {
            if (!pRow)
            {
                pRow = static_cast<PMIB_IPFORWARDROW>(malloc(sizeof(MIB_IPFORWARDROW)));
                memcpy(pRow, &pIpForwardTable->table[i], sizeof(MIB_IPFORWARDROW));
            }
        }
    }

    pRow->dwForwardDest = destination.ReversedIPv4();
    pRow->dwForwardMask = mask.ReversedIPv4();
    pRow->dwForwardAge = INFINITE;

    const auto status = CreateIpForwardEntry(pRow);

    if (pIpForwardTable)
        free(pIpForwardTable);
    if (pRow)
        free(pRow);

    return status;
}

DWORD Route::Delete(IpAddress destination, IpAddress mask)
{
    DWORD status = 0;
    auto pIpForwardTable = getForwardTable();

    for (unsigned int i = 0; i < pIpForwardTable->dwNumEntries; i++)
    {
        auto route = pIpForwardTable->table[i];
        if (route.dwForwardDest == destination.ReversedIPv4() && route.dwForwardMask == mask.ReversedIPv4())
        {
            status = DeleteIpForwardEntry(&route);
            break;
        }
    }

    if (pIpForwardTable)
        free(pIpForwardTable);

    return status;
}

Route::~Route()
= default;
