#pragma once

#include <fwpsk.h>
#include <fwpmtypes.h>

NTSTATUS RegisterCallout(
	_In_ PDEVICE_OBJECT deviceObject,
	_In_ const GUID& key,
	_In_ FWPS_CALLOUT_CLASSIFY_FN classifyFn);

NTSTATUS UnregisterCallout(_In_ const GUID& key);

void NTAPI RedirectConnection(
	IN const FWPS_INCOMING_VALUES* inFixedValues,
	IN const FWPS_INCOMING_METADATA_VALUES* inMetaValues,
	IN OUT VOID* layerData,
	IN const void* classifyContext,
	IN const FWPS_FILTER* filter,
	IN UINT64 flowContext,
	IN OUT FWPS_CLASSIFY_OUT* classifyOut
);

void NTAPI RedirectUDPFlow(
	IN const FWPS_INCOMING_VALUES* inFixedValues,
	IN const FWPS_INCOMING_METADATA_VALUES* inMetaValues,
	IN OUT VOID* layerData,
	IN const void* classifyContext,
	IN const FWPS_FILTER* filter,
	IN UINT64 flowContext,
	IN OUT FWPS_CLASSIFY_OUT* classifyOut
);