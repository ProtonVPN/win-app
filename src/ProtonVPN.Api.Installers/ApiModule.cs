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

using Autofac;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Deserializers;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Api.Handlers.Retries;

namespace ProtonVPN.Api.Installers
{
    public class ApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ApiClient>().As<IApiClient>().SingleInstance();
            builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>().SingleInstance();
            builder.RegisterType<CertificateHandler>().SingleInstance();
            builder.RegisterType<SafeDnsHandler>().SingleInstance();
            builder.RegisterType<LoggingHandler>().As<LoggingHandlerBase>().AsSelf().SingleInstance();
            builder.RegisterType<SleepDurationProvider>().SingleInstance();
            builder.RegisterType<RetryingHandler>().As<RetryingHandlerBase>().AsSelf().SingleInstance();
            builder.RegisterType<OutdatedAppHandler>().SingleInstance();
            builder.RegisterType<HumanVerificationHandler>().As<HumanVerificationHandlerBase>().AsSelf().SingleInstance();
            builder.RegisterType<UnauthorizedResponseHandler>().SingleInstance();
            builder.RegisterType<CancellingHandler>().As<CancellingHandlerBase>().AsSelf().SingleInstance();
            builder.RegisterType<RetryPolicyProvider>().As<IRetryPolicyProvider>().SingleInstance();
            builder.RegisterType<RetryCountProvider>().As<IRetryCountProvider>().SingleInstance();
            builder.RegisterType<RequestTimeoutProvider>().As<IRequestTimeoutProvider>().SingleInstance();
            builder.RegisterType<AlternativeHostHandler>().AsImplementedInterfaces().AsSelf().SingleInstance();
            builder.RegisterType<BaseResponseMessageDeserializer>().As<IBaseResponseMessageDeserializer>().SingleInstance();
        }
    }
}