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

public class PauseFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M4 4a1 1 0 0 1 1-1h1a1 1 0 0 1 1 1v8c0 .5-.5 1-1 1H5c-.5 0-1-.448-1-1V4Z M9 4c0-.5.5-1 1-1h1c.5 0 1 .5 1 1v8c0 .5-.5 1-1 1h-1c-.5 0-1-.5-1-1V4Z";
}