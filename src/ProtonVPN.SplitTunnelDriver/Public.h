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

//
// Data structure passed to the callout in a provider context attached to the callout filter.
// Contains local IP address to bind to.
//
typedef struct CONNECT_REDIRECT_DATA
{
    IN_ADDR localAddress;
} CONNECT_REDIRECT_DATA;
