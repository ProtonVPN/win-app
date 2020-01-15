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
using System.Net;
using ProtonVPN.NetworkFilter;

namespace ProtonVPN.Service.SplitTunneling
{
    public class SplitTunnelNetworkFilters
    {
        private const uint WfpSubLayerWeight = 7;
        private static readonly Guid WfpCalloutKey = Guid.Parse("{3c5a284f-af01-51fa-4361-6c6c50424144}");

        private IpFilter _ipFilter;
        private Sublayer _subLayer;

        public void EnableExcludeMode(IEnumerable<string> apps, IPAddress internetLocalIp)
        {
            Create();

            _ipFilter.Session.StartTransaction();
            try
            {
                var callout = _ipFilter.CreateCallout(
                    new DisplayData
                    {
                        Name = "ProtonVPN Split Tunnel callout",
                        Description = "Redirects network connections",
                    },
                    WfpCalloutKey,
                    Layer.BindRedirectV4
                );

                var providerContext = _ipFilter.CreateProviderContext(
                    new DisplayData
                    {
                        Name = "ProtonVPN Split Tunnel redirect context",
                        Description = "Instructs the callout driver where to redirect network connections",
                    },
                    new BindingRedirectData(internetLocalIp));

                CreateAppFilters(apps, callout, providerContext);

                _ipFilter.Session.CommitTransaction();
            }
            catch
            {
                _ipFilter.Session.AbortTransaction();
                throw;
            }
        }

        public void EnableIncludeMode(IEnumerable<string> apps, IPAddress internetLocalIp, IPAddress vpnLocalIp)
        {
            Create();

            _ipFilter.Session.StartTransaction();
            try
            {
                var callout = _ipFilter.CreateCallout(
                    new DisplayData
                    {
                        Name = "ProtonVPN Split Tunnel callout",
                        Description = "Redirects network connections",
                    },
                    WfpCalloutKey,
                    Layer.BindRedirectV4
                );

                var providerContext = _ipFilter.CreateProviderContext(
                    new DisplayData
                    {
                        Name = "ProtonVPN Split Tunnel redirect context",
                        Description = "Instructs the callout driver where to redirect network connections",
                    },
                    new BindingRedirectData(internetLocalIp));

                _subLayer.CreateLayerCalloutFilter(
                        new DisplayData
                        {
                            Name = "ProtonVPN Split Tunnel redirect",
                            Description = "Redirects network connections"
                        },
                        Layer.BindRedirectV4,
                        14,
                        callout,
                        providerContext);

                providerContext = _ipFilter.CreateProviderContext(
                    new DisplayData
                    {
                        Name = "ProtonVPN Split Tunnel redirect context",
                        Description = "Instructs the callout driver where to redirect network connections",
                    },
                    new BindingRedirectData(vpnLocalIp));

                CreateAppFilters(apps, callout, providerContext);

                _ipFilter.Session.CommitTransaction();
            }
            catch
            {
                _ipFilter.Session.AbortTransaction();
                throw;
            }
        }

        public void Disable()
        {
            Remove();
        }

        private void Create()
        {
            _ipFilter = IpFilter.Create(
                Session.Dynamic(),
                new DisplayData { Name = "Proton Technologies AG", Description = "ProtonVPN Split Tunnel provider" });

            _subLayer = _ipFilter.CreateSublayer(
                new DisplayData { Name = "ProtonVPN Split Tunnel filters" },
                WfpSubLayerWeight);
        }

        private void Remove()
        {
            _ipFilter?.Session.Close();
            _ipFilter = null;
            _subLayer = null;
        }

        private void CreateAppFilters(IEnumerable<string> apps, Callout callout, ProviderContext providerContext)
        {
            foreach (var app in apps)
            {
                SafeCreateAppFilter(app, callout, providerContext);
            }
        }

        private void SafeCreateAppFilter(string app, Callout callout, ProviderContext providerContext)
        {
            try
            {
                CreateAppFilter(app, callout, providerContext);
            }
            catch (NetworkFilterException)
            {
            }
        }

        private void CreateAppFilter(string app, Callout callout, ProviderContext providerContext)
        {
            _subLayer.CreateAppCalloutFilter(
                new DisplayData
                {
                    Name = "ProtonVPN Split Tunnel redirect app",
                    Description = "Redirects network connections of the app"
                },
                Layer.BindRedirectV4,
                15,
                callout,
                providerContext,
                app);
        }
    }
}
