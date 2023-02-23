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

namespace ProtonVPN.NetworkFilter
{
    public class IpFilter
    {
        public IpFilter(Session session, Guid providerId)
        {
            Session = session;
            ProviderId = providerId;
        }

        public Session Session { get; }
        public Guid ProviderId { get; }

        public static IpFilter Create(Session session, DisplayData displayData, bool persistent = false, Guid providerId = new Guid())
        {
            return new IpFilter(
                session,
                IpFilterNative.CreateProvider(session.Handle, displayData, persistent, providerId));
        }

        public static bool IsRegistered(Session session, Guid providerId)
        {
            return IpFilterNative.IsProviderRegistered(session.Handle, providerId);
        }

        public static void Destroy(Session session, Guid providerId)
        {
            IpFilterNative.DestroyProvider(session.Handle, providerId);
        }

        public void Destroy()
        {
            IpFilterNative.DestroyProvider(Session.Handle, ProviderId);
        }

        public Sublayer CreateSublayer(DisplayData displayData, uint weight, bool persistent = false, Guid id = new Guid())
        {
            var sublayerId = IpFilterNative.CreateSublayer(
                Session.Handle,
                ProviderId,
                displayData,
                weight,
                persistent,
                id);

            return new Sublayer(this, sublayerId);
        }

        public ProviderContext CreateProviderContext(DisplayData displayData, byte[] data, bool persistent = false, Guid id = new Guid())
        {
            var providerContextId = IpFilterNative.CreateProviderContext(
                Session.Handle,
                ProviderId,
                displayData,
                data,
                persistent,
                id);

            return new ProviderContext(providerContextId);
        }

        public Callout CreateCallout(DisplayData displayData, Guid key, Layer layer, bool persistent = false)
        {
            var calloutId = IpFilterNative.CreateCallout(
                Session.Handle,
                key,
                ProviderId,
                displayData,
                layer,
                persistent);

            return new Callout(calloutId);
        }
    }
}