/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Runtime.InteropServices;
using Polly;
using Polly.Retry;

namespace ProtonVPN.NetworkFilter
{
    internal class IpFilterNative
    {
        private const uint ErrorSuccess = 0;
        private const uint ErrorFilterNotFound = 0x80320003;
        private const uint ErrorCalloutNotFound = 0x80320001;
        private const uint ErrorTimeout = 0x80320012;
        private const int RetryCount = 3;

        private static readonly RetryPolicy RetryPolicy = Policy
            .Handle<NetworkFilterException>(e => e.Code == ErrorTimeout)
            .Retry(RetryCount);

        public static IntPtr CreateDynamicSession()
        {
            IntPtr handle = IntPtr.Zero;

            AssertSuccess(
                () => PInvoke.CreateDynamicSession(ref handle));

            return handle;
        }

        public static IntPtr CreateSession()
        {
            IntPtr handle = IntPtr.Zero;

            AssertSuccess(
                () => PInvoke.CreateSession(ref handle));

            return handle;
        }

        public static void DestroySession(IntPtr handle)
        {
            AssertSuccess(() => PInvoke.DestroySession(handle));
        }

        public static void StartTransaction(IntPtr sessionHandle)
        {
            AssertSuccess(() => PInvoke.StartTransaction(sessionHandle));
        }

        public static void AbortTransaction(IntPtr sessionHandle)
        {
            AssertSuccess(() => PInvoke.AbortTransaction(sessionHandle));
        }

        public static void CommitTransaction(IntPtr sessionHandle)
        {
            AssertSuccess(() => PInvoke.CommitTransaction(sessionHandle));
        }

