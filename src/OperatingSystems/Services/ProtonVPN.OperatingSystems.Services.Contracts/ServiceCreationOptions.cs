/*
 * Copyright (c) 2025 Proton AG
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

namespace ProtonVPN.OperatingSystems.Services.Contracts;

public sealed class ServiceCreationOptions
{
    public string PathAndArguments { get; }

    public bool IsUnrestricted { get; }

    public IReadOnlyList<string> Dependencies { get; }

    public ServiceCreationOptions(string pathAndArguments, bool isUnrestricted, IEnumerable<string>? dependencies = null)
    {
        PathAndArguments = pathAndArguments ?? throw new ArgumentNullException(nameof(pathAndArguments));
        IsUnrestricted = isUnrestricted;
        Dependencies = dependencies is null ? Array.Empty<string>() : new List<string>(dependencies);
    }
}