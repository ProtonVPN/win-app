/*
 * Copyright (c) 2022 Proton Technologies AG
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
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Handlers;
using ProtonVPN.Core.Update;
using ProtonVPN.Update.Config;
using Module = Autofac.Module;

namespace ProtonVPN.Core.Ioc
{
    public class UpdateModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<FeedUrlProvider>().AsImplementedInterfaces().SingleInstance();
            builder.Register(c => new DefaultAppUpdateConfig
                {
                    HttpClient =
                        c.Resolve<IHttpClients>().Client(c.Resolve<RetryingHandler>(),
                            c.Resolve<IApiAppVersion>().UserAgent()),
                    FeedUriProvider = c.Resolve<IFeedUrlProvider>(),
                    UpdatesPath = c.Resolve<Common.Configuration.Config>().UpdatesPath,
                    CurrentVersion = Version.Parse(c.Resolve<Common.Configuration.Config>().AppVersion),
                    EarlyAccessCategoryName = "EarlyAccess",
                    MinProgressDuration = TimeSpan.FromSeconds(1.5)
                })
                .As<IAppUpdateConfig>().SingleInstance();
        }
    }
}