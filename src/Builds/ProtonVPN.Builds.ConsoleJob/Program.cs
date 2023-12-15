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

using System.Reflection;
using System.Text;
using ProtonVPN.Builds.Variables;
using ProtonVPN.Common.OS.EnvironmentVariables;

namespace ProtonVPN.Builds.ConsoleJob;

public static class Program
{
    private static readonly Type _globalConfigType = typeof(GlobalConfig);

    public static void Main(string[] args)
    {
        Console.WriteLine($"Started {typeof(Program).Namespace}");
        Execute();
        Console.WriteLine($"Finished {typeof(Program).Namespace}");
    }

    private static void Execute()
    {
        string fullFilePath = Path.Combine(GetRootDirectory(),
            "src", "Builds", _globalConfigType.Namespace, $"{_globalConfigType.Name}.cs");

        if (File.Exists(fullFilePath))
        {
            Console.WriteLine($"Overwriting the existing file {fullFilePath}");
            File.WriteAllText(fullFilePath, GetFileContents(), Encoding.UTF8);
        }
        else
        {
            string errorMessage = $"Could not find the file {fullFilePath}";
            Console.WriteLine(errorMessage);
            throw new Exception(errorMessage);
        }
    }

    private static string GetRootDirectory()
    {
        return EnvironmentVariableLoader.GetOrNull("CI_PROJECT_DIR") ??
            Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\"));
    }

    private static string GetFileContents()
    {
        string sentryDsn = GetEnvironmentVariable("SENTRY_DSN_V2");
        string internalReleaseUrl = GetEnvironmentVariable("INTERNAL_RELEASE_URL");
        string btiApiDomain = GetEnvironmentVariable("BTI_API_DOMAIN");
        string btiApiTlsPinningPublicKeyHashes = GetEnvironmentVariable("BTI_API_TLS_PINNINGS");
        string btiAlternativeRoutingTlsPinningPublicKeyHashes = GetEnvironmentVariable("BTI_ALT_ROUTE_TLS_PINNINGS");
        string btiCertificateValidation = GetEnvironmentVariable("BTI_CERT_VALIDATION");
        string btiDohUrls = GetEnvironmentVariable("BTI_DOH_URLS");
        string btiServerSignaturePublicKey = GetEnvironmentVariable("BTI_SERVER_SIGNATURE_PUBLIC_KEY");

        return $@"
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

namespace ProtonVPN.Builds.Variables;

public static class {_globalConfigType.Name}
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
";
    }

    private static string GetEnvironmentVariable(string variable)
    {
        return EnvironmentVariableLoader.GetOrNull(variable) ?? string.Empty;
    }
}
