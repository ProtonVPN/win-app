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

    private bool _isActive;
    private Callout _connectRedirectCalloutV4;
    private Callout _bindRedirectCalloutV4;
    private ProviderContext _providerContextV4;
    private Callout _connectRedirectCalloutV6;
    private Callout _bindRedirectCalloutV6;
    private ProviderContext _providerContextV6;
    private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Guid>> _appFilterIds = new(StringComparer.OrdinalIgnoreCase);

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

    public void AddAppPathsDynamically(string[] apps)
    {
        if (!_isActive || _ipFilter == null || _subLayer == null) return;

        _ipFilter.Session.StartTransaction();
        try
        {
            CreateAppCalloutFilters(apps, _bindRedirectCalloutV4, Layer.BindRedirectV4, _providerContextV4);
            CreateAppCalloutFilters(apps, _connectRedirectCalloutV4, Layer.AppConnectRedirectV4, _providerContextV4);

            if (_bindRedirectCalloutV6 != null && _connectRedirectCalloutV6 != null && _providerContextV6 != null)
            {
                CreateAppCalloutFilters(apps, _connectRedirectCalloutV6, Layer.AppConnectRedirectV6, _providerContextV6);
                CreateAppCalloutFilters(apps, _bindRedirectCalloutV6, Layer.BindRedirectV6, _providerContextV6);
            }

            _ipFilter.Session.CommitTransaction();
        }
        catch
        {
            _ipFilter.Session.AbortTransaction();
            throw;
        }
    }

    public void RemoveAppPathsDynamically(string[] apps)
    {
        if (!_isActive || _ipFilter == null || _subLayer == null) return;

        _ipFilter.Session.StartTransaction();
        try
        {
            foreach (string app in apps)
            {
                if (_appFilterIds.TryGetValue(app, out System.Collections.Generic.List<Guid> filterIds))
                {
                    foreach (Guid filterId in filterIds)
                    {
                        _subLayer.DestroyFilter(filterId);
                    }
                    _appFilterIds.Remove(app);
                }
            }
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
        _connectRedirectCalloutV4 = CreateConnectRedirectCallout(Layer.AppConnectRedirectV4, _connectRedirectV4CalloutKey);
        _bindRedirectCalloutV4 = CreateUDPRedirectCallout(Layer.BindRedirectV4, _bindRedirectV4CalloutKey);

        _providerContextV4 = GetProviderContext(ipv4Address);
        CreateAppCalloutFilters(apps, _bindRedirectCalloutV4, Layer.BindRedirectV4, _providerContextV4);
        CreateAppCalloutFilters(apps, _connectRedirectCalloutV4, Layer.AppConnectRedirectV4, _providerContextV4);

        if (ipv6Address is not null)
        {
            _providerContextV6 = GetProviderContext(ipv6Address);
            _connectRedirectCalloutV6 = CreateConnectRedirectCallout(Layer.AppConnectRedirectV6, _connectRedirectV6CalloutKey);
            _bindRedirectCalloutV6 = CreateUDPRedirectCallout(Layer.BindRedirectV6, _bindRedirectV6CalloutKey);

            CreateAppCalloutFilters(apps, _connectRedirectCalloutV6, Layer.AppConnectRedirectV6, _providerContextV6);
            CreateAppCalloutFilters(apps, _bindRedirectCalloutV6, Layer.BindRedirectV6, _providerContextV6);
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
        _appFilterIds.Clear();
        _isActive = true;

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

        _isActive = false;
        _appFilterIds.Clear();
        _connectRedirectCalloutV4 = null;
        _bindRedirectCalloutV4 = null;
        _providerContextV4 = null;
        _connectRedirectCalloutV6 = null;
        _bindRedirectCalloutV6 = null;
        _providerContextV6 = null;
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
            Guid filterId = CreateAppFilter(app, callout, layer, providerContext);

            if (!_appFilterIds.TryGetValue(app, out System.Collections.Generic.List<Guid> ids))
            {
                ids = new System.Collections.Generic.List<Guid>();
                _appFilterIds[app] = ids;
            }
            
            ids.Add(filterId);
        }
        catch (NetworkFilterException)
        {
        }
    }

    private Guid CreateAppFilter(string app, Callout callout, Layer layer, ProviderContext providerContext)
    {
        return _subLayer.CreateAppCalloutFilter(
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