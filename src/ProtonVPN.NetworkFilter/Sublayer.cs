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
using System.Collections.Generic;
using System.Linq;

namespace ProtonVPN.NetworkFilter
{
    public class Sublayer
    {
        private readonly IpFilter _ipFilter;
        private readonly HashSet<Guid> _filters = new HashSet<Guid>();

        public Sublayer(IpFilter ipFilter, Guid id)
        {
            _ipFilter = ipFilter;
            Id = id;
        }

        public Guid Id { get; }

        public void DestroyAllFilters()
        {
            foreach (var filter in _filters.ToList())
            {
                DestroyFilter(filter);
            }
        }

        public void DestroyFilter(Guid filterId)
        {
            try
            {
                IpFilterNative.DestroyFilter(
                    Session.Handle,
                    filterId);
            }
            catch (FilterNotFoundException)
            {
            }

            RemoveFilter(filterId);
        }

        public Guid CreateLayerFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight)
        {
            var filterId = IpFilterNative.CreateLayerFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight, Guid.Empty, Guid.Empty);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateLayerCalloutFilter(
            DisplayData displayData,
            Layer layer,
            uint weight,
            Callout callout,
            ProviderContext providerContext)
        {
            var filterId = IpFilterNative.CreateLayerFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                Action.Callout,
                weight,
                callout.Id, 
                providerContext.Id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteIPv4Filter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            string address)
        {
            var filterId = IpFilterNative.CreateRemoteIPv4Filter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                address);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateAppFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            string appPath)
        {
            var filterId = IpFilterNative.CreateAppFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                Guid.Empty, 
                Guid.Empty,
                appPath);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateAppCalloutFilter(DisplayData displayData,
            Layer layer,
            uint weight,
            Callout callout,
            ProviderContext providerContext,
            string appPath)
        {
            var filterId = IpFilterNative.CreateAppFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                Action.Callout,
                weight,
                callout.Id,
                providerContext.Id,
                appPath);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteNetworkIPv4Filter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            NetworkAddress addr)
        {
            var filterId = IpFilterNative.CreateRemoteNetworkIPv4Filter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                addr);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteUdpPortFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            uint port)
        {
            var filterId = IpFilterNative.CreateRemoteUdpPortFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                port);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteTcpPortFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            uint port)
        {
            var filterId = IpFilterNative.CreateRemoteTcpPortFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                port);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateNetInterfaceFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            string interfaceId)
        {
            var filterId = IpFilterNative.CreateNetInterfaceFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                interfaceId);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateLoopbackFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight)
        {
            var filterId = IpFilterNative.CreateLoopbackFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight);

            AddFilter(filterId);

            return filterId;
        }

        private void AddFilter(Guid id)
        {
            _filters.Add(id);
        }

        private void RemoveFilter(Guid id)
        {
            _filters.Remove(id);
        }

        private Session Session => _ipFilter.Session;

        private Guid ProviderId => _ipFilter.ProviderId;
    }
}
