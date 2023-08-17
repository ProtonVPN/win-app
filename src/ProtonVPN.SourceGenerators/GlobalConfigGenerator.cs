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
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ProtonVPN.SourceGenerators
{
    [Generator]
    public class GlobalConfigGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            string sentryDsn = GetEnvironmentVariableOrNull("SENTRY_DSN_V2");
            string internalReleaseUrl = GetEnvironmentVariableOrNull("INTERNAL_RELEASE_URL");
            string btiApiDomain = GetEnvironmentVariableOrNull("BTI_API_DOMAIN");
            string btiApiTlsPinningPublicKeyHashes = GetEnvironmentVariableOrNull("BTI_API_TLS_PINNINGS");
            string btiAlternativeRoutingTlsPinningPublicKeyHashes = GetEnvironmentVariableOrNull("BTI_ALT_ROUTE_TLS_PINNINGS");
            string btiCertificateValidation = GetEnvironmentVariableOrNull("BTI_CERT_VALIDATION");
            string btiDohUrls = GetEnvironmentVariableOrNull("BTI_DOH_URLS");
            string btiServerSignaturePublicKey = GetEnvironmentVariableOrNull("BTI_SERVER_SIGNATURE_PUBLIC_KEY");

            context.AddSource("GlobalConfig.g.cs", SourceText.From($@"
using System.Collections.Generic;

namespace ProtonVPN.Common.Configuration
{{
    public class GlobalConfig
    {{
        public const string SentryDsn = ""{sentryDsn}"";
        public const string InternalReleaseUpdateUrl = ""{internalReleaseUrl}"";
        public const string BtiApiDomain = ""{btiApiDomain}"";
        public const string BtiApiTlsPinningPublicKeyHashes = ""{btiApiTlsPinningPublicKeyHashes}"";
        public const string BtiAlternativeRoutingTlsPinningPublicKeyHashes = ""{btiAlternativeRoutingTlsPinningPublicKeyHashes}"";
        public const string BtiCertificateValidation = ""{btiCertificateValidation}"";
        public const string BtiDohProviders = ""{btiDohUrls}"";
        public const string BtiServerSignaturePublicKey = ""{btiServerSignaturePublicKey}"";
    }}
}}", Encoding.UTF8));
        }

        public static string GetEnvironmentVariableOrNull(string variable)
        {
            string result;
            try
            {
                result = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process) ??
                         Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User) ??
                         Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);
            }
            catch
            {
                result = null;
            }

            return result;
        }
    }
}