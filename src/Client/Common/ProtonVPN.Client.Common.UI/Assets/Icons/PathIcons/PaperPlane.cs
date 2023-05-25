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

public class PaperPlane : CustomPathIcon
{
    protected override string IconGeometry { get; }
        = "M12.672 2.636 2.662 5.98l4.102 2.558 5.908-5.9ZM7.467 9.248l2.564 4.094 3.32-9.971-5.884 5.877Zm6.868-8.221a.505.505 0 0 1 .638.638l-4.325 12.99a.505.505 0 0 1-.907.108L6.586 9.724a1.011 1.011 0 0 0-.322-.32L1.238 6.27a.503.503 0 0 1 .107-.905l12.99-4.338Z";
}