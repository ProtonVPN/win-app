#include "pch.h"
#include "ip_filter.h"
#include "filter.h"
#include "guid.h"
#include "buffer.h"

#include <fwptypes.h>
#include <fwpmu.h>

unsigned int IPFilterStartTransaction(IPFilterSessionHandle handle)
{
    return FwpmTransactionBegin(handle, 0);
}

unsigned int IPFilterAbortTransaction(IPFilterSessionHandle handle)
{
    return FwpmTransactionAbort(handle);
}

unsigned int IPFilterCommitTransaction(IPFilterSessionHandle handle)
{
    return FwpmTransactionCommit(handle);
}

unsigned int IPFilterCreateDynamicSession(
    IPFilterSessionHandle* handle)
{
    FWPM_SESSION session{};
    session.flags = FWPM_SESSION_FLAG_DYNAMIC;

    return FwpmEngineOpen(
        nullptr,
        RPC_C_AUTHN_WINNT,
        nullptr,
        &session,
        handle);
}

unsigned int IPFilterCreateSession(IPFilterSessionHandle* handle)
{
    return FwpmEngineOpen(
        nullptr,
        RPC_C_AUTHN_WINNT,
        nullptr,
        nullptr,
        handle);
}

unsigned int IPFilterDestroySession(
    IPFilterSessionHandle handle)
{
    return FwpmEngineClose(handle);
}

unsigned int IPFilterCreateProvider(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    BOOL persistent,
    GUID* providerKey)
{
    FWPM_PROVIDER provider{};

    provider.providerKey = ipfilter::guid::makeGuid(providerKey);
    provider.displayData.name = const_cast<wchar_t *>(displayData->name);
    provider.displayData.description = const_cast<wchar_t *>(displayData->description);

    if (persistent)
    {
        provider.flags |= FWPM_PROVIDER_FLAG_PERSISTENT;
    }

    auto result = FwpmProviderAdd(
        const_cast<void *>(sessionHandle),
        &provider,
        nullptr);

    if (result == ERROR_SUCCESS)
    {
        *providerKey = provider.providerKey;
    }

    return result;
}

unsigned int IPFilterIsProviderRegistered(
    IPFilterSessionHandle sessionHandle,
    const GUID* providerKey,
    unsigned int* result)
{
    FWPM_PROVIDER* provider{};

    auto status = FwpmProviderGetByKey(sessionHandle, providerKey, &provider);
    if (status == FWP_E_PROVIDER_NOT_FOUND)
    {
        status = ERROR_SUCCESS;
        *result = 1;
    }
    else if (status == ERROR_SUCCESS)
    {
        *result = 0;
    }

    if (provider != nullptr)
    {
        FwpmFreeMemory(reinterpret_cast<void**>(&provider));
    }

    return status;
}

unsigned int IPFilterDestroyProvider(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey)
{
    auto result = FwpmProviderDeleteByKey(sessionHandle, providerKey);
    if (result == FWP_E_PROVIDER_NOT_FOUND)
    {
        return ERROR_SUCCESS;
    }

    return result;
}

