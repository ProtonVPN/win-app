/*++

Module Name:

    public.h

Abstract:

    This module contains the common declarations shared by driver
    and user applications.

Environment:

    User and kernel.

--*/
#pragma once

#include <initguid.h>

//
// A GUID that uniquely identifies the Split Tunnel redirect callout.
// {3c5a284f-af01-51fa-4361-6c6c50424144}
//
DEFINE_GUID(CONNECT_REDIRECT_CALLOUT_KEY,
            0x3c5a284f, 0xaf01, 0x51fa, 0x43, 0x61, 0x6c, 0x6c, 0x50, 0x42, 0x41, 0x44);

//{10636af3-50d6-4f53-acb7-d5af33217fca}
DEFINE_GUID(REDIRECT_UDP_CALLOUT_KEY,
    0x10636af3, 0x50d6, 0x4f53, 0xac, 0xb7, 0xd5, 0xaf, 0x33, 0x21, 0x7f, 0xca);

//{10636af3-50d6-4f53-acb7-d5af33217fcb}
DEFINE_GUID(BLOCK_DNS_CALLOUT_KEY,
    0x10636af3, 0x50d6, 0x4f53, 0xac, 0xb7, 0xd5, 0xaf, 0x33, 0x21, 0x7f, 0xcb);

#define ProtonTAG 'pvpn'

//
// Data structure passed to the callout in a provider context attached to the callout filter.
// Contains local IP address to bind to.
//
typedef struct CONNECT_REDIRECT_DATA
{
    IN_ADDR localAddress;
} CONNECT_REDIRECT_DATA;

extern HANDLE injectHandle;
extern NDIS_HANDLE nbl_pool_handle;