/*
 * Copyright (c) 2020 Proton Technologies AG
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
        private const uint ErrorTimeout = 0x80320012;
        private const int RetryCount = 3;

        private static readonly RetryPolicy _retryPolicy = Policy
            .Handle<NetworkFilterException>(e => e.Code == ErrorTimeout)
            .Retry(RetryCount);

        public static IntPtr CreateDynamicSession()
        {
            var handle = IntPtr.Zero;

            AssertSuccess(
                () => PInvoke.CreateDynamicSession(ref handle));

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
            DisplayData displayData)
        {
            var id = Guid.Empty;

            AssertSuccess(() => PInvoke.CreateProvider(
                sessionHandle,
                ref displayData,
                ref id));

            return id;
        }

        public static Guid CreateProviderContext(
            IntPtr sessionHandle,
            Guid providerId,
            DisplayData displayData,
            byte[] data)
        {
            var id = Guid.Empty;
            var dataPtr = IntPtr.Zero;
            var size = 0;
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
            Layer layer)
        {
            var id = key;

            AssertSuccess(() => PInvoke.CreateCallout(
                sessionHandle,
                ref displayData,
                ref providerId,
                (uint)layer,
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
            uint weight)
        {
            var id = Guid.Empty;

            AssertSuccess(() => PInvoke.CreateSublayer(
                sessionHandle,
                ref providerId,
                ref displayData,
                weight,
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
            Guid providerContextId)
        {
            var id = Guid.Empty;

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
            string ipAddress)
        {
            var id = Guid.Empty;

            AssertSuccess(() => PInvoke.CreateRemoteIPv4Filter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ipAddress,
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
            string appPath)
        {
            var id = Guid.Empty;

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
            uint port)
        {
            var id = Guid.Empty;

            AssertSuccess(() => PInvoke.CreateRemoteTCPPortFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                port,
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
            uint port)
        {
            var id = Guid.Empty;

            AssertSuccess(() => PInvoke.CreateRemoteUDPPortFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                port,
                ref id));

            return id;
        }

        public static Guid CreateRemoteNetworkIPv4Filter(
            IntPtr sessionHandle,
            Guid providerId,
            Guid sublayerId,
            DisplayData displayData,
            Layer layer,
            Action action,
            uint weight,
            NetworkAddress address)
        {
            var id = Guid.Empty;

            AssertSuccess(() => PInvoke.CreateRemoteNetworkIPv4Filter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref address,
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
            string interfaceId)
        {
            var id = Guid.Empty;

            AssertSuccess(() => PInvoke.CreateNetInterfaceFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                interfaceId,
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
            uint weight)
        {
            var id = Guid.Empty;

            AssertSuccess(() => PInvoke.CreateLoopbackFilter(
                sessionHandle,
                ref providerId,
                ref sublayerId,
                ref displayData,
                (uint)layer,
                (uint)action,
                weight,
                ref id));

            return id;
        }

        private static void AssertSuccess(Func<uint> function)
        {
            _retryPolicy.Execute(() => AssertSuccessInner(function));
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
                default:
                    throw new NetworkFilterException(status);
            }
        }
    }
}
