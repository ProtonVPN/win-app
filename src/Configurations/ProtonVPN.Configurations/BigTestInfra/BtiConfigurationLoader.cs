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
using ProtonVPN.Configurations.Contracts;

namespace ProtonVPN.Configurations.BigTestInfra;

public static class BtiConfigurationLoader
{
    public static object? GetValue(PropertyInfo property, object? defaultValue)
    {
        return property.Name switch
        {
            nameof(IConfiguration.Urls) => BtiUrlsLoader.Get(defaultValue),
            nameof(IConfiguration.TlsPinning) => BtiTlsPinningLoader.Get(defaultValue),
            nameof(IConfiguration.IsCertificateValidationEnabled) => BtiIsCertificateValidationEnabledLoader.Get(defaultValue),
            nameof(IConfiguration.DohProviders) => BtiDohProvidersLoader.Get(defaultValue),
            nameof(IConfiguration.ServerValidationPublicKey) => BtiServerValidationPublicKeyLoader.Get(defaultValue),
            _ => defaultValue,
        };
    }
}