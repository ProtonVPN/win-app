/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software:
 you can redistribute it and/or modify
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Announcements.Installers;
using ProtonVPN.Announcements.Tests;
using ProtonVPN.Api;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Installers;
using ProtonVPN.BugReporting;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Ioc;
using ProtonVPN.HumanVerification.Installers;
using ProtonVPN.P2PDetection;
using RichardSzalay.MockHttp;
using File = System.IO.File;

namespace ProtonVPN.IntegrationTests
{
    public class TestBase
    {
        private IContainer _container;
        protected MockHttpMessageHandler MessageHandler;

        public T Resolve<T>() => _container.Resolve<T>();

        [TestInitialize]
        public void Initialize()
        {
            MessageHandler = new();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _container?.Dispose();
            _container = null;
            MessageHandler = null;
        }

        protected void InitializeContainer()
        {
            HttpClient httpClient = MessageHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://localhost");
            ContainerBuilder builder = new();

            builder.RegisterAssemblyModules<ApiModule>(typeof(ApiModule).Assembly);
            builder.Register(_ =>
            {
                IApiHttpClientFactory httpClientFactory = Substitute.For<IApiHttpClientFactory>();
                httpClientFactory.GetApiHttpClientWithCache().Returns(httpClient);
                httpClientFactory.GetApiHttpClientWithoutCache().Returns(httpClient);
                return httpClientFactory;
            }).As<IApiHttpClientFactory>().SingleInstance();

            builder.RegisterModule<CoreModule>()
                .RegisterModule<UiModule>()
                .RegisterModule<AppModule>()
                .RegisterModule<BugReportingModule>()
                .RegisterModule<LoginModule>()
                .RegisterModule<P2PDetectionModule>()
                .RegisterModule<ProfilesModule>()
                .RegisterModule<UpdateModule>()
                .RegisterAssemblyModules<HumanVerificationModule>(typeof(HumanVerificationModule).Assembly);

            builder.Register(_ =>
            {
                IFileDownloadHttpClientFactory httpClients = Substitute.For<IFileDownloadHttpClientFactory>();
                httpClients.GetFileDownloadHttpClient().Returns(new WrappedHttpClient(httpClient));
                return httpClients;
            }).As<IFileDownloadHttpClientFactory>().SingleInstance();
            builder.RegisterAssemblyModules<AnnouncementsModule>(typeof(AnnouncementsModule).Assembly);

            builder.Register(_ => Substitute.For<IScheduler>()).As<IScheduler>().SingleInstance();
            builder.Register(_ => new AnnouncementCacheMock()).As<IAnnouncementCache>().SingleInstance();

            new Update.Config.Module().Load(builder);

            _container = builder.Build();
        }

        protected string GetJsonMock(string name)
        {
            return File.ReadAllText($"TestData\\{name}.json");
        }
    }
}