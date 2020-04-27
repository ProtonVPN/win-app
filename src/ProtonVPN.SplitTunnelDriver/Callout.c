#include <fwpsk.h>
#include <fwpmtypes.h>
#include <mstcpip.h>

#include "Trace.h"
#include "Callout.h"
#include "Callout.tmh"

#include "Public.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text (INIT, RegisterCallout)
#pragma alloc_text (PAGE, UnregisterCallout)
#endif

BOOL isLoopbackConnection(const FWPS_INCOMING_VALUES* inValues)
{
    auto localAddr = RtlUlongByteSwap(inValues->incomingValue[
        FWPS_FIELD_ALE_CONNECT_REDIRECT_V4_IP_LOCAL_ADDRESS].value.uint32);
    auto remoteAddr = RtlUlongByteSwap(inValues->incomingValue[
        FWPS_FIELD_ALE_CONNECT_REDIRECT_V4_IP_REMOTE_ADDRESS].value.uint32);

    if (IN4_IS_ADDR_LOOPBACK((IN_ADDR*)(&localAddr)))
    {
        return TRUE;
    }

    if (IN4_IS_ADDR_LOOPBACK((IN_ADDR*)(&remoteAddr)))
    {
        return TRUE;
    }

    return FALSE;
}

//
// The callout ClassifyFn1 function
//
VOID NTAPI
ClassifyFn1(
    IN const FWPS_INCOMING_VALUES0* inFixedValues,
    IN const FWPS_INCOMING_METADATA_VALUES0* inMetaValues,
    IN OUT VOID* layerData,
    IN const void* classifyContext,
    IN const FWPS_FILTER1* filter,
    IN UINT64 flowContext,
    IN OUT FWPS_CLASSIFY_OUT0* classifyOut
)
{
    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Entry");

    classifyOut->actionType = FWP_ACTION_PERMIT;

    UNREFERENCED_PARAMETER(inFixedValues);
    UNREFERENCED_PARAMETER(inMetaValues);
    UNREFERENCED_PARAMETER(layerData);
    UNREFERENCED_PARAMETER(flowContext);

    if (inFixedValues == NULL)
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! inFixedValues is NULL");

        return;
    }

    if (classifyContext == NULL)
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! classifyContext is NULL");

        return;
    }

    if (filter == NULL)
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! filter is NULL");

        return;
    }

    if (inFixedValues->layerId != FWPS_LAYER_ALE_CONNECT_REDIRECT_V4)
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! Invalid layer");

        return;
    }

    if (isLoopbackConnection(inFixedValues))
    {
        return;
    }

    if (filter->providerContext == NULL)
    {
        TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Provider context not specified");

        return;
    }

    if (filter->providerContext->type != FWPM_GENERAL_CONTEXT)
    {
        TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT,
                    "%!FUNC! Provider context type is not FWPM_GENERAL_CONTEXT");

        return;
    }

    if (filter->providerContext->dataBuffer == NULL)
    {
        TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Provider context data buffer is NULL");

        return;
    }

    auto size = filter->providerContext->dataBuffer->size;
    if (size != sizeof(CONNECT_REDIRECT_DATA))
    {
        TraceEvents(
            TRACE_LEVEL_ERROR,
            TRACE_CALLOUT,
            "%!FUNC! Provider context data size is %i instead of expected %i",
            size,
            sizeof(CONNECT_REDIRECT_DATA));

        return;
    }

    if (filter->providerContext->dataBuffer->data == NULL)
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! Provider context data not specified");

        return;
    }

    NTSTATUS status = STATUS_SUCCESS;
    UINT64 classifyHandle = 0;
    FWPS_CONNECT_REQUEST* writableLayerData = NULL;

    status = FwpsAcquireClassifyHandle(
        (void*)classifyContext,
        0,
        &classifyHandle);

    if (status != STATUS_SUCCESS)
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! FwpsAcquireClassifyHandle failed %!STATUS!", status);

        return;
    }

    status = FwpsAcquireWritableLayerDataPointer(
        classifyHandle,
        filter->filterId,
        0,
        (PVOID*)(&writableLayerData),
        classifyOut);

    if (status != STATUS_SUCCESS)
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! FwpsAcquireWritableLayerDataPointer failed %!STATUS!",
                    status);

        goto Exit;
    }

    for (FWPS_CONNECT_REQUEST* connectRequest = writableLayerData->previousVersion;
        connectRequest != NULL;
        connectRequest = connectRequest->previousVersion)
    {
        if (connectRequest->modifierFilterId == filter->filterId)
        {
            // Don't redirect the same socket more than once

            TraceEvents(TRACE_LEVEL_WARNING, TRACE_CALLOUT, "%!FUNC! Already redirected");

            goto Exit;
        }
    }

    CONNECT_REDIRECT_DATA* redirectData = (CONNECT_REDIRECT_DATA*)filter->providerContext->dataBuffer->data;

    INETADDR_SET_ADDRESS((PSOCKADDR)&(writableLayerData->localAddressAndPort),
                         &(redirectData->localAddress.S_un.S_un_b.s_b1));

Exit:

    if (writableLayerData != NULL)
    {
        FwpsApplyModifiedLayerData(
            classifyHandle,
            (PVOID*)(&writableLayerData),
            0);
    }

    if (classifyHandle != 0)
    {
        FwpsReleaseClassifyHandle0(classifyHandle);
    }

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Exit");
}

//
// The callout NotifyFn1 function
//
NTSTATUS NTAPI
NotifyFn1(
    IN FWPS_CALLOUT_NOTIFY_TYPE notifyType,
    IN const GUID* filterKey,
    IN const FWPS_FILTER1* filter
)
{
    UNREFERENCED_PARAMETER(notifyType);
    UNREFERENCED_PARAMETER(filterKey);
    UNREFERENCED_PARAMETER(filter);

    return STATUS_SUCCESS;
}

//
// Registers the callout
//
NTSTATUS
RegisterCallout(
    _In_ PDEVICE_OBJECT deviceObject
)
{
    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Entry");

    NTSTATUS status;
    UINT32 CalloutId;

    // Callout registration structure
    FWPS_CALLOUT1 callout = {0};
    callout.calloutKey = CONNECT_REDIRECT_CALLOUT_KEY;
    callout.flags = 0;
    callout.classifyFn = ClassifyFn1;
    callout.notifyFn = NotifyFn1;
    callout.flowDeleteFn = NULL;

    status = FwpsCalloutRegister1(
        deviceObject,
        &callout,
        &CalloutId
    );

    if (!NT_SUCCESS(status))
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! FwpsCalloutRegister1 failed %!STATUS!", status);
        return status;
    }

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Exit");

    return status;
}

//
// Unregisters the callout
//
NTSTATUS
UnregisterCallout()
{
    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Entry");

    NTSTATUS status;

    status = FwpsCalloutUnregisterByKey0(&CONNECT_REDIRECT_CALLOUT_KEY);

    if (!NT_SUCCESS(status))
    {
        TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! FwpsCalloutUnregisterByKey0 failed %!STATUS!", status);
    }

    TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Exit");

    return status;
}
