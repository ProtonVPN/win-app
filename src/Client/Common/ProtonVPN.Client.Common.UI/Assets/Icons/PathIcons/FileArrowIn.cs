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

public class FileArrowIn : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M4.5 2A1.5 1.5 0 0 0 3 3.5v9A1.5 1.5 0 0 0 4.5 14H9v1H4.5A2.5 2.5 0 0 1 2 12.5v-9A2.5 2.5 0 0 1 4.5 1h4.672a2.5 2.5 0 0 1 1.767.732l2.329 2.329A2.5 2.5 0 0 1 14 5.828V10h-1V6h-2.5A1.5 1.5 0 0 1 9 4.5V2H4.5Zm8.06 2.768c.072.071.135.149.19.232H10.5a.5.5 0 0 1-.5-.5V2.25c.083.054.16.118.232.19l2.329 2.328Zm-.707 4.378a.5.5 0 0 0-.707 0l-1.858 1.859a.7.7 0 0 0 0 .99l1.858 1.859a.5.5 0 0 0 .707-.708L10.707 12H15a.5.5 0 0 0 0-1h-4.293l1.146-1.146a.5.5 0 0 0 0-.708Z";
}