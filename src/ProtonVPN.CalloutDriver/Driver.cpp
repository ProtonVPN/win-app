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
HANDLE injectHandle = nullptr;
NDIS_HANDLE nbl_pool_handle = nullptr;

NTSTATUS
DriverEntry(
    _In_ PDRIVER_OBJECT DriverObject,
    _In_ PUNICODE_STRING RegistryPath
)
{
    //https://docs.microsoft.com/en-us/windows-hardware/drivers/kernel/single-binary-opt-in-pool-nx-optin
    ExInitializeDriverRuntime(DrvRtPoolNxOptIn);

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

    status = FwpsInjectionHandleCreate(AF_INET, FWPS_INJECTION_TYPE_NETWORK, &injectHandle);
    if (!NT_SUCCESS(status))
    {
        WPP_CLEANUP(DriverObject);
        return status;
    }

    status = RegisterCallout(deviceObject, BLOCK_DNS_CALLOUT_KEY, BlockDnsBySendingServerFailPacket);
    if (!NT_SUCCESS(status))
    {
        WPP_CLEANUP(DriverObject);
        return status;
    }

    NET_BUFFER_LIST_POOL_PARAMETERS nbl_pool_params;

    RtlZeroMemory(&nbl_pool_params, sizeof(nbl_pool_params));
    nbl_pool_params.Header.Type = NDIS_OBJECT_TYPE_DEFAULT;
    nbl_pool_params.Header.Revision = NET_BUFFER_LIST_POOL_PARAMETERS_REVISION_1;
    nbl_pool_params.Header.Size = sizeof(nbl_pool_params);
    nbl_pool_params.fAllocateNetBuffer = TRUE;
    nbl_pool_params.PoolTag = ProtonTAG;
    nbl_pool_params.DataSize = 0;
    nbl_pool_handle = NdisAllocateNetBufferListPool(nullptr, &nbl_pool_params);
    if (nbl_pool_handle == nullptr)
    {
        WPP_CLEANUP(DriverObject);
        return STATUS_INSUFFICIENT_RESOURCES;
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
    UnregisterCallout(BLOCK_DNS_CALLOUT_KEY);

    if (injectHandle != nullptr)
    {
        FwpsInjectionHandleDestroy0(injectHandle);
    }

    if (nbl_pool_handle != nullptr)
    {
        NdisFreeNetBufferListPool(nbl_pool_handle);
    }

    // Stop WPP Tracing
    WPP_CLEANUP(WdfDriverWdmGetDriverObject(Driver));

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_DRIVER, "%!FUNC! Exit");
}
