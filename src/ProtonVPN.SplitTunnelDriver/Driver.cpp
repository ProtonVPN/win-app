#include <ntddk.h>
#include <wdf.h>

#include "Trace.h"
#include "Driver.h"
#include "Driver.tmh"

#include "Device.h"
#include "Callout.h"
#include "Public.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text (INIT, DriverEntry)
#pragma alloc_text (PAGE, DriverUnload)
#endif

WDFDEVICE Device = NULL;

NTSTATUS
DriverEntry(
    _In_ PDRIVER_OBJECT DriverObject,
    _In_ PUNICODE_STRING RegistryPath
)
{
    WDF_DRIVER_CONFIG config;
    WDFDRIVER driver;
    NTSTATUS status;

    // Initialize WPP Tracing
    WPP_INIT_TRACING(DriverObject, RegistryPath);

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Entry");

    // Initialize the driver config structure
    WDF_DRIVER_CONFIG_INIT(&config, NULL);

    // Indicate that this is a non-PNP driver
    config.DriverInitFlags = WdfDriverInitNonPnpDriver;

    // Specify the callout driver's Unload function
    config.EvtDriverUnload = DriverUnload;

    status = WdfDriverCreate(DriverObject,
                             RegistryPath,
                             WDF_NO_OBJECT_ATTRIBUTES,
                             &config,
                             &driver
    );

    if (!NT_SUCCESS(status))
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_DRIVER, "%!FUNC! WdfDriverCreate failed %!STATUS!", status);
        WPP_CLEANUP(DriverObject);
        return status;
    }

    status = CreateDevice(driver, &Device);

    if (!NT_SUCCESS(status))
    {
        WPP_CLEANUP(DriverObject);
        return status;
    }

    // Get the associated WDM device object
    PDEVICE_OBJECT deviceObject = WdfDeviceWdmGetDeviceObject(Device);

    status = RegisterCallout(deviceObject, CONNECT_REDIRECT_CALLOUT_KEY, RedirectConnection);
    if (!NT_SUCCESS(status))
    {
        WPP_CLEANUP(DriverObject);
        return status;
    }

    status = RegisterCallout(deviceObject, REDIRECT_UDP_CALLOUT_KEY, RedirectUDPFlow);
    if (!NT_SUCCESS(status))
    {
        WPP_CLEANUP(DriverObject);
        return status;
    }

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Exit");

    return status;
}

_Use_decl_annotations_
VOID
DriverUnload(
    _In_ WDFDRIVER Driver
)
{
    PAGED_CODE();

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Entry");

    UnregisterCallout(CONNECT_REDIRECT_CALLOUT_KEY);

    UnregisterCallout(REDIRECT_UDP_CALLOUT_KEY);

    // Stop WPP Tracing
    WPP_CLEANUP(WdfDriverWdmGetDriverObject(Driver));

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Exit");
}
