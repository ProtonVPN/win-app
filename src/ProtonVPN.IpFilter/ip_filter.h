#pragma once

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
    GUID* providerKey);

extern "C" EXPORT unsigned int IPFilterCreateProviderContext(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    GUID* providerKey,
    unsigned int size,
    UINT8* data,
    GUID* providerContextKey);

extern "C" EXPORT unsigned int IPFilterDestroyProviderContext(
    IPFilterSessionHandle sessionHandle,
    GUID* providerContextKey);

extern "C" EXPORT unsigned int IPFilterCreateCallout(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData* displayData,
    GUID* providerKey,
    unsigned int layer,
    GUID* calloutKey);

extern "C" EXPORT unsigned int IPFilterCreateSublayer(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    const IPFilterDisplayData* displayData,
    unsigned int weight,
    GUID* sublayerKey);

extern "C" EXPORT unsigned int IPFilterDestroySublayer(
    IPFilterSessionHandle sessionHandle,
    GUID* subLayerKey);

extern "C" EXPORT unsigned int IPFilterDestroyFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* filterKey);

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
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateRemoteIPv4Filter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    const char* addr,
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
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateRemoteNetworkIPv4Filter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    const IPFilterNetworkAddress* addr,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateNetInterfaceFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    const char* name,
    GUID* filterKey);

extern "C" EXPORT unsigned int IPFilterCreateLoopbackFilter(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID* filterKey);
