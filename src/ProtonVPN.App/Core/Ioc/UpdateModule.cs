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
using System.Net.Http;
using Autofac;
using ProtonVPN.Api;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Api.Handlers.Retries;
using ProtonVPN.Api.Handlers.StackBuilders;
using ProtonVPN.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Net.Http;
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
            builder.Register(CreateDefaultAppUpdateConfig).As<IAppUpdateConfig>().SingleInstance();
        }

        // TO DO: Refactor the code in order to delete this custom registration
        private DefaultAppUpdateConfig CreateDefaultAppUpdateConfig(IComponentContext c)
        {
            return new DefaultAppUpdateConfig
            {
                HttpClient = c.Resolve<IFileDownloadHttpClientFactory>().GetFileDownloadHttpClient(),
                FeedUriProvider = c.Resolve<IFeedUrlProvider>(),
                UpdatesPath = c.Resolve<IConfiguration>().UpdatesPath,
                CurrentVersion = Version.Parse(c.Resolve<IConfiguration>().AppVersion),
                EarlyAccessCategoryName = "EarlyAccess",
                MinProgressDuration = TimeSpan.FromSeconds(1.5)
            };
        }
    }
}