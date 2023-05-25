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

public class Lightbulb : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M11.025 8.618a4 4 0 1 0-6.05 0c.53.611 1.127 1.416 1.389 2.382h3.272c.262-.966.859-1.77 1.389-2.382ZM5.5 12c0-1.033-.604-1.947-1.28-2.727a5 5 0 1 1 7.56 0c-.676.78-1.28 1.694-1.28 2.727v.5c0 .822-.303 1.464-.796 1.894C9.222 14.815 8.598 15 8 15c-.598 0-1.222-.185-1.704-.606-.493-.43-.796-1.072-.796-1.894V12Zm1 .5V12h3v.5c0 .559-.197.917-.454 1.141C8.778 13.875 8.402 14 8 14s-.778-.125-1.046-.359c-.257-.224-.454-.582-.454-1.141Zm-.5-7A1.5 1.5 0 0 1 7.5 4a.5.5 0 0 0 0-1A2.5 2.5 0 0 0 5 5.5a.5.5 0 0 0 1 0Z";
}