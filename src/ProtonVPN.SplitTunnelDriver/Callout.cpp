#include <fwpsk.h>
#include <fwpmtypes.h>
#include <mstcpip.h>

#include "Trace.h"
#include "Public.h"
#include "Callout.h"
#include "Callout.tmh"

const UINT8 TCP_PROTOCOL_ID = 6;

void SetSocketIPv4Addr(const SOCKADDR_STORAGE& sockAddrStorage, const IN_ADDR& addr)
{
	INETADDR_SET_ADDRESS((PSOCKADDR) & (sockAddrStorage), &(addr.S_un.S_un_b.s_b1));
}

bool isLoopbackIPv4Address(UINT32 addr)
{
	if (IN4_IS_ADDR_LOOPBACK(reinterpret_cast<IN_ADDR*>(&addr)))
	{
		return true;
	}

	return false;
}

const CONNECT_REDIRECT_DATA* GetConnectRedirectDataFromProviderContext(const FWPM_PROVIDER_CONTEXT* context)
{
	if (context == nullptr)
	{
		return nullptr;
	}

	if (context->type != FWPM_GENERAL_CONTEXT)
	{
		return nullptr;
	}

	if (context->dataBuffer == nullptr)
	{
		return nullptr;
	}

	if (context->dataBuffer->size != sizeof(CONNECT_REDIRECT_DATA))
	{
		return nullptr;
	}

	return reinterpret_cast<CONNECT_REDIRECT_DATA*>(context->dataBuffer->data);
}

void NTAPI RedirectConnection(
	IN const FWPS_INCOMING_VALUES* inFixedValues,
	IN const FWPS_INCOMING_METADATA_VALUES*,
	IN OUT void*,
	IN const void* classifyContext,
	IN const FWPS_FILTER* filter,
	IN UINT64,
	IN OUT FWPS_CLASSIFY_OUT* classifyOut
)
{
	classifyOut->actionType = FWP_ACTION_PERMIT;

	if (inFixedValues == nullptr)
	{
		return;
	}

	if (inFixedValues->layerId != FWPS_LAYER_ALE_CONNECT_REDIRECT_V4)
	{
		return;
	}

	if ((classifyOut->rights & FWPS_RIGHT_ACTION_WRITE) == 0)
	{
		return;
	}

	auto flags = inFixedValues->incomingValue[FWPS_FIELD_ALE_CONNECT_REDIRECT_V4_FLAGS].value.uint32;
	if (flags & FWP_CONDITION_FLAG_IS_REAUTHORIZE)
	{
		return;
	}

	auto connectRedirectData = GetConnectRedirectDataFromProviderContext(filter->providerContext);
	if (connectRedirectData == nullptr)
	{
		return;
	}

	auto remoteAddr = RtlUlongByteSwap(inFixedValues->incomingValue[
		FWPS_FIELD_ALE_CONNECT_REDIRECT_V4_IP_REMOTE_ADDRESS].value.uint32);

	if (isLoopbackIPv4Address(remoteAddr))
	{
		return;
	}

	UINT64 classifyHandle{};
	FWPS_CONNECT_REQUEST* connectReq{};

	__try
	{
		auto status = FwpsAcquireClassifyHandle(const_cast<void*>(classifyContext), 0, &classifyHandle);
		if (!NT_SUCCESS(status))
		{
			return;
		}

		status = FwpsAcquireWritableLayerDataPointer(classifyHandle,
			filter->filterId, 0, reinterpret_cast<PVOID*>(&connectReq), classifyOut);
		if (!NT_SUCCESS(status))
		{
			return;
		}

		SetSocketIPv4Addr(connectReq->localAddressAndPort, connectRedirectData->localAddress);
	}
	__finally
	{
		if (connectReq != nullptr)
		{
			FwpsApplyModifiedLayerData(classifyHandle, reinterpret_cast<PVOID>(connectReq), 0);
		}

		if (classifyHandle != 0)
		{
			FwpsReleaseClassifyHandle(classifyHandle);
		}
	}
}

