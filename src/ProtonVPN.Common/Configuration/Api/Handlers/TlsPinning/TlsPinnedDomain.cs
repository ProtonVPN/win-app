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

using System.Collections.Generic;

namespace ProtonVPN.Common.Configuration.Api.Handlers.TlsPinning
{
    public class TlsPinnedDomain
    {
        public string Name { get; internal set; }
        public IReadOnlyCollection<string> PublicKeyHashes { get; internal set; } = new HashSet<string>();
        public bool Enforce { get; internal set; }
        public bool SendReport { get; internal set; }
    }
}
