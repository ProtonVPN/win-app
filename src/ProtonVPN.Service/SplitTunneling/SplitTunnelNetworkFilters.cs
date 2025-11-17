/*
 * Copyright (c) 2025 Proton AG
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
using System.Net;
using ProtonVPN.NetworkFilter;

namespace ProtonVPN.Service.SplitTunneling;

public class SplitTunnelNetworkFilters
{
    private const uint WFP_SUB_LAYER_WEIGHT = 10001;

    private static readonly Guid _connectRedirectV4CalloutKey = Guid.Parse("{3c5a284f-af01-51fa-4361-6c6c50424144}");
    private static readonly Guid _connectRedirectV6CalloutKey = Guid.Parse("{3c5a284f-af01-51fa-4361-6c6c50424145}");
    private static readonly Guid _bindRedirectV4CalloutKey = Guid.Parse("{10636af3-50d6-4f53-acb7-d5af33217fca}");
    private static readonly Guid _bindRedirectV6CalloutKey = Guid.Parse("{10636af3-50d6-4f53-acb7-d5af33217faa}");

    private IpFilter _ipFilter;
    private Sublayer _subLayer;

    public void EnableExcludeMode(string[] apps, IPAddress localIpv4Address, IPAddress localIpv6Address)
    {
        Create();

        _ipFilter.Session.StartTransaction();
        try
        {
            Redirect(apps, localIpv4Address, localIpv6Address);

            _ipFilter.Session.CommitTransaction();
        }
        catch
        {
            _ipFilter.Session.AbortTransaction();
            throw;
        }
    }

    public void EnableIncludeMode(string[] apps, IPAddress serverIpv4Address, IPAddress serverIpv6Address)
    {
        Create();

        _ipFilter.Session.StartTransaction();
        try
        {
            Redirect(apps, serverIpv4Address, serverIpv6Address);

            _ipFilter.Session.CommitTransaction();
        }
        catch
        {
            _ipFilter.Session.AbortTransaction();
            throw;
        }
    }

    private void Redirect(string[] apps, IPAddress ipv4Address, IPAddress ipv6Address)
    {
        Callout connectRedirectCalloutV4 = CreateConnectRedirectCallout(Layer.AppConnectRedirectV4, _connectRedirectV4CalloutKey);
        Callout bindRedirectCalloutV4 = CreateUDPRedirectCallout(Layer.BindRedirectV4, _bindRedirectV4CalloutKey);

        ProviderContext providerContextV4 = GetProviderContext(ipv4Address);
        CreateAppCalloutFilters(apps, bindRedirectCalloutV4, Layer.BindRedirectV4, providerContextV4);
        CreateAppCalloutFilters(apps, connectRedirectCalloutV4, Layer.AppConnectRedirectV4, providerContextV4);

        if (ipv6Address is not null)
        {
            ProviderContext providerContextV6 = GetProviderContext(ipv6Address);
            Callout connectRedirectCalloutV6 = CreateConnectRedirectCallout(Layer.AppConnectRedirectV6, _connectRedirectV6CalloutKey);
            Callout redirectUDPCalloutV6 = CreateUDPRedirectCallout(Layer.BindRedirectV6, _bindRedirectV6CalloutKey);

            CreateAppCalloutFilters(apps, connectRedirectCalloutV6, Layer.AppConnectRedirectV6, providerContextV6);
            CreateAppCalloutFilters(apps, redirectUDPCalloutV6, Layer.BindRedirectV6, providerContextV6);
        }
    }

    private ProviderContext GetProviderContext(IPAddress ipAddress)
    {
        return _ipFilter.CreateProviderContext(
            new DisplayData
            {
                Name = "ProtonVPN Split Tunnel redirect context",
                Description = "Instructs the callout driver where to redirect network connections",
            },
            new ConnectRedirectData(ipAddress));
    }

    public void Disable()
    {
        Remove();
    }

    private void Create()
    {
        _ipFilter = IpFilter.Create(
            Session.Dynamic(),
            new DisplayData { Name = "Proton AG", Description = "ProtonVPN Split Tunnel provider" });

        _subLayer = _ipFilter.CreateSublayer(
            new DisplayData { Name = "ProtonVPN Split Tunnel filters" },
            WFP_SUB_LAYER_WEIGHT);
    }

    private void Remove()
    {
        _ipFilter?.Session.Close();
        _ipFilter = null;
        _subLayer = null;
    }

    private void CreateAppCalloutFilters(string[] apps, Callout callout, Layer layer, ProviderContext providerContext)
    {
        foreach (string app in apps)
        {
            SafeCreateAppFilter(app, callout, layer, providerContext);
        }
    }

    private void SafeCreateAppFilter(string app, Callout callout, Layer layer, ProviderContext providerContext)
    {
        try
        {
            CreateAppFilter(app, callout, layer, providerContext);
        }
        catch (NetworkFilterException)
        {
        }
    }

    private void CreateAppFilter(string app, Callout callout, Layer layer, ProviderContext providerContext)
    {
        _subLayer.CreateAppCalloutFilter(
            new DisplayData
            {
                Name = "ProtonVPN Split Tunnel redirect app",
                Description = "Redirects network connections of the app"
            },
            layer,
            15,
            callout,
            providerContext,
            app,
            false);
    }

    private Callout CreateConnectRedirectCallout(Layer layer, Guid calloutKey)
    {
        return _ipFilter.CreateCallout(
            new DisplayData
            {
                Name = "ProtonVPN Split Tunnel callout",
                Description = "Redirects network connections",
            },
            calloutKey,
            layer
        );
    }

    private Callout CreateUDPRedirectCallout(Layer layer, Guid calloutKey)
    {
        return _ipFilter.CreateCallout(
            new DisplayData
            {
                Name = "ProtonVPN Split Tunnel callout",
                Description = "Redirects UDP network flow",
            },
            calloutKey,
            layer
        );
    }
}