#pragma once

EXTERN_C_START

//
// The device context performs the same job as
// a WDM device extension in the driver frameworks
//
typedef struct _DEVICE_CONTEXT
{
    ULONG PrivateDeviceData; // just a placeholder
} DEVICE_CONTEXT, *PDEVICE_CONTEXT;

//
// This macro will generate an inline function called DeviceGetContext
// which will be used to get a pointer to the device context memory
// in a type safe manner.
//
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_CONTEXT, DeviceGetContext)

//
// Initialize the device
//
NTSTATUS
CreateDevice(
    _In_ WDFDRIVER Driver,
    _In_ WDFDEVICE* Device
);

//
// Delete the device
//
VOID
DeleteDevice(
    _In_ WDFDEVICE Device
);

EXTERN_C_END
