#pragma once

EXTERN_C_START

//
// Registers the callout
//
NTSTATUS
RegisterCallout(
    _In_ PDEVICE_OBJECT deviceObject
);

//
// Unregisters the callout
//
NTSTATUS
UnregisterCallout(
);

EXTERN_C_END
