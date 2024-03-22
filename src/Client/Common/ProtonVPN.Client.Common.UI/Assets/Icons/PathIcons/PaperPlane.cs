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
    protected override string IconGeometry16 { get; }
        = "M12.672 2.636 2.662 5.98l4.102 2.558 5.908-5.9ZM7.467 9.248l2.564 4.094 3.32-9.971-5.884 5.877Zm6.868-8.221a.505.505 0 0 1 .638.638l-4.325 12.99a.505.505 0 0 1-.907.108L6.586 9.724a1.011 1.011 0 0 0-.322-.32L1.238 6.27a.503.503 0 0 1 .107-.905l12.99-4.338Z";

    protected override string IconGeometry20 { get; }
        = "M15.84 3.295 3.327 7.474l5.128 3.197 7.385-7.376ZM9.334 11.56l3.205 5.117 4.15-12.463-7.355 7.346Zm8.584-10.277a.631.631 0 0 1 .799.798L13.31 18.32a.63.63 0 0 1-1.133.133l-3.945-6.297a1.265 1.265 0 0 0-.402-.402L1.547 7.838a.63.63 0 0 1 .134-1.132l16.237-5.423Z"; 

    protected override string IconGeometry24 { get; }
        = "M19.008 3.954 3.992 8.97l6.154 3.836 8.862-8.85ZM11.2 13.872l3.847 6.14 4.98-14.956-8.827 8.816ZM21.502 1.54a.758.758 0 0 1 .958.957l-6.488 19.487a.757.757 0 0 1-1.36.16l-4.734-7.557a1.518 1.518 0 0 0-.481-.482l-7.54-4.7a.755.755 0 0 1 .16-1.358L21.502 1.54Z"; 

    protected override string IconGeometry32 { get; }
        = "m25.343 5.272-20.02 6.686 8.204 5.115 11.816-11.8Zm-10.41 13.223 5.13 8.189 6.64-19.942-11.77 11.753ZM28.67 2.053a1.01 1.01 0 0 1 1.278 1.276l-8.651 25.982c-.27.809-1.36.938-1.813.214L13.17 19.45a2.022 2.022 0 0 0-.642-.642L2.475 12.54c-.724-.452-.595-1.542.214-1.81l25.98-8.677Z";
}