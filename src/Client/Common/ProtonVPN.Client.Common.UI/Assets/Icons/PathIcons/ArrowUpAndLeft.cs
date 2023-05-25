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

public class ArrowUpAndLeft : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "m2.64 6.998 3.206-3.141a.5.5 0 1 0-.7-.714L1.21 6.999a.7.7 0 0 0 0 1l3.936 3.858a.5.5 0 0 0 .7-.714L2.64 7.998h9.858a1.5 1.5 0 0 1 1.5 1.5V12a.5.5 0 1 0 1 0V9.498a2.5 2.5 0 0 0-2.5-2.5H2.639Z";
}