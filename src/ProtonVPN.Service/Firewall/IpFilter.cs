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
using Autofac;
using ProtonVPN.Common.Logging;
using ProtonVPN.NetworkFilter;

namespace ProtonVPN.Service.Firewall
{
    public class IpFilter : IStartable
    {
        public static Guid DnsCalloutGuid = Guid.Parse("{10636af3-50d6-4f53-acb7-d5af33217fcb}");
        private readonly Guid _providerGuid = Guid.Parse("{20865f68-0b04-44da-bb83-2238622540fa}");
        private readonly Guid _sublayerGuid = Guid.Parse("{aa867e71-5765-4be3-9399-581585c226ce}");

        private readonly ILogger _logger;
        private NetworkFilter.IpFilter _previousInstance;
        private Sublayer _previousSublayer;
        private const int SublayerWeight = 1000;

        public IpFilter(ILogger logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            CreatePermanentFilers();
        }

        public NetworkFilter.IpFilter Instance { get; private set; }

        public Sublayer Sublayer { get; private set; }

        public void StartSession(SessionType type)
        {
            _previousInstance = Instance;
            _previousSublayer = Sublayer;

            switch (type)
            {
                case SessionType.Permanent:
                    CreatePermanentSession();
                    break;
                case SessionType.Dynamic:
                    CreateDynamicSession();
                    break;
            }
        }

        public void ClosePreviousSession()
        {
            if (_previousInstance != null)
            {
                CloseSession(_previousInstance, _previousSublayer);
                _previousInstance = null;
                _previousSublayer = null;
            }
        }

        public void CloseCurrentSession()
        {
            if (Instance != null && Sublayer != null)
            {
                CloseSession(Instance, Sublayer);
                Instance = null;
                Sublayer = null;
            }
        }

        public void DeletePermanentFilters()
        {
            var instance = new NetworkFilter.IpFilter(Session.Permanent(), _providerGuid);
            var sublayer = new Sublayer(instance, _sublayerGuid);
            sublayer.DestroyAllFilters();
            instance.Session.Close();
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
            Instance = NetworkFilter.IpFilter.Create(
                Session.Dynamic(),
                new DisplayData("ProtonVPN", "ProtonVPN Dynamic Provider"));

            Sublayer = Instance.CreateSublayer(new DisplayData { Name = "ProtonVPN Firewall filters" }, SublayerWeight);
        }

        private void CreatePermanentSession()
        {
            Instance = new NetworkFilter.IpFilter(Session.Permanent(), _providerGuid);
            Sublayer = new Sublayer(Instance, _sublayerGuid);
        }

        private void CreatePermanentFilers()
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
                        new DisplayData("ProtonVPN", "ProtonVPN Permanent Provider"),
                        true,
                        _providerGuid);

                    instance.CreateSublayer(new DisplayData("ProtonVPN Permanent Sublayer", "Permanent Sublayer"),
                        SublayerWeight,
                        true,
                        _sublayerGuid);

                    instance.CreateCallout(
                        new DisplayData
                        {
                            Name = "ProtonVPN block dns callout",
                            Description = "Sends server failure packet response for non TAP/TUN DNS queries.",
                        },
                        DnsCalloutGuid,
                        Layer.OutboundIPPacketV4,
                        true);
                });
            }
            catch (NetworkFilterException e)
            {
                _logger.Error(e);
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