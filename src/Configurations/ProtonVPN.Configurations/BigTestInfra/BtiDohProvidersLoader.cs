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

using ProtonVPN.Builds.Variables;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.OperatingSystems.EnvironmentVariables;

namespace ProtonVPN.Configurations.BigTestInfra;

public class BtiDohProvidersLoader
{
    public static IList<string> Get(object? defaultValue)
    {
        IList<string> absoluteUris = GetDohProviderAbsoluteUris();
        return absoluteUris is not null && absoluteUris.Count > 0 
            ? absoluteUris 
            : (defaultValue is not null && defaultValue is IList<string> dohp ? dohp : new List<string>());
    }

    private static IList<string> GetDohProviderAbsoluteUris()
    {
        IList<string> urls = GetDohProviderUrls();

        List<string> absoluteUris = new();
        foreach (string url in urls)
        {
            if (url.IsHttpUri(out Uri uri))
            {
                absoluteUris.Add(uri.AbsoluteUri);
            }
        }

        return absoluteUris;
    }

    private static IList<string> GetDohProviderUrls()
    {
        List<string>? dohProviders = EnvironmentVariableLoader.GetOrNull("BTI_DOH_URLS")?.SplitToList(',');
        if (dohProviders is null || dohProviders.Count == 0)
        {
            dohProviders = GlobalConfig.BtiDohProviders.SplitToList(',');
            if (dohProviders is not null && dohProviders.Count > 0)
            {
                return dohProviders;
            }
        }
        else
        {
            return dohProviders;
        }
        return new List<string>();
    }
}