/*
 * Copyright (c) 2023 Proton AG
 * Extended to support unified Proton Suite (VPN, Mail, Drive, Pass)
 */

using Autofac;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Mail;
using ProtonVPN.Api.Contracts.Drive;
using ProtonVPN.Api.Contracts.Pass;
using ProtonVPN.Api.Deserializers;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Api.Handlers.Retries;
using ProtonVPN.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Legacy.OS.Net.Http;

namespace ProtonVPN.Api.Installers
{
    public class ApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // ── Shared infrastructure ──────────────────────────────
            builder.RegisterType<TokenHttpClientFactory>()
                   .AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<ApiHttpClientFactory>()
                   .AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<SleepDurationProvider>().SingleInstance();
            builder.RegisterType<RetryPolicyProvider>()
                   .As<IRetryPolicyProvider>().SingleInstance();
            builder.RegisterType<RetryCountProvider>()
                   .As<IRetryCountProvider>().SingleInstance();
            builder.RegisterType<RequestTimeoutProvider>()
                   .As<IRequestTimeoutProvider>().SingleInstance();
            builder.RegisterType<BaseResponseMessageDeserializer>()
                   .As<IBaseResponseMessageDeserializer>().SingleInstance();
            builder.RegisterType<ApiHostProvider>()
                   .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CertificateValidator>()
                   .As<ICertificateValidator>().SingleInstance();
            builder.RegisterType<ApiAppVersion>()
                   .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TokenClient>()
                   .As<ITokenClient>().SingleInstance();
            builder.RegisterType<ReportClientUriProvider>()
                   .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ApiAvailabilityVerifier>()
                   .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<HttpClients>()
                   .As<IHttpClients>().SingleInstance();
            builder.RegisterType<HumanVerificationHttpClientFactory>()
                   .AsImplementedInterfaces().SingleInstance();

            // ── Report client (cached) ─────────────────────────────
            builder.Register(c =>
                    new CachingReportClient(
                        new ReportClient(c.Resolve<IReportClientUriProvider>())))
                .As<IReportClient>()
                .SingleInstance();

            // ── Per-app API clients ────────────────────────────────
            RegisterAppClients(builder);

            // ── HTTP handlers ──────────────────────────────────────
            RegisterHandlers(builder);

            // ── Per-app HTTP client factories ──────────────────────
            RegisterAppHttpClientFactories(builder);
        }

        // ── NEW: Register one API client per Proton app ────────────
        private void RegisterAppClients(ContainerBuilder builder)
        {
            // Proton VPN (existing)
            builder.RegisterType<ApiClient>()
                   .As<IApiClient>()
                   .SingleInstance();

            // Proton Mail
            builder.RegisterType<MailApiClient>()
                   .As<IMailApiClient>()
                   .SingleInstance();

            // Proton Drive
            builder.RegisterType<DriveApiClient>()
                   .As<IDriveApiClient>()
                   .SingleInstance();

            // Proton Pass
            builder.RegisterType<PassApiClient>()
                   .As<IPassApiClient>()
                   .SingleInstance();
        }

        // ── NEW: Separate HTTP client factories per app ────────────
        private void RegisterAppHttpClientFactories(ContainerBuilder builder)
        {
            // VPN (existing)
            builder.RegisterType<FileDownloadHttpClientFactory>()
                   .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UpdateHttpClientFactory>()
                   .AsImplementedInterfaces().SingleInstance();

            // Mail, Drive, Pass get their own factories
            // so their traffic is isolated and identifiable
            builder.RegisterType<MailHttpClientFactory>()
                   .As<IMailHttpClientFactory>().SingleInstance();
            builder.RegisterType<DriveHttpClientFactory>()
                   .As<IDriveHttpClientFactory>().SingleInstance();
            builder.RegisterType<PassHttpClientFactory>()
                   .As<IPassHttpClientFactory>().SingleInstance();
        }

        // ── Existing: HTTP handler stack ───────────────────────────
        // Each app gets its own handler instances (InstancePerDependency)
        // so their retry/logging stacks don't interfere with each other
        private void RegisterHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<RetryingHandler>()
                   .As<RetryingHandlerBase>().AsSelf().InstancePerDependency();
            builder.RegisterType<LoggingHandler>()
                   .As<LoggingHandlerBase>().AsSelf().InstancePerDependency();
            builder.RegisterType<HumanVerificationHandler>()
                   .As<HumanVerificationHandlerBase>().AsSelf().InstancePerDependency();
            builder.RegisterType<CancellingHandler>()
                   .As<CancellingHandlerBase>().AsSelf().InstancePerDependency();

            builder.RegisterType<AlternativeHostHandler>()
                   .AsImplementedInterfaces().AsSelf().InstancePerDependency();
            builder.RegisterType<DnsHandler>().InstancePerDependency();
            builder.RegisterType<OutdatedAppHandler>().InstancePerDependency();
            builder.RegisterType<UnauthorizedResponseHandler>().InstancePerDependency();
            builder.RegisterType<TlsPinnedCertificateHandler>().InstancePerDependency();
            builder.RegisterType<SslCertificateHandler>().InstancePerDependency();
        }
    }
}