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

using ProtonVPN.Client.Logic.Servers.Contracts.Models;

namespace ProtonVPN.Client.Logic.Servers.Cache;

public interface IServersCache
{
    IReadOnlyList<Server> Servers { get; }
    IReadOnlyList<FreeCountry> FreeCountries { get; }
    IReadOnlyList<Country> Countries { get; }
    IReadOnlyList<State> States { get; }
    IReadOnlyList<City> Cities { get; }
    IReadOnlyList<Gateway> Gateways { get; }
    IReadOnlyList<SecureCoreCountryPair> SecureCoreCountryPairs { get; }

    bool IsEmpty();
    bool IsStale();
    bool IsOutdated();
    bool IsLoadOutdated();

    void Clear();
    void LoadFromFileIfEmpty();

    Task UpdateAsync(CancellationToken cancellationToken);
    Task UpdateLoadsAsync(CancellationToken cancellationToken);
}