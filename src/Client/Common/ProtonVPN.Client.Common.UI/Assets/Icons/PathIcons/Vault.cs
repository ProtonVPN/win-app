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

using ProtonVPN.Client.Common.UI.Assets.Icons.Base;

namespace ProtonVPN.Client.Common.UI.Assets.Icons.PathIcons;

public class Vault : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M3 3h12v10H3V3ZM2 3a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1h-.5a.5.5 0 0 1 0-1H2V4h-.5a.5.5 0 0 1 0-1H2Zm9 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm0 1a3 3 0 1 0 0-6 3 3 0 0 0 0 6Zm1-3a1 1 0 1 1-2 0 1 1 0 0 1 2 0Zm-6-.134a1 1 0 1 0-1 0V9.5a.5.5 0 0 0 1 0V7.866Z";
}