unsigned int IPFilterCreateProviderContext(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    GUID* providerKey,
    unsigned int size,
    UINT8* data,
    BOOL persistent,
    GUID* providerContextKey)
{
    FWPM_PROVIDER_CONTEXT context{};

    context.providerContextKey = ipfilter::guid::makeGuid(providerContextKey);
    context.providerKey = providerKey;
    context.displayData.name = const_cast<wchar_t*>(displayData->name);
    context.displayData.description = const_cast<wchar_t*>(displayData->description);

    context.type = FWPM_GENERAL_CONTEXT;
    context.dataBuffer = new FWP_BYTE_BLOB{};
    context.dataBuffer->size = size;
    context.dataBuffer->data = data;

    if (persistent)
    {
        context.flags |= FWPM_PROVIDER_CONTEXT_FLAG_PERSISTENT;
    }

    UINT64 id;

    auto result = FwpmProviderContextAdd(
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
    return FwpmProviderContextDeleteByKey(sessionHandle, providerContextKey);
}

unsigned int IPFilterCreateCallout(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    GUID* providerKey,
    unsigned int layer,
    BOOL persistent,
    GUID* calloutKey)
{
    GUID layerKey{};
    IPFilterGetLayerKey(layerKey, layer);

    FWPM_CALLOUT callout{};

    callout.calloutKey = ipfilter::guid::makeGuid(calloutKey);
    callout.providerKey = providerKey;
    callout.applicableLayer = layerKey;
    callout.flags = FWPM_CALLOUT_FLAG_USES_PROVIDER_CONTEXT;
    callout.displayData.name = const_cast<wchar_t*>(displayData->name);
    callout.displayData.description = const_cast<wchar_t*>(displayData->description);

    if (persistent)
    {
        callout.flags |= FWPM_CALLOUT_FLAG_PERSISTENT;
    }

    UINT32 id;

    auto result = FwpmCalloutAdd(
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

unsigned int IPFilterDestroyCallout(
    IPFilterSessionHandle sessionHandle,
    GUID* calloutKey)
{
    return FwpmCalloutDeleteByKey(sessionHandle, calloutKey);
}

unsigned int IPFilterCreateSublayer(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    const IPFilterDisplayData* displayData,
    unsigned int weight,
    BOOL persistent,
    GUID* sublayerKey)
{
    FWPM_SUBLAYER sublayer{};
    sublayer.subLayerKey = ipfilter::guid::makeGuid(sublayerKey);
    sublayer.providerKey = providerKey;
    sublayer.displayData.name = const_cast<wchar_t *>(displayData->name);
    sublayer.displayData.description = const_cast<wchar_t *>(displayData->description);
    sublayer.weight = weight;

    if (persistent)
    {
        sublayer.flags |= FWPM_SUBLAYER_FLAG_PERSISTENT;
    }

    auto result = FwpmSubLayerAdd(
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
    auto result = FwpmSubLayerDeleteByKey(sessionHandle, subLayerKey);
    if (result == FWP_E_SUBLAYER_NOT_FOUND)
    {
        return ERROR_SUCCESS;
    }

    return result;
}

unsigned int IPFilterDestroyFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* filterKey)
{
    return FwpmFilterDeleteByKey(sessionHandle, filterKey);
}

unsigned int IPFilterGetSublayerFilterCount(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    unsigned int* result)
{
    HANDLE enumHandle = nullptr;

    auto status = FwpmFilterCreateEnumHandle(sessionHandle,
        nullptr,
        &enumHandle);
    if (status != ERROR_SUCCESS)
    {
        return status;
    }

    while (true)
    {
        FWPM_FILTER** filters{};
        UINT32 filterCount{};

        status = FwpmFilterEnum(sessionHandle, enumHandle, 1, &filters, &filterCount);
        if (status != ERROR_SUCCESS || filterCount == 0)
        {
            break;
        }

        for (UINT32 i = 0; i < filterCount; i++)
        {
            FWPM_FILTER* filter = filters[i];
            if (filter->providerKey == nullptr || *filter->providerKey != *providerKey)
            {
                continue;
            }

            if (filter->subLayerKey != *sublayerKey)
            {
                continue;
            }

            (*result)++;
        }

        FwpmFreeMemory(reinterpret_cast<void**>(&filters));
    }

    FwpmFilterDestroyEnumHandle(sessionHandle, enumHandle);

    return status;
}

unsigned int IPFilterDestroySublayerFilters(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey)
{
    HANDLE enumHandle = nullptr;
    
    auto status = FwpmFilterCreateEnumHandle(sessionHandle,
        nullptr,
        &enumHandle);
    if (status != ERROR_SUCCESS)
    {
        return status;
    }

    while (true)
    {
        FWPM_FILTER** filters{};
        UINT32 filterCount{};

        status = FwpmFilterEnum(sessionHandle, enumHandle, 1, &filters, &filterCount);
        if (status != ERROR_SUCCESS || filterCount == 0)
        {
            break;
        }

        for (UINT32 i = 0; i < filterCount; i++)
        {
            FWPM_FILTER* filter = filters[i];
            if (filter->providerKey == nullptr || *filter->providerKey != *providerKey)
            {
                continue;
            }

            if (filter->subLayerKey != *sublayerKey)
            {
                continue;
            }

            status = IPFilterDestroyFilter(sessionHandle, &filter->filterKey);
            if (status != ERROR_SUCCESS)
            {
                break;
            }
        }

        FwpmFreeMemory(reinterpret_cast<void**>(&filters));
    }

    FwpmFilterDestroyEnumHandle(sessionHandle, enumHandle);

    return status;
}

unsigned int IPFilterDestroySublayerFiltersByName(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const wchar_t* name)
{
    HANDLE enumHandle = nullptr;

    auto status = FwpmFilterCreateEnumHandle(sessionHandle,
        nullptr,
        &enumHandle);
    if (status != ERROR_SUCCESS)
    {
        return status;
    }

    while (true)
    {
        FWPM_FILTER** filters{};
        UINT32 filterCount{};

        status = FwpmFilterEnum(sessionHandle, enumHandle, 1, &filters, &filterCount);
        if (status != ERROR_SUCCESS || filterCount == 0)
        {
            break;
        }

        for (UINT32 i = 0; i < filterCount; i++)
        {
            FWPM_FILTER* filter = filters[i];
            if (filter->providerKey == nullptr || *filter->providerKey != *providerKey)
            {
                continue;
            }

            if (filter->subLayerKey != *sublayerKey)
            {
                continue;
            }

            if (wcscmp(filter->displayData.name, name) != 0)
            {
                continue;
            }

            status = IPFilterDestroyFilter(sessionHandle, &filter->filterKey);
            if (status != ERROR_SUCCESS)
            {
                break;
            }
        }

        FwpmFreeMemory(reinterpret_cast<void**>(&filters));
    }

    FwpmFilterDestroyEnumHandle(sessionHandle, enumHandle);

    return status;
}

unsigned int IPFilterDestroyCallouts(IPFilterSessionHandle sessionHandle, GUID* providerKey)
{
    HANDLE enumHandle = nullptr;

    FWPM_CALLOUT_ENUM_TEMPLATE tpl;
    memset(&tpl, 0, sizeof(tpl));
    tpl.providerKey = providerKey;

    auto status = FwpmCalloutCreateEnumHandle(sessionHandle,
        &tpl,
        &enumHandle);
    if (status != ERROR_SUCCESS)
    {
        return status;
    }

    while (true)
    {
        FWPM_CALLOUT** callouts{};
        UINT32 calloutCount{};

        status = FwpmCalloutEnum(sessionHandle, enumHandle, 1, &callouts, &calloutCount);
        if (status != ERROR_SUCCESS || calloutCount == 0)
        {
            break;
        }

        for (UINT32 i = 0; i < calloutCount; i++)
        {
            FWPM_CALLOUT* callout = callouts[i];
            if (callout->providerKey == nullptr || *callout->providerKey != *providerKey)
            {
                continue;
            }

            status = IPFilterDestroyCallout(sessionHandle, &callout->calloutKey);
            if (status != ERROR_SUCCESS)
            {
                break;
            }
        }

        FwpmFreeMemory(reinterpret_cast<void**>(&callouts));
    }

    FwpmCalloutDestroyEnumHandle(sessionHandle, enumHandle);

    return status;
}

unsigned int IPFilterDoesSublayerExist(
    IPFilterSessionHandle sessionHandle,
    const GUID* sublayerKey,
    unsigned int* result)
{
    FWPM_SUBLAYER* sublayer{};

    auto status = FwpmSubLayerGetByKey(sessionHandle, sublayerKey, &sublayer);
    if (status == FWP_E_SUBLAYER_NOT_FOUND)
    {
        status = ERROR_SUCCESS;
        *result = 1;
    } else if (status == ERROR_SUCCESS)
    {
        *result = 0;
    }

    if (sublayer != nullptr)
    {
        FwpmFreeMemory(reinterpret_cast<void**>(&sublayer));
    }

    return status;
}

unsigned int IPFilterDoesFilterExist(
    IPFilterSessionHandle sessionHandle,
    const GUID * filterKey,
    unsigned int* result)
{
    FWPM_FILTER* filter{};

    auto status = FwpmFilterGetByKey(sessionHandle, filterKey, &filter);
    if (status == FWP_E_FILTER_NOT_FOUND)
    {
        status = ERROR_SUCCESS;
        *result = 1;
    }
    else if (status == ERROR_SUCCESS)
    {
        *result = 0;
    }

    if (filter != nullptr)
    {
        FwpmFreeMemory(reinterpret_cast<void**>(&filter));
    }

    return status;
}

unsigned int IPFilterDoesProviderContextExist(
    IPFilterSessionHandle sessionHandle,
    const GUID* providerContextKey,
    unsigned int* result)
{
    FWPM_PROVIDER_CONTEXT* context{};

    auto status = FwpmProviderContextGetByKey(sessionHandle, providerContextKey, &context);
    if (status == FWP_E_PROVIDER_CONTEXT_NOT_FOUND)
    {
        status = ERROR_SUCCESS;
        *result = 1;
    }
    else if (status == ERROR_SUCCESS)
    {
        *result = 0;
    }

    if (context != nullptr)
    {
        FwpmFreeMemory(reinterpret_cast<void**>(&context));
    }

    return status;
}

unsigned int IPFilterDoesCalloutExist(
    IPFilterSessionHandle sessionHandle,
    const GUID* calloutKey,
    unsigned int* result)
{
    FWPM_CALLOUT* callout{};

    auto status = FwpmCalloutGetByKey(sessionHandle, calloutKey, &callout);
    if (status == FWP_E_PROVIDER_CONTEXT_NOT_FOUND)
    {
        status = ERROR_SUCCESS;
        *result = 1;
    }
    else if (status == ERROR_SUCCESS)
    {
        *result = 0;
    }

    if (callout != nullptr)
    {
        FwpmFreeMemory(reinterpret_cast<void**>(&callout));
    }

    return status;
}