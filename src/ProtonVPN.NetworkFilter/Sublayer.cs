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
        private readonly HashSet<Guid> _callouts = new HashSet<Guid>();

        public Sublayer(IpFilter ipFilter, Guid id)
        {
            _ipFilter = ipFilter;
            Id = id;
        }

        public Guid Id { get; }

        public void DestroyAllFilters()
        {
            IpFilterNative.DestroySublayerFilters(Session.Handle, _ipFilter.ProviderId, Id);
        }

        public bool DoesFilterExist(Guid id)
        {
            return IpFilterNative.DoesFilterExist(Session.Handle, id);
        }

        public void DestroyAllCallouts()
        {
            foreach (var key in _callouts.ToList())
            {
                _ipFilter.DestroyCallout(key);
            }
        }

        public Callout CreateCallout(DisplayData displayData, Guid calloutId, Layer layer)
        {
            var callout = _ipFilter.CreateCallout(displayData, calloutId, layer);
            _callouts.Add(callout.Id);

            return callout;
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
        }

        public void DestroyCallout(Guid calloutId)
        {
            try
            {
                IpFilterNative.DestroyCallout(
                    Session.Handle,
                    calloutId);
            }
            catch (CalloutNotFoundException)
            {
            }

            _callouts.Remove(calloutId);
        }

        public Guid CreateLayerFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateLayerFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                Guid.Empty,
                Guid.Empty,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateLayerCalloutFilter(
            DisplayData displayData,
            Layer layer,
            uint weight,
            Callout callout,
            ProviderContext providerContext,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateLayerFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                Action.Callout,
                weight,
                callout.Id, 
                providerContext.Id,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteIPv4Filter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            string address,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateRemoteIPv4Filter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                Guid.Empty,
                Guid.Empty,
                address,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteIPv4CalloutFilter(DisplayData displayData,
            Layer layer,
            uint weight,
            Callout callout,
            ProviderContext providerContext,
            string address,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateRemoteIPv4Filter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                Action.Callout,
                weight,
                callout.Id,
                providerContext.Id,
                address,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateAppFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            string appPath,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateAppFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                Guid.Empty, 
                Guid.Empty,
                appPath,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateAppCalloutFilter(DisplayData displayData,
            Layer layer,
            uint weight,
            Callout callout,
            ProviderContext providerContext,
            string appPath,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateAppFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                Action.Callout,
                weight,
                callout.Id,
                providerContext.Id,
                appPath,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteNetworkIPv4Filter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            NetworkAddress addr,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateRemoteNetworkIPv4Filter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                Guid.Empty,
                Guid.Empty,
                addr,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteNetworkIPv4CalloutFilter(
            DisplayData displayData,
            Layer layer,
            uint weight,
            Callout callout,
            ProviderContext providerContext,
            NetworkAddress addr,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateRemoteNetworkIPv4Filter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                Action.Callout,
                weight,
                callout.Id,
                providerContext.Id,
                addr,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteUdpPortFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            uint port,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateRemoteUdpPortFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                port,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateRemoteTcpPortFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            uint port,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateRemoteTcpPortFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                port,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateNetInterfaceFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint index,
            uint weight,
            bool persistent = false,
            Guid id = new Guid())
        {
            Guid filterId = IpFilterNative.CreateNetInterfaceFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                index,
                persistent,
                id);

            AddFilter(filterId);

            return filterId;
        }

        public Guid CreateLoopbackFilter(
            DisplayData displayData,
            Action action,
            Layer layer,
            uint weight,
            bool persistent = false)
        {
            Guid filterId = IpFilterNative.CreateLoopbackFilter(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                action,
                weight,
                persistent);

            AddFilter(filterId);

            return filterId;
        }

        public Guid BlockOutsideDns(DisplayData displayData,
            Layer layer,
            uint weight,
            Callout callout,
            uint index,
            bool persistent = false)
        {
            Guid filterId = IpFilterNative.BlockOutsideDns(
                Session.Handle,
                ProviderId,
                Id,
                displayData,
                layer,
                Action.Callout,
                weight,
                callout.Id,
                index,
                (uint) (persistent ? 1 : 0));

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