#include <fwpsk.h>
#include <fwpmtypes.h>
#include <mstcpip.h>

#include "Trace.h"
#include "Public.h"
#include "Callout.h"
#include "Callout.tmh"
#include "stdio.h"

const UINT8 TCP_PROTOCOL_ID = 6;

struct REDIRECT_ADDRESS
{
	ADDRESS_FAMILY family { AF_UNSPEC };
	union
	{
		IN_ADDR ipv4;
		IN6_ADDR ipv6;
	} address;
};

static bool TryGetRedirectAddressFromContext(const FWPM_PROVIDER_CONTEXT* context, REDIRECT_ADDRESS& redirectAddress)
{
	redirectAddress.family = AF_UNSPEC;

	if (context == nullptr || context->type != FWPM_GENERAL_CONTEXT)
	{
		return false;
	}

	if (context->dataBuffer == nullptr || context->dataBuffer->data == nullptr)
	{
		return false;
	}

	const auto* data = reinterpret_cast<const CONNECT_REDIRECT_DATA*>(context->dataBuffer->data);

	if (data->addressFamily == AF_INET)
	{
		redirectAddress.family = AF_INET;
		redirectAddress.address.ipv4 = data->address.v4;
		return true;
	}

	if (data->addressFamily == AF_INET6)
	{
		redirectAddress.family = AF_INET6;
		redirectAddress.address.ipv6 = data->address.v6;
		return true;
	}

	return false;
}

static bool ApplyRedirectAddress(SOCKADDR_STORAGE& sockAddrStorage, const REDIRECT_ADDRESS& redirectAddress)
{
	if (redirectAddress.family == AF_INET)
	{
		auto* address4 = reinterpret_cast<SOCKADDR_IN*>(&sockAddrStorage);
		address4->sin_family = AF_INET;
		address4->sin_addr = redirectAddress.address.ipv4;
		return true;
	}

	if (redirectAddress.family == AF_INET6)
	{
		auto* address6 = reinterpret_cast<SOCKADDR_IN6*>(&sockAddrStorage);
		address6->sin6_family = AF_INET6;
		address6->sin6_addr = redirectAddress.address.ipv6;
		return true;
	}

	return false;
}

static bool IsLocalIPv4Address(const IN_ADDR& address)
{
	auto* addressPtr = const_cast<IN_ADDR*>(&address);

	return (
		IN4_IS_ADDR_LOOPBACK(addressPtr) ||      // 127/8 - Loopback address range
		IN4_IS_ADDR_LINKLOCAL(addressPtr) ||     // 169.254/16 - Link-local address range
		IN4_IS_ADDR_RFC1918(addressPtr) ||       // 10/8, 172.16/12, 192.168/16 - Private address ranges
		IN4_IS_ADDR_MC_LINKLOCAL(addressPtr) ||  // 224.0.0/24 - Multicast link-local addresses
		IN4_IS_ADDR_BROADCAST(addressPtr) ||     // 255.255.255.255 - Broadcast address
		IN4_IS_ADDR_MC_ADMINLOCAL(addressPtr)    // 239.255/16 - Administrative-local multicast addresses
	);
}

static bool IsAddressTeredo(const IN6_ADDR& address)
{
	// Teredo addresses start with 2001:0000::/32
	// Check if the first 4 bytes match the Teredo prefix
	return (address.u.Byte[0] == 0x20) && (address.u.Byte[1] == 0x01) &&
		(address.u.Byte[2] == 0x00) && (address.u.Byte[3] == 0x00);
}

static bool IsLocalIPv6Address(const IN6_ADDR& address)
{
	auto* addressPtr = const_cast<IN6_ADDR*>(&address);

	return (
		IN6_IS_ADDR_UNSPECIFIED(addressPtr) ||    // ::/128 - all zeros address
		IN6_IS_ADDR_LOOPBACK(addressPtr) ||       // ::1/128 - loopback address
		IN6_IS_ADDR_LINKLOCAL(addressPtr) ||      // fe80::/10 - link-local addresses
		IN6_IS_ADDR_SITELOCAL(addressPtr) ||      // fec0::/10 - site-local addresses (deprecated)
		IN6_IS_ADDR_MULTICAST(addressPtr) ||      // ff00::/8 - multicast addresses
		IsAddressTeredo(address) ||               // 2001::/32 - Teredo addresses
		(address.u.Byte[0] & 0xFE) == 0xFC        // fc00::/7 - unique local addresses
	);
}