        public static Guid CreateProvider(
            IntPtr sessionHandle,
            DisplayData displayData,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateProvider(
                sessionHandle,
                ref displayData,
                (uint) (persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static bool IsProviderRegistered(
            IntPtr sessionHandle,
            Guid id)
        {
            uint result = 0;

            AssertSuccess(() => PInvoke.IsProviderRegistered(
                sessionHandle,
                ref id,
                ref result));

            return result == 0;
        }

        public static void DestroyProvider(
            IntPtr sessionHandle,
            Guid id)
        {
            AssertSuccess(() => PInvoke.DestroyProvider(
                sessionHandle,
                ref id));
        }

        public static Guid CreateProviderContext(
            IntPtr sessionHandle,
            Guid providerId,
            DisplayData displayData,
            byte[] data,
            bool persistent = false,
            Guid id = new())
        {
            IntPtr dataPtr = IntPtr.Zero;
            int size = 0;
            if (data != null && (size = data.Length) != 0)
            {
                dataPtr = Marshal.AllocHGlobal(size);
                Marshal.Copy(data, 0, dataPtr, size);
            }

            try
            {
                AssertSuccess(() => PInvoke.CreateProviderContext(
                    sessionHandle,
                    ref displayData,
                    ref providerId,
                    (uint) size,
                    dataPtr,
                    (uint)(persistent ? 1 : 0),
                    ref id));
            }
            finally
            {
                Marshal.FreeHGlobal(dataPtr);
            }

            return id;
        }

        public static void DestroyProviderContext(
            IntPtr sessionHandle,
            Guid contextId)
        {
            AssertSuccess(() => PInvoke.DestroyProviderContext(
                sessionHandle,
                ref contextId));
        }

        public static Guid CreateCallout(
            IntPtr sessionHandle,
            Guid key,
            Guid providerId,
            DisplayData displayData,
            Layer layer,
            bool persistent = false)
        {
            Guid id = key;

            AssertSuccess(() => PInvoke.CreateCallout(
                sessionHandle,
                ref displayData,
                ref providerId,
                (uint)layer,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static void DestroyCallout(
            IntPtr sessionHandle,
            Guid calloutId)
        {
            AssertSuccess(() => PInvoke.DestroyCallout(
                sessionHandle,
                ref calloutId));
        }

        public static Guid CreateSublayer(
            IntPtr sessionHandle,
            Guid providerId,
            DisplayData displayData,
            uint weight,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateSublayer(
                sessionHandle,
                ref providerId,
                ref displayData,
                weight,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static void DestroySublayer(
            IntPtr sessionHandle,
            Guid sublayerId)
        {
            AssertSuccess(() => PInvoke.DestroySublayer(
                sessionHandle,
                ref sublayerId));
        }

        public static bool DoesSublayerExist(
            IntPtr sessionHandle,
            Guid id)
        {
            uint result = 0;

            AssertSuccess(() => PInvoke.DoesSublayerExist(
                sessionHandle,
                ref id,
                ref result));

            return result == 0;
        }

        public static bool DoesFilterExist(
            IntPtr sessionHandle,
            Guid id)
        {
            uint result = 0;

            AssertSuccess(() => PInvoke.DoesFilterExist(
                sessionHandle,
                ref id,
                ref result));

            return result == 0;
        }

        public static bool DoesProviderContextExist(
            IntPtr sessionHandle,
            Guid id)
        {
            uint result = 0;

            AssertSuccess(() => PInvoke.DoesProviderContextExist(
                sessionHandle,
                ref id,
                ref result));

            return result == 0;
        }

        public static bool DoesCalloutExist(
            IntPtr sessionHandle,
            Guid id)
        {
            uint result = 0;

            AssertSuccess(() => PInvoke.DoesCalloutExist(
                sessionHandle,
                ref id,
                ref result));

            return result == 0;
        }

        public static void DestroySublayerFilters(IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId)
        {
            AssertSuccess(() => PInvoke.DestroySublayerFilters(
                sessionHandle,
                ref providerId,
                ref sublayerId));
        }

        public static void DestroySublayerFiltersByName(IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            string name)
        {
            AssertSuccess(() => PInvoke.DestroySublayerFiltersByName(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                name));
        }

        public static uint GetSublayerFilterCount(IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId)
        {
            uint total = 0;

            AssertSuccess(() => PInvoke.GetSublayerFilterCount(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref total));

            return total;
        }

        public static void DestroyCallouts(IntPtr sessionHandle, Guid providerId)
        {
            AssertSuccess(() => PInvoke.DestroyCallouts(sessionHandle, ref providerId));
        }

        public static void DestroyFilter(
            IntPtr sessionHandle,
            Guid id)
        {
            AssertSuccess(() => PInvoke.DestroyFilter(sessionHandle, ref id));
        }

        public static Guid CreateLayerFilter(IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateLayerFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid CreateRemoteIPv4Filter(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            string ipAddress,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateRemoteIPv4Filter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                ipAddress,
                (uint)(persistent? 1 : 0),
                ref id));

            return id;
        }

        public static Guid CreateAppFilter(IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            string appPath,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateAppFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                appPath,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid CreateRemoteTcpPortFilter(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            uint port,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateRemoteTCPPortFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                port,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid CreateRemoteUdpPortFilter(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            uint port,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateRemoteUDPPortFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                port,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid CreateRemoteNetworkIPFilter(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            NetworkAddress address,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateRemoteNetworkIPFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                ref address,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid CreateNetInterfaceFilter(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            uint index,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateNetInterfaceFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                index,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid CreateLoopbackFilter(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.CreateLoopbackFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid BlockOutsideDns(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            uint index,
            uint persistent)
        {
            Guid id = Guid.Empty;

            AssertSuccess(() => PInvoke.BlockOutsideDns(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                index,
                persistent,
                ref id));

            return id;
        }

        public static Guid BlockOutsideOpenVpn(IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            uint weight,
            string appPath,
            string serverIpAddress,
            uint persistent,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.BlockOutsideOpenVpn(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                weight,
                appPath,
                serverIpAddress,
                persistent,
                ref id));

            return id;
        }

        public static Guid PermitRouterSolicitationMessage(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.PermitRouterSolicitationMessage(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid PermitRouterAdvertisementMessage(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.PermitRouterAdvertisementMessage(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid PermitNeighborSolicitationMessage(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.PermitNeighborSolicitationMessage(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid PermitNeighborAdvertisementMessage(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.PermitNeighborAdvertisementMessage(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid PermitIcmpRedirectMessage(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.PermitIcmpRedirectMessage(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid PermitOutboundIpv6Dhcp(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.PermitOutboundIpv6Dhcp(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        public static Guid PermitInboundIpv6Dhcp(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            Guid calloutId,
            Guid providerContextId,
            bool persistent = false,
            Guid id = new())
        {
            AssertSuccess(() => PInvoke.PermitInboundIpv6Dhcp(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref calloutId,
                ref providerContextId,
                (uint)(persistent ? 1 : 0),
                ref id));

            return id;
        }

        private static void AssertSuccess(Func<uint> function)
        {
            RetryPolicy.Execute(() => AssertSuccessInner(function));
        }

        private static void AssertSuccessInner(Func<uint> function)
        {
            uint status;
            try
            {
                status = function();
            }
            catch (SEHException ex)
            {
                throw new NetworkFilterException(ex.ErrorCode, ex);
            }

            switch (status)
            {
                case ErrorSuccess:
                    return;
                case ErrorFilterNotFound:
                    throw new FilterNotFoundException(status);
                case ErrorCalloutNotFound:
                    throw new CalloutNotFoundException(status);
                default:
                    throw new NetworkFilterException(status);
            }
        }
    }
}
