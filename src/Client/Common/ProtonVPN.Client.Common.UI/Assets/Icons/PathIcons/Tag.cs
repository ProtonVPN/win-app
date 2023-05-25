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

public class Tag : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "m9.425 14.375 4.95-4.95L7.95 3.002H3v4.95l6.425 6.424Zm-.707.707a1 1 0 0 0 1.414 0l4.95-4.95a1 1 0 0 0 0-1.414L8.657 2.294A1 1 0 0 0 7.95 2H3a1 1 0 0 0-1 1v4.95a1 1 0 0 0 .293.707l6.425 6.425ZM5.5 6.5a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z";
}