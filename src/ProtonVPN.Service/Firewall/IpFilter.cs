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

namespace ProtonVPN.Service.Firewall;

public class IpFilter : IStartable
{
    public static Guid DnsCalloutGuid = Guid.Parse("{10636af3-50d6-4f53-acb7-d5af33217fcb}");
    private readonly Guid _providerGuid = Guid.Parse("{20865f68-0b04-44da-bb83-2238622540fa}");
    private readonly Guid _sublayerGuid = Guid.Parse("{aa867e71-5765-4be3-9399-581585c226ce}");

    private readonly ILogger _logger;
    private const int PERMANENT_SUBLAYER_WEIGHT = 1000;
    private const int DYNAMIC_SUBLAYER_WEIGHT = 1001;

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
            new DisplayData { Name = "ProtonVPN Dynamic Provider" });

        DynamicSublayer = DynamicInstance.CreateSublayer(new DisplayData { Name = "ProtonVPN Dynamic Sublayer" },
            DYNAMIC_SUBLAYER_WEIGHT);
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
            try
            {
                ExecuteTransaction(session, () =>
                {
                    NetworkFilter.IpFilter instance = new(session, _providerGuid);
                    CreateCallout(instance);
                });
            }
            catch (NetworkFilterException e)
            {
                _logger.Error<FirewallLog>("Error when creating the callout for an already existing Provider.", e);
                throw;
            }
            finally
            {
                session.Close();
            }

            return;
        }

        try
        {
            ExecuteTransaction(session, () =>
            {
                NetworkFilter.IpFilter instance = NetworkFilter.IpFilter.Create(session,
                    new DisplayData { Name = "ProtonVPN Permanent Provider" },
                    true,
                    _providerGuid);

                instance.CreateSublayer(new DisplayData { Name = "ProtonVPN Permanent Sublayer" },
                    PERMANENT_SUBLAYER_WEIGHT,
                    true,
                    _sublayerGuid);

                CreateCallout(instance);
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

    private void CreateCallout(NetworkFilter.IpFilter instance)
    {
        instance.CreateCallout(
            new DisplayData
            {
                Name = "ProtonVPN block dns callout",
                Description = "Sends server failure packet response for non TAP/TUN DNS queries.",
            },
            DnsCalloutGuid,
            Layer.OutboundIPPacketV4,
            true);
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