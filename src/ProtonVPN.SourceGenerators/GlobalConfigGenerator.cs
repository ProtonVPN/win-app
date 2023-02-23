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
            string sentryDsn = Environment.GetEnvironmentVariable("SENTRY_DSN_V2");
            string internalReleaseUrl = Environment.GetEnvironmentVariable("INTERNAL_RELEASE_URL");
            string internalReleaseOldUrl = Environment.GetEnvironmentVariable("INTERNAL_RELEASE_OLD_URL");

            context.AddSource("GlobalConfig.g.cs", SourceText.From($@"
namespace ProtonVPN.Common.Configuration
{{
    public class GlobalConfig
    {{
        public const string SentryDsn = ""{sentryDsn}"";
        public const string InternalReleaseUpdateUrl = ""{internalReleaseUrl}"";
        public const string InternalReleaseOldUpdateUrl = ""{internalReleaseOldUrl}"";
    }}
}}", Encoding.UTF8));
        }
    }
}