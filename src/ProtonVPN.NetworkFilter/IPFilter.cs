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

namespace ProtonVPN.NetworkFilter
{
    public class IpFilter
    {
        protected IpFilter(Session session, Guid providerId)
        {
            Session = session;
            ProviderId = providerId;
        }

        public Session Session { get; }
        public Guid ProviderId { get; }

        public static IpFilter Create(Session session, DisplayData displayData)
        {
            return new IpFilter(
                session,
                IpFilterNative.CreateProvider(session.Handle, displayData));
        }

        public Sublayer CreateSublayer(DisplayData displayData, uint weight)
        {
            var id = IpFilterNative.CreateSublayer(
                Session.Handle,
                ProviderId,
                displayData,
                weight);

            return new Sublayer(this, id);
        }

        public void DestroySublayer(Sublayer sublayer)
        {
            sublayer.DestroyAllFilters();

            IpFilterNative.DestroySublayer(
                Session.Handle,
                sublayer.Id);
        }

        public ProviderContext CreateProviderContext(DisplayData displayData, byte[] data)
        {
            var id = IpFilterNative.CreateProviderContext(
                Session.Handle,
                ProviderId,
                displayData,
                data);

            return new ProviderContext(id);
        }

        public void DestroyProviderContext(ProviderContext context)
        {
            IpFilterNative.DestroyProviderContext(
                Session.Handle,
                context.Id);
        }

        public Callout CreateCallout(DisplayData displayData, Guid key, Layer layer)
        {
            var id = IpFilterNative.CreateCallout(
                Session.Handle,
                key,
                ProviderId,
                displayData,
                layer);

            return new Callout(id);
        }

        public void DestroyCallout(Callout callout)
        {
            IpFilterNative.DestroyCallout(
                Session.Handle,
                callout.Id);
        }
    }
}
