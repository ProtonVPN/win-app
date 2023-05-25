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

public class BrandProtonMail : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M2.61 2.71a1 1 0 0 0-1.612.792v7.995a2 2 0 0 0 2 2h10.004a2 2 0 0 0 2-2V3.502a1 1 0 0 0-1.611-.791L8.306 6.643a.5.5 0 0 1-.612 0L2.61 2.71Zm-.612.792 5.084 3.932c.18.139.383.231.595.278l-.518.463a.9.9 0 0 1-1.165.03L1.998 4.987V3.502Zm11.004 8.995h-.817v-7.59l1.817-1.405v7.995a1 1 0 0 1-1 1Z";
}