#pragma once

#define BOOL int

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
    AppAuthRecvAcceptV6 = 8,
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
    int prefix;
    bool isIpv6;
};

unsigned int IPFilterCreateDynamicSession(
    IPFilterSessionHandle * handle);

unsigned int IPFilterCreateSession(
    IPFilterSessionHandle * handle);

unsigned int IPFilterDestroySession(
    IPFilterSessionHandle handle);

unsigned int IPFilterStartTransaction(
    IPFilterSessionHandle handle);

unsigned int IPFilterAbortTransaction(
    IPFilterSessionHandle handle);

unsigned int IPFilterCommitTransaction(
    IPFilterSessionHandle handle);

unsigned int IPFilterCreateProvider(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData * displayData,
    BOOL persistent,
    GUID * providerKey);

unsigned int IPFilterDestroyProvider(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey);

unsigned int IPFilterIsProviderRegistered(
    IPFilterSessionHandle sessionHandle,
    const GUID * providerKey,
    unsigned int* result);

unsigned int IPFilterCreateProviderContext(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData * displayData,
    GUID * providerKey,
    unsigned int size,
    UINT8 * data,
    BOOL persistent,
    GUID * providerContextKey);

unsigned int IPFilterDestroyProviderContext(
    IPFilterSessionHandle sessionHandle,
    GUID * providerContextKey);

unsigned int IPFilterCreateCallout(
    IPFilterSessionHandle sessionHandle,
    const IPFilterDisplayData * displayData,
    GUID * providerKey,
    unsigned int layer,
    BOOL persistent,
    GUID * calloutKey);

unsigned int IPFilterCreateSublayer(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    const IPFilterDisplayData * displayData,
    unsigned int weight,
    BOOL persistent,
    GUID * sublayerKey);

unsigned int IPFilterDestroySublayer(
    IPFilterSessionHandle sessionHandle,
    GUID * subLayerKey);

unsigned int IPFilterDestroySublayerFilters(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey);

unsigned int IPFilterDestroySublayerFiltersByName(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const wchar_t* name);

unsigned int IPFilterGetSublayerFilterCount(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    unsigned int* result);

unsigned int IPFilterDestroyCallouts(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey);

unsigned int IPFilterDoesSublayerExist(
    IPFilterSessionHandle sessionHandle,
    const GUID * sublayerKey,
    unsigned int* result);

unsigned int IPFilterDoesFilterExist(
    IPFilterSessionHandle sessionHandle,
    const GUID * filterKey,
    unsigned int* result);

unsigned int IPFilterDoesProviderContextExist(
    IPFilterSessionHandle sessionHandle,
    const GUID * providerContextKey,
    unsigned int* result);

unsigned int IPFilterDoesCalloutExist(
    IPFilterSessionHandle sessionHandle,
    const GUID * calloutKey,
    unsigned int* result);

unsigned int IPFilterDestroyFilter(
    IPFilterSessionHandle sessionHandle,
    GUID * filterKey);

unsigned int IPFilterDestroyCallout(
    IPFilterSessionHandle sessionHandle,
    GUID * calloutKey);

unsigned int IPFilterCreateLayerFilter(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey,
    const IPFilterDisplayData * displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID * calloutKey,
    GUID * providerContextKey,
    BOOL persistent,
    GUID * filterKey);

unsigned int IPFilterCreateRemoteIPv4Filter(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey,
    const IPFilterDisplayData * displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID * calloutKey,
    GUID * providerContextKey,
    const char* addr,
    BOOL persistent,
    GUID * filterKey);

unsigned int IPFilterCreateAppFilter(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey,
    const IPFilterDisplayData * displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID * calloutKey,
    GUID * providerContextKey,
    const wchar_t* path,
    BOOL persistent,
    GUID * filterKey);

unsigned int IPFilterCreateRemoteTCPPortFilter(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey,
    const IPFilterDisplayData * displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    unsigned int port,
    BOOL persistent,
    GUID * filterKey);

unsigned int IPFilterCreateRemoteUDPPortFilter(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey,
    const IPFilterDisplayData * displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    unsigned int port,
    BOOL persistent,
    GUID * filterKey);

unsigned int IPFilterCreateRemoteNetworkIPFilter(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey,
    const IPFilterDisplayData * displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    GUID * calloutKey,
    GUID * providerContextKey,
    const IPFilterNetworkAddress * addr,
    BOOL persistent,
    GUID * filterKey);

unsigned int IPFilterCreateNetInterfaceFilter(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey,
    const IPFilterDisplayData * displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    ULONG index,
    BOOL persistent,
    GUID * filterKey);

unsigned int IPFilterCreateLoopbackFilter(
    IPFilterSessionHandle sessionHandle,
    GUID * providerKey,
    GUID * sublayerKey,
    const IPFilterDisplayData * displayData,
    unsigned int layer,
    unsigned int action,
    unsigned int weight,
    BOOL persistent,
    GUID * filterKey);

unsigned int BlockOutsideDns(
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

unsigned int BlockOutsideOpenVpn(
    IPFilterSessionHandle sessionHandle,
    GUID* providerKey,
    GUID* sublayerKey,
    const IPFilterDisplayData* displayData,
    unsigned int layer,
    unsigned int weight,
    const wchar_t* openVpnPath,
    const char* serverIpAddress,
    BOOL persistent,
    GUID* filterKey);

unsigned int PermitRouterSolicitationMessage(
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

unsigned int PermitRouterAdvertisementMessage(
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

unsigned int PermitNeighborSolicitationMessage(
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

unsigned int PermitNeighborAdvertisementMessage(
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

unsigned int PermitIcmpRedirectMessage(
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