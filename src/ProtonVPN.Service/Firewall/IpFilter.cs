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
using Autofac;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.FirewallLogs;
using ProtonVPN.NetworkFilter;

namespace ProtonVPN.Service.Firewall
{
    public class IpFilter : IStartable
    {
        private readonly Guid _providerGuid = Guid.Parse("{20865f68-0b04-44da-bb83-2238622540fa}");
        private readonly Guid _sublayerGuid = Guid.Parse("{aa867e71-5765-4be3-9399-581585c226ce}");

        private readonly ILogger _logger;
        private const int PermanentSublayerWeight = 1000;
        private const int DynamicSublayerWeight = 1001;

        public IpFilter(ILogger logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            CreatePermanentFilters();
            CreateDynamicSession();
            CreatePermanentSession();
        }

        public NetworkFilter.IpFilter PermanentInstance { get; private set; }
        public NetworkFilter.IpFilter DynamicInstance { get; private set; }

        public Sublayer PermanentSublayer { get; private set; }
        public Sublayer DynamicSublayer { get; private set; }

        public Sublayer GetSublayer(SessionType type)
        {
            return type == SessionType.Dynamic ? DynamicSublayer : PermanentSublayer;
        }

        public void CloseSession(NetworkFilter.IpFilter instance, Sublayer sublayer)
        {
            if (instance.Session.Type == SessionType.Permanent)
            {
                sublayer.DestroyAllFilters();
            }

            instance.Session.Close();
        }

        private void CreateDynamicSession()
        {
            DynamicInstance = NetworkFilter.IpFilter.Create(
                Session.Dynamic(),
                new DisplayData {Name = "ProtonVPN Dynamic Provider"});

            DynamicSublayer = DynamicInstance.CreateSublayer(new DisplayData {Name = "ProtonVPN Dynamic Sublayer"},
                DynamicSublayerWeight);
        }

        private void CreatePermanentSession()
        {
            PermanentInstance = new NetworkFilter.IpFilter(Session.Permanent(), _providerGuid);
            PermanentSublayer = new Sublayer(PermanentInstance, _sublayerGuid);
        }

        private void CreatePermanentFilters()
        {
            var session = Session.Permanent();
            if (NetworkFilter.IpFilter.IsRegistered(session, _providerGuid))
            {
                session.Close();
                return;
            }

            try
            {
                ExecuteTransaction(session, () =>
                {
                    NetworkFilter.IpFilter instance = NetworkFilter.IpFilter.Create(session,
                        new DisplayData {Name = "ProtonVPN Permanent Provider"},
                        true,
                        _providerGuid);

                    instance.CreateSublayer(new DisplayData {Name = "ProtonVPN Permanent Sublayer"},
                        PermanentSublayerWeight,
                        true,
                        _sublayerGuid);
                });
            }
            catch (NetworkFilterException e)
            {
                _logger.Error<FirewallLog>("Error when creating permanent IP filtering.", e);
                throw;
            }
            finally
            {
                session.Close();
            }
        }

        private void ExecuteTransaction(Session session, System.Action action)
        {
            session.StartTransaction();

            try
            {
                action();
                session.CommitTransaction();
            }
            catch (NetworkFilterException)
            {
                session.AbortTransaction();
                throw;
            }
        }
    }
}