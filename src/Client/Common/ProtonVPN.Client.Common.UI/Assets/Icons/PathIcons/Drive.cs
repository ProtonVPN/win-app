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

public class Drive : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M14 12v-2a1 1 0 0 0-1-1H3a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1Zm1 0V9.41a2 2 0 0 0-.162-.787l-2.318-5.41A2 2 0 0 0 10.68 2H5.32a2 2 0 0 0-1.84 1.212l-2.32 5.41a2 2 0 0 0-.16.79V12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2Zm-3.4-8.394 1.912 4.46A2.004 2.004 0 0 0 13 8H3c-.177 0-.348.023-.512.066L4.4 3.606A1 1 0 0 1 5.319 3h5.362a1 1 0 0 1 .92.606Zm.9 7.894a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1Z";
}