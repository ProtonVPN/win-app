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

public class InfoCircleFilled : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M8.5 16a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Zm.8-10.6a.9.9 0 1 1-1.8 0 .9.9 0 0 1 1.8 0ZM7.25 7.5a.5.5 0 0 1 .5-.5H9v4h.5a.5.5 0 0 1 0 1h-2a.5.5 0 0 1 0-1H8V8h-.25a.5.5 0 0 1-.5-.5Z";
}