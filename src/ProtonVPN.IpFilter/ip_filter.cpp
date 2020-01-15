#include "pch.h"
#include "ip_filter.h"
#include "filter.h"
#include "guid.h"
#include "buffer.h"

#include <fwptypes.h>
#include <fwpmu.h>

unsigned int IPFilterStartTransaction(IPFilterSessionHandle* handle)
{
    return FwpmTransactionBegin0(handle, 0);
}

unsigned int IPFilterAbortTransaction(IPFilterSessionHandle* handle)
{
    return FwpmTransactionAbort0(handle);
}

unsigned int IPFilterCommitTransaction(IPFilterSessionHandle* handle)
{
    return FwpmTransactionCommit0(handle);
}

unsigned int IPFilterCreateDynamicSession(
    IPFilterSessionHandle* handle)
{
    FWPM_SESSION0 session{};
    session.flags = FWPM_SESSION_FLAG_DYNAMIC;

    return FwpmEngineOpen0(
        nullptr,
        RPC_C_AUTHN_WINNT,
        nullptr,
        &session,
        handle);
}

unsigned int IPFilterDestroySession(
    IPFilterSessionHandle handle)
{
    return FwpmEngineClose0(handle);
}

unsigned int IPFilterCreateProvider(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    GUID* providerKey)
{
    FWPM_PROVIDER0 provider{};

    provider.providerKey = ipfilter::guid::makeGuid(providerKey);
    provider.displayData.name = const_cast<wchar_t *>(displayData->name);
    provider.displayData.description = const_cast<wchar_t *>(displayData->description);

    auto result = FwpmProviderAdd0(
        const_cast<void *>(sessionHandle),
        &provider,
        nullptr);

    if (result == ERROR_SUCCESS)
    {
        *providerKey = provider.providerKey;
    }

    return result;
}

unsigned int IPFilterCreateProviderContext(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    GUID* providerKey,
    unsigned int size,
    UINT8* data,
    GUID* providerContextKey)
{
    FWPM_PROVIDER_CONTEXT0 context{};

    context.providerContextKey = ipfilter::guid::makeGuid(providerContextKey);
    context.providerKey = providerKey;
    context.displayData.name = const_cast<wchar_t*>(displayData->name);
    context.displayData.description = const_cast<wchar_t*>(displayData->description);

    context.type = FWPM_GENERAL_CONTEXT;
    context.dataBuffer = new FWP_BYTE_BLOB{};
    context.dataBuffer->size = size;
    context.dataBuffer->data = data;

    UINT64 id;

    auto result = FwpmProviderContextAdd0(
        const_cast<void*>(sessionHandle),
        &context,
        nullptr,
        &id);

    delete context.dataBuffer;

    if (result == ERROR_SUCCESS)
    {
        *providerContextKey = context.providerContextKey;
    }

    return result;
}

unsigned int IPFilterDestroyProviderContext(
    IPFilterSessionHandle sessionHandle,
    GUID* providerContextKey)
{
    return FwpmProviderContextDeleteByKey0(sessionHandle, providerContextKey);
}

unsigned int IPFilterCreateCallout(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    GUID* providerKey,
    unsigned int layer,
    GUID* calloutKey)
{
    GUID layerKey{};
    IPFilterGetLayerKey(layerKey, layer);

    FWPM_CALLOUT0 callout{};

    callout.calloutKey = ipfilter::guid::makeGuid(calloutKey);
    callout.providerKey = providerKey;
    callout.applicableLayer = layerKey;
    callout.flags = FWPM_CALLOUT_FLAG_USES_PROVIDER_CONTEXT;
    callout.displayData.name = const_cast<wchar_t*>(displayData->name);
    callout.displayData.description = const_cast<wchar_t*>(displayData->description);

    UINT32 id;

    auto result = FwpmCalloutAdd0(
        const_cast<void*>(sessionHandle),
        &callout,
        nullptr,
        &id);

    if (result == ERROR_SUCCESS)
    {
        *calloutKey = callout.calloutKey;
    }

    return result;
}

unsigned int IPFilterCreateSublayer(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    const IPFilterDisplayData* displayData,
    unsigned int weight,
    GUID* sublayerKey)
{
    FWPM_SUBLAYER0 sublayer{};
    sublayer.subLayerKey = ipfilter::guid::makeGuid(sublayerKey);
    sublayer.providerKey = providerKey;
    sublayer.displayData.name = const_cast<wchar_t *>(displayData->name);
    sublayer.displayData.description = const_cast<wchar_t *>(displayData->description);
    sublayer.weight = weight;

    auto result = FwpmSubLayerAdd0(
        sessionHandle,
        &sublayer,
        nullptr);

    if (result == ERROR_SUCCESS)
    {
        *sublayerKey = sublayer.subLayerKey;
    }

    return result;
}

unsigned int IPFilterDestroySublayer(
    IPFilterSessionHandle sessionHandle,
    GUID* subLayerKey)
{
    return FwpmSubLayerDeleteByKey0(sessionHandle, subLayerKey);
}
