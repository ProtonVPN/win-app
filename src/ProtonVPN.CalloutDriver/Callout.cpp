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

bool isLocalNetwork(UINT32 addr)
{
	if (IN4_IS_ADDR_LOOPBACK(reinterpret_cast<IN_ADDR*>(&addr)) || //127/8
		IN4_IS_ADDR_LINKLOCAL(reinterpret_cast<IN_ADDR*>(&addr)) || //169.254/16
		IN4_IS_ADDR_RFC1918(reinterpret_cast<IN_ADDR*>(&addr)) || //10/8, 172.16/12, 192.168/16
		IN4_IS_ADDR_MC_LINKLOCAL(reinterpret_cast<IN_ADDR*>(&addr)) || //224.0.0/24
		IN4_IS_ADDR_BROADCAST(reinterpret_cast<IN_ADDR*>(&addr)) || //255.255.255.255
		IN4_IS_ADDR_MC_ADMINLOCAL(reinterpret_cast<IN_ADDR*>(&addr))) //239.255/16
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

	if (isLocalNetwork(remoteAddr))
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

VOID NTAPI CompleteBasicPacketInjection(_In_ VOID*,
	_Inout_ NET_BUFFER_LIST*,
	_In_ BOOLEAN)
{
}

void PacketDnsReplyInitv4(PDNSPACKETV4 packet)
{
	memset(packet, 0, sizeof(DNSPACKETV4));
	packet->ip.Version = 4;
	packet->ip.HdrLength = sizeof(WINDIVERT_IPHDR) / sizeof(UINT32);
	packet->ip.Length = htons(sizeof(DNSPACKETV4));
	packet->ip.Protocol = IPPROTO_UDP;
	packet->ip.Id = 0;
	packet->ip.TTL = 64;
	packet->udp.Length = ntohs(sizeof(WINDIVERT_UDPHDR) + sizeof(DNSHEADER));
}

UINT16 CalcChecksum(PVOID data, UINT len)
{
	UINT32 sum = 0;
	const auto* data16 = static_cast<const UINT16*>(data);
	const size_t len16 = len >> 1;

	for (size_t i = 0; i < len16; i++)
	{
		sum += static_cast<UINT32>(data16[i]);
	}

	if (len & 0x1)
	{
		const auto* data8 = static_cast<const UINT8*>(data);
		sum += static_cast<UINT16>(data8[len - 1]);
	}

	sum = (sum & 0xFFFF) + (sum >> 16);
	sum += (sum >> 16);
	sum = ~sum;

	return static_cast<UINT16>(sum);
}

DNSPACKETV4 GetDnsResponsePacket(UINT16 transaction_id, UINT32 src_addr, UINT32 dst_addr, UINT16 src_port, UINT16 dst_port)
{
	DNSPACKETV4 packet;
	PacketDnsReplyInitv4(&packet);
	packet.ip.SrcAddr = src_addr;
	packet.ip.DstAddr = dst_addr;
	packet.udp.SrcPort = src_port;
	packet.udp.DstPort = dst_port;
	packet.dns.transaction_id = transaction_id;
	packet.ip.Checksum = CalcChecksum(
		&packet.ip,
		packet.ip.HdrLength * sizeof(UINT32));

	return packet;
}

bool BlockDnsPacket(PNET_BUFFER buffer, UINT32 interface_index, UINT32 subinterface_index)
{
	UNREFERENCED_PARAMETER(interface_index);
	UNREFERENCED_PARAMETER(subinterface_index);
	
	const UINT total_len = NET_BUFFER_DATA_LENGTH(buffer);
	if (total_len < sizeof(WINDIVERT_IPHDR))
	{
		return false;
	}

	auto* ip_header = static_cast<PWINDIVERT_IPHDR>(NdisGetDataBuffer(buffer, sizeof(WINDIVERT_IPHDR), nullptr, 1, 0));
	if (ip_header == nullptr)
	{
		return false;
	}

	const UINT ip_header_len = ip_header->HdrLength * sizeof(UINT32);
	if (ip_header->Version != 4 ||
		RtlUshortByteSwap(ip_header->Length) != total_len ||
		ip_header->HdrLength < 5 ||
		ip_header_len > total_len)
	{
		return false;
	}

	if (ip_header->Protocol != IPPROTO_UDP)
	{
		return false;
	}

	auto offset_delta = ip_header_len;
	
	NdisAdvanceNetBufferDataStart(buffer, offset_delta, FALSE, nullptr);
	auto* udp_header = static_cast<PWINDIVERT_UDPHDR>(NdisGetDataBuffer(buffer, sizeof(WINDIVERT_UDPHDR), nullptr, 1, 0));

	if (static_cast<UINT32>(ntohs(udp_header->DstPort)) != 53)
	{
		NdisRetreatNetBufferDataStart(buffer, offset_delta, 0, nullptr);
		return false;
	}

	const UINT udp_header_len = 8;
	NdisAdvanceNetBufferDataStart(buffer, udp_header_len, FALSE, nullptr);
	auto* dns_header = static_cast<PDNSHEADER>(NdisGetDataBuffer(buffer, sizeof(DNSHEADER), nullptr, 1, 0));
	offset_delta += udp_header_len;

	auto dns_packet = GetDnsResponsePacket(
		dns_header->transaction_id,
		ip_header->DstAddr,
		ip_header->SrcAddr,
		udp_header->DstPort,
		udp_header->SrcPort);
	
	UINT packet_length = RtlUshortByteSwap(dns_packet.ip.Length);
	auto* mdl_copy = IoAllocateMdl(&dns_packet, packet_length, FALSE, FALSE, nullptr);
	MmBuildMdlForNonPagedPool(mdl_copy);

	NET_BUFFER_LIST* cloned_net_buffer_list;
	auto status = FwpsAllocateNetBufferAndNetBufferList0(
		nbl_pool_handle,
		0,
		0,
		mdl_copy,
		0,
		packet_length,
		&cloned_net_buffer_list);

	if (!NT_SUCCESS(status))
	{
		IoFreeMdl(mdl_copy);
		NdisRetreatNetBufferDataStart(buffer, offset_delta, 0, nullptr);
		return false;
	}

	status = FwpsInjectNetworkReceiveAsync0(
		injectHandle,
		nullptr,
		0,
		UNSPECIFIED_COMPARTMENT_ID,
		interface_index,
		subinterface_index,
		cloned_net_buffer_list,
		CompleteBasicPacketInjection,
		nullptr);

	if (!NT_SUCCESS(status))
	{
		FwpsFreeNetBufferList0(cloned_net_buffer_list);
		IoFreeMdl(mdl_copy);
		NdisRetreatNetBufferDataStart(buffer, offset_delta, 0, nullptr);
		return false;
	}

	FwpsFreeNetBufferList0(cloned_net_buffer_list);
	IoFreeMdl(mdl_copy);
	NdisRetreatNetBufferDataStart(buffer, offset_delta, 0, nullptr);

	return true;
}

void NTAPI BlockDnsBySendingServerFailPacket(
	IN const FWPS_INCOMING_VALUES* inFixedValues,
	IN const FWPS_INCOMING_METADATA_VALUES*,
	IN OUT void* packet,
	IN const void*,
	IN const FWPS_FILTER*,
	IN UINT64,
	IN OUT FWPS_CLASSIFY_OUT* result)
{
	if (packet == nullptr)
	{
		return;
	}

	result->actionType = FWP_ACTION_PERMIT;

	auto* const buffers = static_cast<PNET_BUFFER_LIST>(packet);
	HANDLE packet_context = nullptr;

	if (NET_BUFFER_LIST_NEXT_NBL(buffers) != nullptr)
	{
		return;
	}

	const auto packet_state = FwpsQueryPacketInjectionState0(injectHandle, buffers, &packet_context);
	if (packet_state == FWPS_PACKET_INJECTED_BY_SELF ||
		packet_state == FWPS_PACKET_PREVIOUSLY_INJECTED_BY_SELF)
	{
		return;
	}

	const auto interfaceIndex = static_cast<IF_INDEX>(inFixedValues->incomingValue[FWPS_FIELD_OUTBOUND_IPPACKET_V4_INTERFACE_INDEX].value.uint32);
	const auto subInterfaceIndex = static_cast<IF_INDEX>(inFixedValues->incomingValue[FWPS_FIELD_OUTBOUND_IPPACKET_V4_SUB_INTERFACE_INDEX].value.uint32);
	auto blocked = false;
	auto* buffer = NET_BUFFER_LIST_FIRST_NB(buffers);

    while (buffer != nullptr)
	{
		blocked = BlockDnsPacket(buffer, interfaceIndex, subInterfaceIndex);
		buffer = NET_BUFFER_NEXT_NB(buffer);
	}

	if (blocked)
	{
		result->actionType = FWP_ACTION_BLOCK;
		result->flags |= FWPS_CLASSIFY_OUT_FLAG_ABSORB;
		result->rights &= ~FWPS_RIGHT_ACTION_WRITE;
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