static UINT32 GetConnectRedirectFlags(const FWPS_INCOMING_VALUES* inFixedValues, bool isIpv4Layer)
{
	return isIpv4Layer
		? inFixedValues->incomingValue[FWPS_FIELD_ALE_CONNECT_REDIRECT_V4_FLAGS].value.uint32
		: inFixedValues->incomingValue[FWPS_FIELD_ALE_CONNECT_REDIRECT_V6_FLAGS].value.uint32;
}

static bool IsRemoteEndpointLocal(const FWPS_INCOMING_VALUES* inFixedValues, bool isIpv4Layer)
{
	if (isIpv4Layer)
	{
		IN_ADDR remoteAddress{};
		remoteAddress.S_un.S_addr = RtlUlongByteSwap(
			inFixedValues->incomingValue[FWPS_FIELD_ALE_CONNECT_REDIRECT_V4_IP_REMOTE_ADDRESS].value.uint32);

		if (IsLocalIPv4Address(remoteAddress))
		{
			return true;
		}
	}
	else
	{
		const auto* remoteByteArray = inFixedValues->incomingValue[FWPS_FIELD_ALE_CONNECT_REDIRECT_V6_IP_REMOTE_ADDRESS].value.byteArray16;
		if (remoteByteArray == nullptr)
		{
			return true;
		}

		IN6_ADDR remoteAddress{};
		RtlCopyMemory(&remoteAddress, remoteByteArray->byteArray16, sizeof(remoteAddress));

		if (IsLocalIPv6Address(remoteAddress))
		{
			return true;
		}
	}

	return false;
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

	if (inFixedValues == nullptr || filter == nullptr)
	{
		return;
	}

	if ((classifyOut->rights & FWPS_RIGHT_ACTION_WRITE) == 0)
	{
		return;
	}

	const auto layerId = inFixedValues->layerId;
	const bool isIpv4Layer = layerId == FWPS_LAYER_ALE_CONNECT_REDIRECT_V4;
	const bool isIpv6Layer = layerId == FWPS_LAYER_ALE_CONNECT_REDIRECT_V6;

	if (!isIpv4Layer && !isIpv6Layer)
	{
		return;
	}

	UINT32 flags = GetConnectRedirectFlags(inFixedValues, isIpv4Layer);
	if (flags & FWP_CONDITION_FLAG_IS_REAUTHORIZE)
	{
		return;
	}

	if (IsRemoteEndpointLocal(inFixedValues, isIpv4Layer))
	{
		return;
	}

	REDIRECT_ADDRESS redirectAddress{};
	if (!TryGetRedirectAddressFromContext(filter->providerContext, redirectAddress))
	{
		return;
	}

	if ((isIpv4Layer && redirectAddress.family != AF_INET) ||
		(isIpv6Layer && redirectAddress.family != AF_INET6))
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

		status = FwpsAcquireWritableLayerDataPointer(
			classifyHandle,
			filter->filterId,
			0,
			reinterpret_cast<PVOID*>(&connectReq),
			classifyOut);
		if (!NT_SUCCESS(status))
		{
			return;
		}

		ApplyRedirectAddress(connectReq->localAddressAndPort, redirectAddress);
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

static UINT32 GetBindRedirectFlags(const FWPS_INCOMING_VALUES* inFixedValues, bool isIpv4Layer)
{
	return isIpv4Layer
		? inFixedValues->incomingValue[FWPS_FIELD_ALE_BIND_REDIRECT_V4_FLAGS].value.uint32
		: inFixedValues->incomingValue[FWPS_FIELD_ALE_BIND_REDIRECT_V6_FLAGS].value.uint32;
}

static UINT8 GetBindRedirectProtocol(const FWPS_INCOMING_VALUES* inFixedValues, bool isIpv4Layer)
{
	return isIpv4Layer
		? inFixedValues->incomingValue[FWPS_FIELD_ALE_BIND_REDIRECT_V4_IP_PROTOCOL].value.uint8
		: inFixedValues->incomingValue[FWPS_FIELD_ALE_BIND_REDIRECT_V6_IP_PROTOCOL].value.uint8;
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

	if (inFixedValues == nullptr || filter == nullptr)
	{
		return;
	}

	if ((classifyOut->rights & FWPS_RIGHT_ACTION_WRITE) == 0)
	{
		return;
	}

	const auto layerId = inFixedValues->layerId;
	const bool isIpv4Layer = layerId == FWPS_LAYER_ALE_BIND_REDIRECT_V4;
	const bool isIpv6Layer = layerId == FWPS_LAYER_ALE_BIND_REDIRECT_V6;

	if (!isIpv4Layer && !isIpv6Layer)
	{
		return;
	}

	UINT32 flags = GetBindRedirectFlags(inFixedValues, isIpv4Layer);
	if (flags & FWP_CONDITION_FLAG_IS_REAUTHORIZE)
	{
		return;
	}

	UINT8 protocol = GetBindRedirectProtocol(inFixedValues, isIpv4Layer);
	if (protocol == TCP_PROTOCOL_ID)
	{
		return;
	}

	REDIRECT_ADDRESS redirectAddress{};
	if (!TryGetRedirectAddressFromContext(filter->providerContext, redirectAddress))
	{
		return;
	}

	if ((isIpv4Layer && redirectAddress.family != AF_INET) ||
		(isIpv6Layer && redirectAddress.family != AF_INET6))
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

		status = FwpsAcquireWritableLayerDataPointer(
			classifyHandle,
			filter->filterId,
			0,
			reinterpret_cast<PVOID*>(&bindReq),
			classifyOut);
		if (!NT_SUCCESS(status))
		{
			return;
		}

		ApplyRedirectAddress(bindReq->localAddressAndPort, redirectAddress);
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

void FreeMemory(PVOID ptr)
{
    if (ptr != nullptr)
    {
        ExFreePoolWithTag(ptr, ProtonTAG);
    }
}

PVOID AllocateMemory(size_t size)
{
    return ExAllocatePool2(POOL_FLAG_NON_PAGED, size, ProtonTAG);
}

void NTAPI CompleteBasicPacketInjection(VOID* data,
	_Inout_ NET_BUFFER_LIST* bufferList,
	_In_ BOOLEAN)
{
	PNET_BUFFER buffer = NET_BUFFER_LIST_FIRST_NB(bufferList);
	PMDL mdl = NET_BUFFER_FIRST_MDL(buffer);
	FreeMemory(data);
	IoFreeMdl(mdl);
	FwpsFreeNetBufferList0(bufferList);
}

void PacketDnsReplyInitv4(PDNSPACKETV4 packet)
{
	memset(packet, 0, sizeof(DNSPACKETV4));
	packet->ip.Version = 4;
	packet->ip.HdrLength = sizeof(IPHDR) / sizeof(UINT32);
	packet->ip.Length = htons(sizeof(DNSPACKETV4));
	packet->ip.Protocol = IPPROTO_UDP;
	packet->ip.Id = 0;
	packet->ip.TTL = 64;
	packet->udp.Length = ntohs(sizeof(UDPHDR) + sizeof(DNSHEADER));
}

UINT16 CalcChecksum(const PVOID data, UINT len)
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

bool BlockDnsPacket(PNET_BUFFER buffer, UINT32 interface_index, UINT32 subinterface_index)
{
	const UINT total_len = NET_BUFFER_DATA_LENGTH(buffer);
	if (total_len < sizeof(IPHDR) || total_len > MAX_PACKET_SIZE)
	{
		return false;
	}

	PVOID buffer_ptr = AllocateMemory(total_len);
	if (buffer_ptr == nullptr)
	{
		return false;
	}

	auto* const buffer_data = static_cast<unsigned char*>(buffer_ptr);
	auto* result = NdisGetDataBuffer(buffer, total_len, buffer_data, 1, 0);
	if (result == nullptr)
	{
		FreeMemory(buffer_ptr);
		return false;
	}

	auto* ip_header = reinterpret_cast<PIPHDR>(buffer_data);
	if (ip_header == nullptr)
	{
		FreeMemory(buffer_ptr);
		return false;
	}

	const UINT ip_header_len = ip_header->HdrLength * sizeof(UINT32);
	if (ip_header->Version != 4 ||
		ntohs(ip_header->Length) != total_len ||
		ip_header_len < sizeof(IPHDR) ||
		ip_header->Protocol != IPPROTO_UDP ||
		total_len < ip_header_len + sizeof(UDPHDR) + sizeof(DNSHEADER))
	{
		FreeMemory(buffer_ptr);
		return false;
	}

	auto* udp_header = reinterpret_cast<PUDPHDR>(buffer_data + ip_header_len);
	if (ntohs(udp_header->DstPort) != 53)
	{
		FreeMemory(buffer_ptr);
		return false;
	}

	const UINT udp_header_len = sizeof(UDPHDR);
	const UINT udp_payload_size = total_len - ip_header_len - udp_header_len;
	auto* const dns_header_size = buffer_data + ip_header_len + udp_header_len;
	auto* dns_header = reinterpret_cast<PDNSHEADER>(dns_header_size);
	const UINT reply_size = sizeof(IPHDR) + sizeof(UDPHDR) + udp_payload_size;

	PVOID reply_ptr = AllocateMemory(MAX_PACKET_SIZE);
	if (reply_ptr == nullptr)
	{
		FreeMemory(buffer_ptr);
		return false;
	}

	auto* const reply = static_cast<unsigned char*>(reply_ptr);
	auto dns_packet = reinterpret_cast<DNSPACKETV4*>(reply);
	PacketDnsReplyInitv4(dns_packet);

	//28 = 20 bytes of IP header and 8 bytes of UDP header. So we skip those
	RtlCopyMemory(reply + sizeof(IPHDR) + sizeof(UDPHDR), dns_header_size, udp_payload_size);

	dns_packet->dns.flags = htons(0x8002);
	dns_packet->ip.SrcAddr = ip_header->DstAddr;
	dns_packet->ip.DstAddr = ip_header->SrcAddr;
	dns_packet->ip.Length = htons(static_cast<UINT16>(reply_size));
	dns_packet->udp.SrcPort = udp_header->DstPort;
	dns_packet->udp.DstPort = udp_header->SrcPort;
	dns_packet->udp.Length = htons(udp_payload_size + sizeof(UDPHDR));
	dns_packet->ip.Checksum = CalcChecksum(&dns_packet->ip, dns_packet->ip.HdrLength * sizeof(UINT32));
	dns_packet->dns.transaction_id = dns_header->transaction_id;

	auto* mdl_copy = IoAllocateMdl(reply_ptr, reply_size, FALSE, FALSE, nullptr);
	if (mdl_copy == nullptr)
	{
		goto block_dns_failed;
	}

	MmBuildMdlForNonPagedPool(mdl_copy);

	PNET_BUFFER_LIST cloned_net_buffer_list = nullptr;
	auto status = FwpsAllocateNetBufferAndNetBufferList0(
		nbl_pool_handle,
		0,
		0,
		mdl_copy,
		0,
		reply_size,
		&cloned_net_buffer_list);

	if (!NT_SUCCESS(status))
	{
		goto block_dns_failed;
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
		reply_ptr);

	if (!NT_SUCCESS(status))
	{
		goto block_dns_failed;
	}

	FreeMemory(buffer_ptr);

	return true;

block_dns_failed:

	if (mdl_copy != nullptr)
	{
		IoFreeMdl(mdl_copy);
	}

	FreeMemory(buffer_ptr);
	FreeMemory(reply_ptr);

	return false;
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
	if ((result->rights & FWPS_RIGHT_ACTION_WRITE) == 0 || packet == nullptr)
	{
		return;
	}

	result->actionType = FWP_ACTION_PERMIT;

	auto* const buffers = static_cast<PNET_BUFFER_LIST>(packet);

	if (NET_BUFFER_LIST_NEXT_NBL(buffers) != nullptr)
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