void NTAPI RedirectUDPFlow(
	IN const FWPS_INCOMING_VALUES* inFixedValues,
	IN const FWPS_INCOMING_METADATA_VALUES*,
	IN OUT void*,
	IN const void* classifyContext,
	IN const FWPS_FILTER* filter,
	IN UINT64,
	IN OUT FWPS_CLASSIFY_OUT* classifyOut
)
{
	classifyOut->actionType = FWP_ACTION_PERMIT;

	if (inFixedValues == nullptr)
	{
		return;
	}

	if (inFixedValues->layerId != FWPS_LAYER_ALE_BIND_REDIRECT_V4)
	{
		return;
	}

	if ((classifyOut->rights & FWPS_RIGHT_ACTION_WRITE) == 0)
	{
		return;
	}

	auto flags = inFixedValues->incomingValue[FWPS_FIELD_ALE_BIND_REDIRECT_V4_FLAGS].value.uint32;
	if (flags & FWP_CONDITION_FLAG_IS_REAUTHORIZE)
	{
		return;
	}

	auto protocol = inFixedValues->incomingValue[FWPS_FIELD_ALE_BIND_REDIRECT_V4_IP_PROTOCOL].value.uint8;
	if (protocol == TCP_PROTOCOL_ID)
	{
		return;
	}

	auto connectRedirectData = GetConnectRedirectDataFromProviderContext(filter->providerContext);
	if (connectRedirectData == nullptr)
	{
		return;
	}

	UINT64 classifyHandle{};
	FWPS_BIND_REQUEST* bindReq{};

	__try
	{
		auto status = FwpsAcquireClassifyHandle(const_cast<void*>(classifyContext), 0, &classifyHandle);
		if (!NT_SUCCESS(status))
		{
			return;
		}

		status = FwpsAcquireWritableLayerDataPointer(classifyHandle,
			filter->filterId, 0, reinterpret_cast<PVOID*>(&bindReq), classifyOut);
		if (!NT_SUCCESS(status))
		{
			return;
		}

		SetSocketIPv4Addr(bindReq->localAddressAndPort, connectRedirectData->localAddress);
	}
	__finally
	{
		if (bindReq != nullptr)
		{
			FwpsApplyModifiedLayerData(classifyHandle, reinterpret_cast<PVOID>(bindReq), 0);
		}

		if (classifyHandle != 0)
		{
			FwpsReleaseClassifyHandle(classifyHandle);
		}
	}
}

NTSTATUS NTAPI NotifyFn(
	IN FWPS_CALLOUT_NOTIFY_TYPE notifyType,
	IN const GUID* filterKey,
	IN const FWPS_FILTER* filter
)
{
	UNREFERENCED_PARAMETER(notifyType);
	UNREFERENCED_PARAMETER(filterKey);
	UNREFERENCED_PARAMETER(filter);

	return STATUS_SUCCESS;
}

NTSTATUS RegisterCallout(
	_In_ PDEVICE_OBJECT deviceObject,
	_In_ const GUID& key,
	_In_ FWPS_CALLOUT_CLASSIFY_FN classifyFn
)
{
	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Entry");

	FWPS_CALLOUT callout{};
	callout.calloutKey = key;
	callout.classifyFn = classifyFn;
	callout.notifyFn = reinterpret_cast<FWPS_CALLOUT_NOTIFY_FN>(NotifyFn);
	callout.flowDeleteFn = nullptr;

	auto status = FwpsCalloutRegister(deviceObject, &callout, nullptr);
	if (!NT_SUCCESS(status))
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! FwpsCalloutRegister1 failed %!STATUS!", status);

		return status;
	}

	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Exit");

	return status;
}

NTSTATUS UnregisterCallout(_In_ const GUID& key)
{
	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Entry");

	auto status = FwpsCalloutUnregisterByKey(&key);
	if (!NT_SUCCESS(status))
	{
		TraceEvents(TRACE_LEVEL_ERROR, TRACE_CALLOUT, "%!FUNC! FwpsCalloutUnregisterByKey failed %!STATUS!", status);
	}

	TraceEvents(TRACE_LEVEL_INFORMATION, TRACE_CALLOUT, "%!FUNC! Exit");

	return status;
}