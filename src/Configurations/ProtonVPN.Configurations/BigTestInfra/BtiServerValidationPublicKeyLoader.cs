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
using ProtonVPN.Common.Core.OperatingSystems.EnvironmentVariables;

namespace ProtonVPN.Configurations.BigTestInfra;

public class BtiServerValidationPublicKeyLoader
{
    public static string? Get(object? defaultValue)
    {
        return GetServerValidationPublicKey() ?? (string?)defaultValue;
    }

    private static string? GetServerValidationPublicKey()
    {
        string? btiServerValidationPublicKey = EnvironmentVariableLoader.GetOrNull("BTI_SERVER_SIGNATURE_PUBLIC_KEY");
        if (string.IsNullOrWhiteSpace(btiServerValidationPublicKey))
        {
            btiServerValidationPublicKey = GlobalConfig.BtiServerSignaturePublicKey;
            if (!string.IsNullOrWhiteSpace(btiServerValidationPublicKey))
            {
                return btiServerValidationPublicKey;
            }
        }
        else
        {
            return btiServerValidationPublicKey;
        }
        return null;
    }
}