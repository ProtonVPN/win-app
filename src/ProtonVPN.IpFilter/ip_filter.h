#pragma once

#define BOOL unsigned int

#define EXPORT __declspec(dllexport)

typedef void* IPFilterSessionHandle;

#define CUSTOM_ERROR_CODE(x) (x <= 0 ? x : ((x & 0x0000FFFF) | (FACILITY_ITF << 16) | 0x80000000))

const unsigned int E_ADAPTER_NOT_FOUND = CUSTOM_ERROR_CODE(0x0200);

enum class IPFilterLayer : unsigned int
{
    AppFlowEstablishedV4 = 0,
    AppFlowEstablishedV6 = 1,
    AppAuthConnectV4 = 2,
    AppAuthConnectV6 = 3,
    BindRedirectV4 = 4,
    BindRedirectV6 = 5,
    AppConnectRedirectV4 = 6,
    OutboundIPPacketV4 = 7,
};

enum class IPFilterAction : unsigned int
{
    SoftBlock = 0,
    HardBlock = 1,
    SoftPermit = 2,
    HardPermit = 3,
    Callout = 4,
};

struct IPFilterDisplayData
{
    wchar_t* name;
    wchar_t* description;
};

struct IPFilterNetworkAddress
{
    char* address;
    char* mask;
};

extern "C" EXPORT unsigned int IPFilterCreateDynamicSession(
    IPFilterSessionHandle* handle);

extern "C" EXPORT unsigned int IPFilterCreateSession(
    IPFilterSessionHandle* handle);

extern "C" EXPORT unsigned int IPFilterDestroySession(
    IPFilterSessionHandle handle);

extern "C" EXPORT unsigned int IPFilterStartTransaction(
    IPFilterSessionHandle* handle);

extern "C" EXPORT unsigned int IPFilterAbortTransaction(
    IPFilterSessionHandle* handle);

extern "C" EXPORT unsigned int IPFilterCommitTransaction(
    IPFilterSessionHandle* handle);

extern "C" EXPORT unsigned int IPFilterCreateProvider(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    BOOL persistent,
    GUID* providerKey);

extern "C" EXPORT unsigned int IPFilterDestroyProvider(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey);

extern "C" EXPORT unsigned int IPFilterIsProviderRegistered(
    IPFilterSessionHandle sessionHandle,
    const GUID * providerKey,
    unsigned int* result);

extern "C" EXPORT unsigned int IPFilterCreateProviderContext(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    GUID* providerKey,
    unsigned int size,
    UINT8* data,
    BOOL persistent,
    GUID* providerContextKey);

extern "C" EXPORT unsigned int IPFilterDestroyProviderContext(
    IPFilterSessionHandle sessionHandle,
    GUID* providerContextKey);

extern "C" EXPORT unsigned int IPFilterCreateCallout(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    GUID* providerKey,
    unsigned int layer,
    BOOL persistent,
    GUID* calloutKey);

extern "C" EXPORT unsigned int IPFilterCreateSublayer(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    const IPFilterDisplayData* displayData,
    unsigned int weight,
    BOOL persistent,
    GUID* sublayerKey);

extern "C" EXPORT unsigned int IPFilterDestroySublayer(
    IPFilterSessionHandle sessionHandle,
    GUID* subLayerKey);

extern "C" EXPORT unsigned int IPFilterDestroySublayerFilters(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey);

extern "C" EXPORT unsigned int IPFilterDestroyCallouts(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey);

extern "C" EXPORT unsigned int IPFilterDoesSublayerExist(
    IPFilterSessionHandle sessionHandle,
    const GUID* sublayerKey,
    unsigned int* result);

extern "C" EXPORT unsigned int IPFilterDoesFilterExist(
    IPFilterSessionHandle sessionHandle,
    const GUID* filterKey,
    unsigned int* result);

extern "C" EXPORT unsigned int IPFilterDoesProviderContextExist(
    IPFilterSessionHandle sessionHandle,
    const GUID* providerContextKey,
    unsigned int* result);

extern "C" EXPORT unsigned int IPFilterDoesCalloutExist(
    IPFilterSessionHandle sessionHandle,
    const GUID* calloutKey,
    unsigned int* result);

extern "C" EXPORT unsigned int IPFilterDestroyFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterDestroyCallout(
    IPFilterSessionHandle sessionHandle,
    GUID * calloutKey);

extern "C" EXPORT unsigned int IPFilterCreateLayerFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    BOOL persistent,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateRemoteIPv4Filter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID * calloutKey,
    GUID * providerContextKey,
    const char* addr,
    BOOL persistent,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateAppFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* calloutKey,
    GUID* providerContextKey,
    const wchar_t* path,
    BOOL persistent,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateRemoteTCPPortFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    unsigned int port,
    BOOL persistent,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateRemoteUDPPortFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    unsigned int port,
    BOOL persistent,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateRemoteNetworkIPv4Filter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID * calloutKey,
    GUID * providerContextKey,
    const IPFilterNetworkAddress* addr,
    BOOL persistent,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateNetInterfaceFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    ULONG index,
    BOOL persistent,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateLoopbackFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    BOOL persistent,
    GUID* filterKey);

extern "C" EXPORT unsigned int BlockOutsideDns(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey,
    const IPFilterDisplayData * displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID * calloutKey,
    ULONG index,
    BOOL persistent,
    GUID * filterKey);